using CardsService.Application.DTOs;
using MediatR;

namespace CardsService.Application.Queries.GetUserCards;

/// <summary>
/// GetUserCardsQuery
/// </summary>
/// <seealso cref="MediatR.IRequest&lt;CardsService.Application.DTOs.PaginatedResponse&lt;CardsService.Application.DTOs.CardResponse&gt;&gt;" />
/// <seealso cref="MediatR.IBaseRequest" />
/// <seealso cref="System.IEquatable&lt;CardsService.Application.Queries.GetUserCards.GetUserCardsQuery&gt;" />
public sealed record GetUserCardsQuery(
    int Page = 0,
    int PageSize = 0,
    string? UserId = null,
    bool InactiveCards = false)
    : IRequest<PaginatedResponse<CardResponse>>;
