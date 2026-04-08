using MediatR;

namespace Auth.Domain.Command.Logout
{
    /// <summary>
    /// LogoutUserCommand
    /// </summary>
    /// <seealso cref="MediatR.IRequest&lt;Auth.Domain.Command.Logout.LogoutUserCommandResponse&gt;" />
    public record LogoutUserCommand(
        string UserId
        ) : IRequest<LogoutUserCommandResponse>;
}
