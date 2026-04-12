using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

[Table("Servicos")]
public sealed class ServicoEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorUnitario { get; set; }
}
