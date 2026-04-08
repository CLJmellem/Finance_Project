using Auth.Domain.Command.Logout;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using MediatR;
using MongoDB.Bson;

namespace Auth.Application.Commands.Logout
{
    /// <summary>
    /// LogoutUserCommandHandler
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;Auth.Domain.Command.Logout.LogoutUserCommand, Auth.Domain.Command.Logout.LogoutUserCommandResponse&gt;" />
    public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, LogoutUserCommandResponse>
    {
        /// <summary>The token repository</summary>
        private readonly ITokenRepository _tokenRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutUserCommandHandler"/> class.
        /// </summary>
        /// <param name="tokenRepository">The token repository.</param>
        public LogoutUserCommandHandler(ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        /// <summary>Handles a request</summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        /// <exception cref="InvalidUserDataException">Tokens not found or already invalidated.</exception>
        public async Task<LogoutUserCommandResponse> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {

            var userTokens = await _tokenRepository.GetOneAsync(t => t.UserId == ObjectId.Parse(request.UserId));

            if (userTokens == null)
                throw new InvalidUserDataException("Tokens not found or already invalidated.");

            await _tokenRepository.DeleteAsync(userTokens.Id);

            return new LogoutUserCommandResponse { Message = "User logged out successfully" };
        }
    }
}