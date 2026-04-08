using MediatR;

namespace Auth.Domain.Commands.Register;

/// <summary>
/// RegisterUserCommand
/// </summary>
/// <seealso cref="MediatR.IRequest&lt;System.String&gt;" />
/// <seealso cref="MediatR.IBaseRequest" />
/// <seealso cref="System.IEquatable&lt;Auth.Domain.Commands.Register.RegisterUserCommand&gt;" />
public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<string>;