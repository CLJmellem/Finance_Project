namespace CardsService.Application.DTOs;

/// <summary>
/// CardResponse
/// </summary>
/// <seealso cref="System.IEquatable&lt;CardsService.Application.DTOs.CardResponse&gt;" />
public sealed record CardResponse(
    string Id,
    string UserId,
    string Name,
    string Brand,
    string LastFourDigits,
    decimal CreditLimit,
    int DueDay,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// PaginatedResponse
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="System.IEquatable&lt;CardsService.Application.DTOs.PaginatedResponse&lt;T&gt;&gt;" />
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}