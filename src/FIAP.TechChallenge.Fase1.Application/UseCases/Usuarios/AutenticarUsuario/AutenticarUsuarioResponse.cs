namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.AutenticarUsuario;

public sealed class AutenticarUsuarioResponse
{
    public string Token { get; init; } = string.Empty;
    public string TipoToken { get; init; } = "Bearer";
    public int ExpiresInSeconds { get; init; }
}
