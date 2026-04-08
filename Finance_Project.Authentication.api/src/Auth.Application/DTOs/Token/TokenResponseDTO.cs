namespace Auth.Application.DTOs.Token
{
    /// <summary>
    /// TokenResponseDTO
    /// </summary>
    public class TokenResponseDTO
    {
        /// <summary>Gets or sets the access token.</summary>
        /// <value>The access token.</value>
        public required string Token { get; set; }

        /// <summary>Gets or sets the access token expiration.</summary>
        /// <value>The access token expiration.</value>
        public DateTime? ExpireAt { get; set; }
    }
}