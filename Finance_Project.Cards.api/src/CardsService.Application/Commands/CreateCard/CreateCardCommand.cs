using CardsService.Application.DTOs;
using CardsService.Domain.Enums;
using MediatR;

namespace CardsService.Application.Commands.CreateCard;

/// <summary>
/// CreateCardCommand
/// </summary>
/// <seealso cref="MediatR.IRequest&lt;CardsService.Application.DTOs.CardResponse&gt;" />
/// <seealso cref="MediatR.IBaseRequest" />
/// <seealso cref="System.IEquatable&lt;CardsService.Application.Commands.CreateCard.CreateCardCommand&gt;" />
public sealed record CreateCardCommand(
    string Name,
    CardBrand Brand,
    string LastFourDigits,
    decimal CreditLimit,
    int DueDay) : IRequest<CardResponse>;
