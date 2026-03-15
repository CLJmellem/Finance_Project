using CardsService.Application.DTOs;
using MediatR;

namespace CardsService.Application.Queries.GetCardById;

/// <summary>
/// GetCardByIdQuery
/// </summary>
/// <seealso cref="MediatR.IRequest&lt;CardsService.Application.DTOs.CardResponse&gt;" />
/// <seealso cref="MediatR.IBaseRequest" />
/// <seealso cref="System.IEquatable&lt;CardsService.Application.Queries.GetCardById.GetCardByIdQuery&gt;" />
public sealed record GetCardByIdQuery(string CardId) : IRequest<CardResponse>;
