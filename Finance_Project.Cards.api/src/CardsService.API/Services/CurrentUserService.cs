using System.Security.Claims;
using CardsService.Application.Interfaces;

namespace CardsService.API.Services;

/// <summary>
/// Extracts user information from the current HTTP context's claims principal.
/// </summary>
/// <seealso cref="CardsService.Application.Interfaces.ICurrentUserService" />
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal _user = httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("No authenticated user found.");

    // Extracts the UserId from claims, supporting both standard and custom claim types.
    public string UserId =>
        _user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _user.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("UserId claim not found in token.");

    // Extracts the Username from claims, supporting both standard and custom claim types.
    public string Username =>
        _user.FindFirstValue(ClaimTypes.Name)
        ?? _user.FindFirstValue("unique_name")
        ?? string.Empty;

    // Extracts the Role from claims, supporting both standard and custom claim types, with a default of "FreeUser".
    public string Role =>
        _user.FindFirstValue(ClaimTypes.Role)
        ?? _user.FindFirstValue("role")
        ?? "FreeUser";

    // Determines if the user has an Admin role, using a case-insensitive comparison.
    public bool IsAdmin =>
        Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
}
