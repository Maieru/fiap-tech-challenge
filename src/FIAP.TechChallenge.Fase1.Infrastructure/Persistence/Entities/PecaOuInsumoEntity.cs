using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

[Table("PecasInsumos")]
public sealed class PecaInsumoEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoUnitario { get; set; }

    public int QuantidadeEstoque { get; set; }

    public bool Ativo { get; set; }
}