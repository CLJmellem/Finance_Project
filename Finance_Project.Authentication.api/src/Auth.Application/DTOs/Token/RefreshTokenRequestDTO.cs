namespace Auth.Application.DTOs.Token
{
    /// <summary>
    /// RefreshTokenRequestDTO
    /// </summary>
    public class RefreshTokenRequestDTO
    {
        /// <summary>Gets or sets the refresh token.</summary>
        /// <value>The refresh token.</value>
        public required string RefreshToken { get; set; }
    }
}
