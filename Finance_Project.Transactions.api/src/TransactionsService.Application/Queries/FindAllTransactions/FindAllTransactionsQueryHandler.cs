using AutoMapper;
using MediatR;
using MongoDB.Bson;
using System.Linq.Expressions;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Interfaces;
using TransactionsService.Domain.Entities;

namespace TransactionsService.Application.Queries.FindAllTransactions;

/// <summary>
/// Handles fetching transactions with optional filters.
/// Priority: CardId > CardName > LastFourDigits.
/// </summary>
public sealed class FindAllTransactionsQueryHandler(
    ITransactionRepository transactionRepository,
    ICardRepository cardRepository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<FindAllTransactionsQuery, PaginatedResponse<TransactionResponse>>
{
    public async Task<PaginatedResponse<TransactionResponse>> Handle(
        FindAllTransactionsQuery request, CancellationToken ct)
    {
        var userId = ResolveUserId(request.UserId);
        var userObjectId = ObjectId.Parse(userId);

        // Resolve card filters — priority: CardId > CardName > LastFourDigits
        string? resolvedCardId = request.CardId;

        if (!string.IsNullOrWhiteSpace(request.CardId))
        {
            // CardId explicitly provided — validate existence and active status
            var cardObjectId = ObjectId.Parse(request.CardId);
            var card = await cardRepository.GetOneAsync(
                c => c.Id == cardObjectId && c.UserId == userObjectId, ct);

            if (card is null)
                throw new NotFoundException($"Card '{request.CardId}' not found.");

            if (!card.IsActive)
                throw new DomainException("Card is inactive.");
        }
        else if (!string.IsNullOrWhiteSpace(request.CardName))
        {
            resolvedCardId = await TryResolveCardByNameAsync(request.CardName, userObjectId, ct);
            if (resolvedCardId is null)
                return EmptyPage(request);
        }
        else if (!string.IsNullOrWhiteSpace(request.LastFourDigits))
        {
            resolvedCardId = await TryResolveCardByLastFourAsync(request.LastFourDigits, userObjectId, ct);
            if (resolvedCardId is null)
                return EmptyPage(request);
        }

        // Build the filter expression
        Expression<Func<TransactionsDataEntity, bool>> filter = BuildFilter(
            userId, resolvedCardId, request.TransactionId);

        var transactions = await transactionRepository.GetAllAsync(
            request.Page, request.PageSize, filter, ct);

        var totalCount = await transactionRepository.CountAllAsync(filter, ct);

        // TransactionId explicitly provided but no results found
        if (!string.IsNullOrWhiteSpace(request.TransactionId) && totalCount == 0)
            throw new NotFoundException($"Transaction '{request.TransactionId}' not found.");

        return new PaginatedResponse<TransactionResponse>(
            Items: mapper.Map<List<TransactionResponse>>(transactions).AsReadOnly(),
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount);
    }

    private string ResolveUserId(string? requestUserId) =>
        !string.IsNullOrWhiteSpace(requestUserId) ? requestUserId : currentUser.UserId;

    // Returns null (empty list) when card not found; inactive card throws a business error
    private async Task<string?> TryResolveCardByNameAsync(string name, ObjectId userObjectId, CancellationToken ct)
    {
        var card = await cardRepository.GetOneAsync(
            c => c.Name == name && c.UserId == userObjectId, ct);

        if (card is null)
            return null;

        if (!card.IsActive)
            throw new DomainException($"Card '{name}' was found but is currently inactive.");

        return card.Id.ToString();
    }

    private async Task<string?> TryResolveCardByLastFourAsync(string lastFour, ObjectId userObjectId, CancellationToken ct)
    {
        var card = await cardRepository.GetOneAsync(
            c => c.LastFourDigits == lastFour && c.UserId == userObjectId, ct);

        if (card is null)
            return null;

        if (!card.IsActive)
            throw new DomainException($"Card ending in '{lastFour}' was found but is currently inactive.");

        return card.Id.ToString();
    }

    private static PaginatedResponse<TransactionResponse> EmptyPage(FindAllTransactionsQuery request) =>
        new(Items: Array.Empty<TransactionResponse>(), Page: request.Page, PageSize: request.PageSize, TotalCount: 0);

    private static Expression<Func<TransactionsDataEntity, bool>> BuildFilter(
        string userId, string? cardId, string? transactionId)
    {
        var userObjectId = ObjectId.Parse(userId);

        if (!string.IsNullOrWhiteSpace(cardId) && !string.IsNullOrWhiteSpace(transactionId))
        {
            var cardObjectId = ObjectId.Parse(cardId);
            var txObjectId = ObjectId.Parse(transactionId);
            return t => t.UserId == userObjectId && t.CardId == cardObjectId && t.Id == txObjectId;
        }

        if (!string.IsNullOrWhiteSpace(cardId))
        {
            var cardObjectId = ObjectId.Parse(cardId);
            return t => t.UserId == userObjectId && t.CardId == cardObjectId;
        }

        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            var txObjectId = ObjectId.Parse(transactionId);
            return t => t.UserId == userObjectId && t.Id == txObjectId;
        }

        return t => t.UserId == userObjectId;
    }
}
