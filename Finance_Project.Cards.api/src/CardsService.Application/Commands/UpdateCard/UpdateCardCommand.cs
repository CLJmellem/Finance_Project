using CardsService.Application.DTOs;
using MediatR;

namespace CardsService.Application.Commands.UpdateCard;

/// <summary>
/// UpdateCardCommand
/// </summary>
/// <seealso cref="MediatR.IRequest&lt;CardsService.Application.DTOs.CardResponse&gt;" />
/// <seealso cref="MediatR.IBaseRequest" />
/// <seealso cref="System.IEquatable&lt;CardsService.Application.Commands.UpdateCard.UpdateCardCommand&gt;" />
public sealed record UpdateCardCommand(
    string CardId,
    string Name,
    decimal CreditLimit,
    int DueDay,
    bool IsActive) : IRequest<CardResponse>;
