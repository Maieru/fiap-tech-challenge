namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
public sealed class AcessoClienteEntity
{
    public Guid OrdemServicoId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
}
