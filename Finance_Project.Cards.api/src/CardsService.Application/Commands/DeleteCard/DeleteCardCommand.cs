using MediatR;

namespace CardsService.Application.Commands.DeleteCard;

/// <summary>
/// DeleteCardCommand
/// </summary>
/// <seealso cref="MediatR.IRequest&lt;MediatR.Unit&gt;" />
/// <seealso cref="MediatR.IBaseRequest" />
/// <seealso cref="System.IEquatable&lt;CardsService.Application.Commands.DeleteCard.DeleteCardCommand&gt;" />
public sealed record DeleteCardCommand(string CardId) : IRequest<Unit>;
