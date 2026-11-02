namespace FIAP.TechChallenge.OS.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioCommand
{
    public string Usuario { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

