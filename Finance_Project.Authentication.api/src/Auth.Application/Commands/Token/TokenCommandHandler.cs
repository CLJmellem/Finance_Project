using Auth.Application.Interfaces;
using Auth.Domain.Command.Token;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Commands.Token
{
    /// <summary>
    /// TokenCommandHandler
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;Auth.Domain.Command.Token.TokenCommand, Auth.Domain.Command.Token.TokenCommandResponse&gt;" />
    public class TokenCommandHandler : IRequestHandler<TokenCommand, TokenCommandResponse>
    {
        /// <summary>The token repository.</summary>
        private readonly ITokenRepository _tokenRepository;
        /// <summary>The user repository.</summary>
        private readonly IUserRepository _userRepository;
        /// <summary>The token creation service.</summary>
        private readonly ITokenCreationService _tokenCreationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCommandHandler"/> class.
        /// </summary>
        /// <param name="tokenRepository">The token repository.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="tokenCreationService">The token creation service.</param>
        public TokenCommandHandler(
            ITokenRepository tokenRepository,
            IUserRepository userRepository,
            ITokenCreationService tokenCreationService)
        {
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
            _tokenCreationService = tokenCreationService;
        }

        /// <summary>Handles a request</summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        public async Task<TokenCommandResponse> Handle(TokenCommand request, CancellationToken cancellationToken)
        {
            // Try to find the token record by the provided refresh token
            var tokenRecord = await _tokenRepository.GetOneAsync(t => t.RefreshToken == request.RefreshToken);

            if (tokenRecord == null)
                throw new InvalidUserDataException("Invalid refresh token.");

            if (tokenRecord.RefreshTokenExpireAt < DateTime.UtcNow)
                throw new InvalidUserDataException("Refresh token has expired.");

            var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);

            if (user == null)
                throw new UserNotFoundException("User not found.");

            // Generate a new access token using the token creation service
            (string accessToken, DateTime accessExpiration) = _tokenCreationService.GenerateToken(user.Id.ToString(), user.Username, user.Role.ToString());

            return new TokenCommandResponse
            {
                Token = accessToken,
                ExpireAt = accessExpiration
            };
        }
    }
}
