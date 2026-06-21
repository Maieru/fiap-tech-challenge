namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.AutenticarUsuario;

public sealed class AutenticarUsuarioCommand
{
    public string Usuario { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

