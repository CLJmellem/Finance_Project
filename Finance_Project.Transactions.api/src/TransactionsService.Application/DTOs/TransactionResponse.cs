using TransactionsService.Domain.Enums;

namespace TransactionsService.Application.DTOs;

/// <summary>
/// Response DTO returned by transaction queries.
/// </summary>
public sealed record TransactionResponse(
    string Id,
    string UserId,
    string CardId,
    double TotalAmount,
    double? MonthAmount,
    int? Installments,
    int? ActualInstallments,
    string Type,
    string? Description,
    bool IsRecurring,
    DateTime CardDueDate,
    DateTime CreatedAt,
    DateTime? LastUpdated);

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
