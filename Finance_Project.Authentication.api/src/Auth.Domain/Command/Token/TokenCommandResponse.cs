namespace Auth.Domain.Command.Token
{
    /// <summary>
    /// TokenCommandResponse
    /// </summary>
    public class TokenCommandResponse
    {
        /// <summary>Gets or sets the access token.</summary>
        /// <value>The access token.</value>
        public string Token { get; set; } = string.Empty;

        /// <summary>Gets or sets the access token expiration.</summary>
        /// <value>The access token expiration.</value>
        public DateTime? ExpireAt { get; set; }
    }
}