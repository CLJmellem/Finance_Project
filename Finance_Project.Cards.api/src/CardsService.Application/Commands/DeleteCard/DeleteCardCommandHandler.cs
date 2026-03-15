using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using MediatR;

namespace CardsService.Application.Commands.DeleteCard;

/// <summary>
/// DeleteCardCommandHandler
/// </summary>
/// <seealso cref="MediatR.IRequestHandler&lt;CardsService.Application.Commands.DeleteCard.DeleteCardCommand, MediatR.Unit&gt;" />
public sealed class DeleteCardCommandHandler(
    ICardRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteCardCommand, Unit>
{

    /// <summary>Handles the specified request.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    /// <exception cref="CardsService.Application.Exceptions.NotFoundException">Card '{request.CardId}' not found.</exception>
    /// <exception cref="CardsService.Application.Exceptions.DomainException">Card must be deactivated before it can be deleted.</exception>
    public async Task<Unit> Handle(DeleteCardCommand request, CancellationToken ct)
    {
        // Admin users can delete any card, while regular users can only delete their own cards.
        var card = currentUser.IsAdmin
            ? await repository.GetByIdAsync(request.CardId, ct)
            : await repository.GetOneAsync(c => c.Id == request.CardId && c.UserId == currentUser.UserId, ct);

        // If the card doesn't exist or the user doesn't have permission to delete it, throw a NotFoundException.
        if (card is null)
            throw new NotFoundException($"Card '{request.CardId}' not found.");

        // If the card is active, it cannot be deleted. It must be deactivated first.
        if (card.IsActive)
            throw new DomainException("Card must be deactivated before it can be deleted.");

        await repository.DeleteAsync(card.Id, ct);

        return Unit.Value;
    }
}