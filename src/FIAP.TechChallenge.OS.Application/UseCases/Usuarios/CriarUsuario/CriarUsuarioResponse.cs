namespace FIAP.TechChallenge.OS.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioResponse
{
    public Guid Id { get; init; }
    public string Usuario { get; init; } = string.Empty;
}

