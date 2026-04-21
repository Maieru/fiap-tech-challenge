namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioCommand
{
    public string Usuario { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}
