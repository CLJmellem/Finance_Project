using MediatR;
using TransactionsService.Application.DTOs;

namespace TransactionsService.Application.Queries.FindTransactionById;

/// <summary>
/// Query to retrieve a single transaction by its ID and card ID.
/// </summary>
public sealed record FindTransactionByIdQuery(
    string CardId,
    string TransactionId,
    string? UserId) : IRequest<TransactionResponse>;
