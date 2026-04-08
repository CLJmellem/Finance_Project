namespace Auth.Application.DTOs.Login
{
    /// <summary>
    /// The login request dto.
    /// </summary>
    public class LoginRequestDTO
    {
        /// <summary>Gets or sets the username or email.</summary>
        /// <value>The username or email.</value>
        public required string UsernameOrEmail { get; set; }

        /// <summary>Gets or sets the password.</summary>
        /// <value>The password.</value>
        public required string Password { get; set; }
    }
}