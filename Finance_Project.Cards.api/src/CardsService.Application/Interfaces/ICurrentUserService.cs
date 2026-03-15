namespace CardsService.Application.Interfaces;

/// <summary>
/// Abstração para obter os dados do usuário autenticado via JWT.
/// Implementado na camada API (HttpContext).
/// </summary>
public interface ICurrentUserService
{
    string UserId { get; }
    string Username { get; }
    string Role { get; }
    bool IsAdmin { get; }
}
