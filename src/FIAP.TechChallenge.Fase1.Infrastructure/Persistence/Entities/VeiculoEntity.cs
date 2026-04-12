using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

[Table("Veiculos")]
public sealed class VeiculoEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("Clientes")]
    public Guid ClienteId { get; set; }

    [Required]
    [MaxLength(7)]
    public string Placa { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Modelo { get; set; } = string.Empty;

    [Required]
    public int Ano { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public ClienteEntity Cliente { get; set; } = null!;
}
