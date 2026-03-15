namespace CardsService.API.Requests;

/// <summary>
/// Request body para atualização de cartão.
/// O CardId vem da rota, não do body (RESTful).
/// </summary>
public sealed record UpdateCardRequest(
    string Name,
    decimal CreditLimit,
    int DueDay,
    bool IsActive);
