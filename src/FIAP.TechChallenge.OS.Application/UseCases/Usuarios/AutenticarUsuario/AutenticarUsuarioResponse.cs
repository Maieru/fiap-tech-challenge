namespace FIAP.TechChallenge.OS.Application.UseCases.Usuarios.AutenticarUsuario;

public sealed class AutenticarUsuarioResponse
{
    public string Token { get; init; } = string.Empty;
    public string TipoToken { get; init; } = "Bearer";
    public int ExpiresInSeconds { get; init; }
}

