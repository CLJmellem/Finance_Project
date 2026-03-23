using AutoMapper;
using MediatR;
using MongoDB.Bson;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Interfaces;

namespace TransactionsService.Application.Commands.UpdateTransaction;

/// <summary>
/// Handles updating an existing transaction.
/// Re-validates card status and available credit after excluding the original transaction amount.
/// </summary>
public sealed class UpdateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    ICardRepository cardRepository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateTransactionCommand, TransactionResponse>
{
    public async Task<TransactionResponse> Handle(UpdateTransactionCommand request, CancellationToken ct)
    {
        var userId = ResolveUserId(request.UserId);

        var cardObjectId = ObjectId.Parse(request.CardId);
        var userObjectId = ObjectId.Parse(userId);

        // Validate card exists, belongs to user, and is active
        var card = await cardRepository.GetOneAsync(
            c => c.Id == cardObjectId && c.UserId == userObjectId, ct);

        if (card is null)
            throw new NotFoundException($"Card '{request.CardId}' not found.");

        if (!card.IsActive)
            throw new DomainException("Card is inactive and cannot be updated until it is reactivated.");

        // Validate the transaction exists and belongs to the card
        var txObjectId = ObjectId.Parse(request.TransactionId);
        var transaction = await transactionRepository.GetOneAsync(
            t => t.Id == txObjectId && t.CardId == cardObjectId, ct);

        if (transaction is null)
            throw new NotFoundException($"Transaction '{request.TransactionId}' not found for card '{request.CardId}'.");

        // Determine the new TotalAmount to validate (fall back to current if not provided)
        var newTotalAmount = request.TotalAmount ?? transaction.TotalAmount;

        if (newTotalAmount > (double)card.CreditLimit)
            throw new DomainException("The transaction amount exceeds the card's credit limit.");

        // Validate available credit excluding the current transaction's contribution
        var usedCreditWithoutThis = await transactionRepository.GetUsedCreditExcludingAsync(
            request.CardId, request.TransactionId, ct);

        var availableCredit = (double)card.CreditLimit - usedCreditWithoutThis;

        if (newTotalAmount > availableCredit)
            throw new DomainException($"Insufficient credit. Available: {availableCredit:F2}, Requested: {newTotalAmount:F2}.");

        // Apply only the fields that were provided (partial update)
        if (request.TotalAmount.HasValue) transaction.TotalAmount = request.TotalAmount.Value;
        if (request.MonthAmount.HasValue) transaction.MonthAmount = request.MonthAmount.Value;
        if (request.Installments.HasValue)
        {
            transaction.Installments = request.Installments.Value;
            // Reset remaining installments to new total when installments are updated
            transaction.ActualInstallments = request.Installments.Value;
        }
        if (request.Type.HasValue) transaction.Type = request.Type.Value;
        if (request.Description is not null) transaction.Description = request.Description.Trim();

        transaction.LastUpdated = DateTime.UtcNow;

        await transactionRepository.UpdateAsync(transaction, ct);

        return mapper.Map<TransactionResponse>(transaction);
    }

    private string ResolveUserId(string? requestUserId) =>
        !string.IsNullOrWhiteSpace(requestUserId) ? requestUserId : currentUser.UserId;
}
