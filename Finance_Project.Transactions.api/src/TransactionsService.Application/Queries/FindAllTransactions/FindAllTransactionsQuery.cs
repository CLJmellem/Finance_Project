using MediatR;
using TransactionsService.Application.DTOs;

namespace TransactionsService.Application.Queries.FindAllTransactions;

/// <summary>
/// Query to retrieve transactions with optional filters and pagination.
/// When page/pageSize are not provided (0), all results are returned without pagination.
/// </summary>
public sealed record FindAllTransactionsQuery(
    string? UserId,
    string? CardId,
    string? TransactionId,
    string? CardName,
    string? LastFourDigits,
    int Page,
    int PageSize) : IRequest<PaginatedResponse<TransactionResponse>>;
