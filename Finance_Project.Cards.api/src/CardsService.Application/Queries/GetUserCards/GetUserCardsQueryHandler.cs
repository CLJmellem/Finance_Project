using AutoMapper;
using CardsService.Application.DTOs;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using CardsService.Domain.Entities;
using MediatR;
using System.Linq.Expressions;

namespace CardsService.Application.Queries.GetUserCards;

/// <summary>
/// GetUserCardsQueryHandler
/// </summary>
/// <seealso cref="MediatR.IRequestHandler&lt;CardsService.Application.Queries.GetUserCards.GetUserCardsQuery, CardsService.Application.DTOs.PaginatedResponse&lt;CardsService.Application.DTOs.CardResponse&gt;&gt;" />
public sealed class GetUserCardsQueryHandler(
    ICardRepository repository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<GetUserCardsQuery, PaginatedResponse<CardResponse>>
{

    /// <summary>Handles the specified request.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    /// <exception cref="CardsService.Application.Exceptions.DomainException">UserId is required for admin queries.</exception>
    public async Task<PaginatedResponse<CardResponse>> Handle(
        GetUserCardsQuery request, CancellationToken ct)
    {
        string userId;

        // Admin users can query cards for any user by providing a UserId, while regular users can only query their own cards.
        if (currentUser.IsAdmin)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new DomainException("UserId is required for admin queries.");

            userId = request.UserId;
        }
        else
        {
            userId = currentUser.UserId;
        }

        Expression<Func<CardDataEntity, bool>> filter = request.InactiveCards
            ? c => c.UserId == userId
            : c => c.UserId == userId && c.IsActive;

        var cards = await repository.GetAllAsync(request.Page, request.PageSize, filter, ct);
        var totalCount = await repository.CountAllAsync(filter, ct);

        return new PaginatedResponse<CardResponse>(
            Items: mapper.Map<List<CardResponse>>(cards).AsReadOnly(),
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount);
    }
}