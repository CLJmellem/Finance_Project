using MediatR;

namespace Auth.Domain.Command.Login
{
    /// <summary>
    /// LoginUserCommand
    /// </summary>
    /// <seealso cref="MediatR.IRequest&lt;Auth.Domain.Command.Login.LoginUserCommandResponse&gt;" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable&lt;Auth.Domain.Command.Login.LoginUserCommand&gt;" />
    public record LoginUserCommand(
        string UsernameOrEmail,
        string Password) : IRequest<LoginUserCommandResponse>
    {}
}