using System.Security.Claims;
using TransactionsService.Application.Interfaces;

namespace TransactionsService.API.Services;

/// <summary>
/// Extracts authenticated user information from the current HTTP context's claims principal.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal _user = httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("No authenticated user found.");

    public string UserId =>
        _user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _user.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("UserId claim not found in token.");

    public string Username =>
        _user.FindFirstValue(ClaimTypes.Name)
        ?? _user.FindFirstValue("unique_name")
        ?? string.Empty;

    public string Role =>
        _user.FindFirstValue(ClaimTypes.Role)
        ?? _user.FindFirstValue("role")
        ?? "FreeUser";

    public bool IsAdmin =>
        Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
}
