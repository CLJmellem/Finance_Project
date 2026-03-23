using MediatR;

namespace TransactionsService.Application.Commands.RemoveTransaction;

/// <summary>
/// Command to physically delete a transaction.
/// </summary>
public sealed record RemoveTransactionCommand(
    string CardId,
    string TransactionId,
    string? UserId) : IRequest<Unit>;
