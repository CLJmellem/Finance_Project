using AutoMapper;
using CardsService.Application.DTOs;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using CardsService.Domain.Entities;
using MediatR;

namespace CardsService.Application.Commands.CreateCard;

/// <summary>
/// CreateCardCommandHandler
/// </summary>
/// <seealso cref="MediatR.IRequestHandler&lt;CardsService.Application.Commands.CreateCard.CreateCardCommand, CardsService.Application.DTOs.CardResponse&gt;" />
public sealed class CreateCardCommandHandler(
    ICardRepository repository,
    ICurrentUserService currentUser,
    IMapper mapper)
    : IRequestHandler<CreateCardCommand, CardResponse>
{

    /// <summary>Handles the specified request.</summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    /// <exception cref="CardsService.Application.Exceptions.DomainException">A card named '{request.Name}' already exists.</exception>
    public async Task<CardResponse> Handle(CreateCardCommand request, CancellationToken ct)
    {
        var existing = await repository.GetOneAsync(
            c => c.UserId == currentUser.UserId && c.Name == request.Name.Trim(), ct);
        if (existing is not null)
            throw new DomainException($"A card named '{request.Name}' already exists.");

        var card = new CardDataEntity
        {
            UserId = currentUser.UserId,
            Name = request.Name.Trim(),
            Brand = request.Brand,
            LastFourDigits = request.LastFourDigits,
            CreditLimit = request.CreditLimit,
            DueDay = request.DueDay
        };

        await repository.CreateAsync(card, ct);

        return mapper.Map<CardResponse>(card);
    }
}