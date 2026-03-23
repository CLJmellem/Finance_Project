namespace TransactionsService.Application.DTOs;

/// <summary>
/// Response DTO representing a card from the usersCards collection.
/// </summary>
public sealed record CardResponse(
    string Id,
    string UserId,
    string Name,
    string LastFourDigits,
    double CreditLimit,
    int DueDay,
    bool IsActive);
