using MediatR;

namespace Auth.Domain.Command.Token
{
    /// <summary>
    /// TokenCommand
    /// </summary>
    /// <seealso cref="MediatR.IRequest&lt;Auth.Domain.Command.Token.TokenCommandResponse&gt;" />
    public record TokenCommand(
        string RefreshToken
        ) : IRequest<TokenCommandResponse>;
}