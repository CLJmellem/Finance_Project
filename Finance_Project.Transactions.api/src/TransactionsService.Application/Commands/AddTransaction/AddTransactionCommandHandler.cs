using AutoMapper;
using MediatR;
using MongoDB.Bson;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Interfaces;
using TransactionsService.Domain.Entities;

namespace TransactionsService.Application.Commands.AddTransaction;

/// <summary>
/// Handles the creation of a new transaction for a card.
/// Validates card ownership, active status, and available credit before persisting.
/// </summary>
public sealed class AddTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    ICardRepository cardRepository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<AddTransactionCommand, TransactionResponse>
{
    public async Task<TransactionResponse> Handle(AddTransactionCommand request, CancellationToken ct)
    {
        // Resolve userId — if provided use it, otherwise fall back to the token's userId
        var userId = ResolveUserId(request.UserId);

        var cardObjectId = ObjectId.Parse(request.CardId);
        var userObjectId = ObjectId.Parse(userId);

        // Validate that the card exists and belongs to the user
        var card = await cardRepository.GetOneAsync(
            c => c.Id == cardObjectId && c.UserId == userObjectId, ct);

        if (card is null)
            throw new NotFoundException($"Card '{request.CardId}' not found.");

        if (!card.IsActive)
            throw new DomainException("Card is inactive and cannot receive new transactions until it is reactivated.");

        // Validate that TotalAmount alone does not exceed the card's credit limit
        if (request.TotalAmount > (double)card.CreditLimit)
            throw new DomainException("The transaction amount exceeds the card's credit limit.");

        // Validate that adding this transaction does not exceed available credit
        var usedCredit = await transactionRepository.GetUsedCreditAsync(request.CardId, ct);
        var availableCredit = (double)card.CreditLimit - usedCredit;

        if (request.TotalAmount > availableCredit)
            throw new DomainException($"Insufficient credit. Available: {availableCredit:F2}, Requested: {request.TotalAmount:F2}.");

        var entity = new TransactionsDataEntity
        {
            UserId = userObjectId,
            CardId = cardObjectId,
            TotalAmount = request.TotalAmount,
            MonthAmount = request.MonthAmount,
            Installments = request.Installments,
            // ActualInstallments starts equal to Installments (decremented monthly by background service)
            ActualInstallments = request.Installments.HasValue ? request.Installments.Value : null,
            Type = request.Type,
            Description = request.Description?.Trim(),
            IsRecurring = request.IsRecurring,
            CardDueDate = CalculateNextDueDate(card.DueDay)
        };

        await transactionRepository.CreateAsync(entity, ct);

        return mapper.Map<TransactionResponse>(entity);
    }

    private string ResolveUserId(string? requestUserId) =>
        !string.IsNullOrWhiteSpace(requestUserId) ? requestUserId : currentUser.UserId;

    /// <summary>
    /// Calculates the next billing due date from today using the card's DueDay.
    /// Always advances to the next month. Example: DueDay=10, today=March → April 10.
    /// </summary>
    private static DateTime CalculateNextDueDate(int dueDay)
    {
        var today = DateTime.UtcNow;
        var nextMonth = today.AddMonths(1);

        // Clamp day to the last valid day of the month (e.g. DueDay=31 in February → Feb 28/29)
        var lastDay = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
        var day = Math.Min(dueDay, lastDay);

        return new DateTime(nextMonth.Year, nextMonth.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}
