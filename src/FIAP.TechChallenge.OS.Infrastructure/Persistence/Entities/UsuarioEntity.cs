namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;

public sealed class UsuarioEntity
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
