namespace Auth.Application.DTOs.Register;

/// <summary>
/// Response DTO para registro de usuário
/// </summary>
public record RegisterResponseDTO
{
    /// <summary>
    /// Mensagem de sucesso
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
