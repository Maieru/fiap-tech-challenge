using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

[Table("Clientes")]
public sealed class ClienteEntity
{
    [Key]
    public Guid Id { get; set; }

    public bool Ativo { get; set; } = true;

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(14)]
    public string? Cpf { get; set; }

    [MaxLength(18)]
    public string? Cnpj { get; set; }

    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }
}
