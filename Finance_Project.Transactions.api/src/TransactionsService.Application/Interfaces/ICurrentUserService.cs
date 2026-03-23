namespace TransactionsService.Application.Interfaces;

/// <summary>
/// Abstraction to retrieve authenticated user data from the JWT token.
/// Implemented in the API layer via HttpContext.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the user identifier.</summary>
    /// <value>The user identifier.</value>
    string UserId { get; }

    /// <summary>Gets the username.</summary>
    /// <value>The username.</value>
    string Username { get; }

    /// <summary>Gets the role.</summary>
    /// <value>The role.</value>
    string Role { get; }

    /// <summary>Gets a value indicating whether this instance is admin.</summary>
    /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
    bool IsAdmin { get; }
}