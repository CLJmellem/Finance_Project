using MediatR;
using MongoDB.Bson;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Interfaces;

namespace TransactionsService.Application.Commands.RemoveTransaction;

/// <summary>
/// Handles the physical deletion of a transaction.
/// Validates card ownership and active status before deleting.
/// </summary>
public sealed class RemoveTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    ICardRepository cardRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<RemoveTransactionCommand, Unit>
{
    public async Task<Unit> Handle(RemoveTransactionCommand request, CancellationToken ct)
    {
        var userId = ResolveUserId(request.UserId);

        var cardObjectId = ObjectId.Parse(request.CardId);
        var userObjectId = ObjectId.Parse(userId);

        var card = await cardRepository.GetOneAsync(
            c => c.Id == cardObjectId && c.UserId == userObjectId, ct);

        if (card is null)
            throw new NotFoundException($"Card '{request.CardId}' not found.");

        if (!card.IsActive)
            throw new DomainException("Card is inactive. Transactions cannot be removed from an inactive card.");

        var txObjectId = ObjectId.Parse(request.TransactionId);
        var transaction = await transactionRepository.GetOneAsync(
            t => t.Id == txObjectId && t.CardId == cardObjectId, ct);

        if (transaction is null)
            throw new NotFoundException($"Transaction '{request.TransactionId}' not found for card '{request.CardId}'.");

        await transactionRepository.DeleteAsync(transaction.Id.ToString(), ct);

        return Unit.Value;
    }

    private string ResolveUserId(string? requestUserId) =>
        !string.IsNullOrWhiteSpace(requestUserId) ? requestUserId : currentUser.UserId;
}
