namespace Auth.Domain.Command.Login
{
    /// <summary>
    /// LoginUserCommandResponse
    /// </summary>
    public class LoginUserCommandResponse
    {
        /// <summary>Gets or sets the access token.</summary>
        /// <value>The access token.</value>
        public string Token { get; set; } = string.Empty;

        /// <summary>Gets or sets the access token expiration.</summary>
        /// <value>The access token expiration.</value>
        public DateTime? Expiration { get; set; }

        /// <summary>Gets or sets the refresh token.</summary>
        /// <value>The refresh token.</value>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>Gets or sets the refresh token expiration.</summary>
        /// <value>The refresh token expiration.</value>
        public DateTime? RefreshTokenExpiration { get; set; }
    }
}