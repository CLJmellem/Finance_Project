using AutoMapper;
using MediatR;
using MongoDB.Bson;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Interfaces;

namespace TransactionsService.Application.Queries.FindTransactionById;

/// <summary>
/// Handles fetching a single transaction, validating card ownership before returning.
/// </summary>
public sealed class FindTransactionByIdQueryHandler(
    ITransactionRepository transactionRepository,
    ICardRepository cardRepository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<FindTransactionByIdQuery, TransactionResponse>
{
    public async Task<TransactionResponse> Handle(FindTransactionByIdQuery request, CancellationToken ct)
    {
        var userId = ResolveUserId(request.UserId);

        var cardObjectId = ObjectId.Parse(request.CardId);
        var userObjectId = ObjectId.Parse(userId);

        var card = await cardRepository.GetOneAsync(
            c => c.Id == cardObjectId && c.UserId == userObjectId, ct);

        if (card is null)
            throw new NotFoundException($"Card '{request.CardId}' not found.");

        if (!card.IsActive)
            throw new DomainException("Card is inactive.");

        var txObjectId = ObjectId.Parse(request.TransactionId);
        var transaction = await transactionRepository.GetOneAsync(
            t => t.Id == txObjectId && t.CardId == cardObjectId, ct);

        if (transaction is null)
            throw new NotFoundException($"Transaction '{request.TransactionId}' not found for card '{request.CardId}'.");

        return mapper.Map<TransactionResponse>(transaction);
    }

    private string ResolveUserId(string? requestUserId) =>
        !string.IsNullOrWhiteSpace(requestUserId) ? requestUserId : currentUser.UserId;
}
