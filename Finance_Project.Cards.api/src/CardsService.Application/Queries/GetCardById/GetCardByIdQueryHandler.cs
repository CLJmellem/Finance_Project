using AutoMapper;
using CardsService.Application.DTOs;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using MediatR;

namespace CardsService.Application.Queries.GetCardById;

/// <summary>
/// GetCardByIdQueryHandler
/// </summary>
/// <seealso cref="MediatR.IRequestHandler&lt;CardsService.Application.Queries.GetCardById.GetCardByIdQuery, CardsService.Application.DTOs.CardResponse&gt;" />
public sealed class GetCardByIdQueryHandler(
    ICardRepository repository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<GetCardByIdQuery, CardResponse>
{

    /// <summary>Handles the specified request.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    /// <exception cref="CardsService.Application.Exceptions.NotFoundException">Card '{request.CardId}' not found.</exception>
    public async Task<CardResponse> Handle(GetCardByIdQuery request, CancellationToken ct)
    {
        // Admins can access any card, while regular users can only access their own cards.
        var card = currentUser.IsAdmin
            ? await repository.GetByIdAsync(request.CardId, ct)
            : await repository.GetOneAsync(c => c.Id == request.CardId && c.UserId == currentUser.UserId, ct);

        if (card is null)
            throw new NotFoundException($"Card '{request.CardId}' not found.");

        return mapper.Map<CardResponse>(card);
    }
}