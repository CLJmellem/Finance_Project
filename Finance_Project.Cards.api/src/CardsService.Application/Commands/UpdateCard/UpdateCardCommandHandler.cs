using AutoMapper;
using CardsService.Application.DTOs;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using MediatR;

namespace CardsService.Application.Commands.UpdateCard;

/// <summary>
/// UpdateCardCommandHandler
/// </summary>
/// <seealso cref="MediatR.IRequestHandler&lt;CardsService.Application.Commands.UpdateCard.UpdateCardCommand, CardsService.Application.DTOs.CardResponse&gt;" />
public sealed class UpdateCardCommandHandler(
    ICardRepository repository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateCardCommand, CardResponse>
{

    /// <summary>Handles the specified request.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    /// <exception cref="CardsService.Application.Exceptions.NotFoundException">Card '{request.CardId}' not found.</exception>
    /// <exception cref="CardsService.Application.Exceptions.DomainException">A card named '{request.Name}' already exists.</exception>
    public async Task<CardResponse> Handle(UpdateCardCommand request, CancellationToken ct)
    {
        // Admins can update any card, regular users can only update their own cards
        var card = currentUser.IsAdmin
            ? await repository.GetByIdAsync(request.CardId, ct)
            : await repository.GetOneAsync(c => c.Id == request.CardId && c.UserId == currentUser.UserId, ct);

        if (card is null)
            throw new NotFoundException($"Card '{request.CardId}' not found.");

        // Check for duplicate card name for the same user (excluding the current card)
        var existing = await repository.GetOneAsync(
            c => c.UserId == card.UserId && c.Name == request.Name.Trim() && c.Id != request.CardId, ct);
        if (existing is not null)
            throw new DomainException($"A card named '{request.Name}' already exists.");

        card.Name = request.Name.Trim();
        card.CreditLimit = request.CreditLimit;
        card.DueDay = request.DueDay;
        card.IsActive = request.IsActive;
        card.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(card, ct);

        return mapper.Map<CardResponse>(card);
    }
}