namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioResponse
{
    public Guid Id { get; init; }
    public string Usuario { get; init; } = string.Empty;
    public string SenhaCriptografada { get; init; } = string.Empty;
}
