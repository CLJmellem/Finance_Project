namespace Auth.Application.DTOs.Register
{
    /// <summary>
    /// The register request dto.
    /// </summary>
    public class RegisterRequestDTO
    {
        /// <summary>Gets or sets the username.</summary>
        /// <value>The username.</value>
        public required string Username { get; set; }

        /// <summary>Gets or sets the password.</summary>
        /// <value>The password.</value>
        public required string Password { get; set; }

        /// <summary>Gets or sets the confirm password.</summary>
        /// <value>The confirm password.</value>
        public required string ConfirmPassword { get; set; }

        /// <summary>Gets or sets the email.</summary>
        /// <value>The email.</value>
        public required string Email { get; set; }
    }
}
