namespace Auth.Application.Interfaces
{
    /// <summary>
    /// ITokenCreationService
    /// </summary>
    public interface ITokenCreationService
    {
        /// <summary>Generates the token.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="username">The username.</param>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        Tuple<string, DateTime> GenerateToken(string userId, string username, string role);
    }
}