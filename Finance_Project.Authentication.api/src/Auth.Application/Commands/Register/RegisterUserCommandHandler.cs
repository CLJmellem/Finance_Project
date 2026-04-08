using Auth.Application.Interfaces;
using Auth.Domain.Commands.Register;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Commands.Register;

/// <summary>
/// RegisterUserCommandHandler
/// </summary>
/// <seealso cref="MediatR.IRequestHandler&lt;Auth.Domain.Commands.Register.RegisterUserCommand, System.String&gt;" />
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    /// <summary>The user repository</summary>
    private readonly IUserRepository _userRepository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public RegisterUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>Handles a request</summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    /// <exception cref="UserAlreadyExistsException">
    /// Email '{request.Email}' já está em uso.
    /// or
    /// Username '{request.Username}' já está em uso.
    /// </exception>
    public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetByEmailAsync(request.Email) != null)
        {
            throw new UserAlreadyExistsException($"Email '{request.Email}' already in use by another user.");
        }

        if (await _userRepository.GetByUsernameAsync(request.Username) != null)
        {
            throw new UserAlreadyExistsException($"Username '{request.Username}' already in use by another user.");
        }

        // Create a password hash using BCrypt with a work factor of 13
        var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, 13);

        var user = new UserInfoDataEntity(
            username: request.Username,
            email: request.Email,
            passwordHash: passwordHash
        );

        var createdUser = await _userRepository.CreateAsync(user);

        return createdUser.Username;
    }
}