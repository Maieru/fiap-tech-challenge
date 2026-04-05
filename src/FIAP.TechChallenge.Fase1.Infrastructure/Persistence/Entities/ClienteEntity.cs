namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

public sealed class ClienteEntity
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Cnpj { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
}