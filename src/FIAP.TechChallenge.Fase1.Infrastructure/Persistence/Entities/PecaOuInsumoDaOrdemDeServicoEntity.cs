using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

[Table("PecasOuInsumoDaOrdemDeServico")]
public sealed class PecaOuInsumoDaOrdemDeServicoEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OrdemServicoId { get; set; }

    // Aqui optei por não criar uma relação direta com a entidade de peça ou insumo, pois a ideia é que esse campo seja só histórico, ou seja,
    // mesmo que a peça ou insumo seja excluído ou alterado, a ordem de serviço deve manter as informações da peça ou insumo conforme estavam no momento da inclusão na ordem de serviço.
    [Required]
    public Guid PecaInsumoId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    [Required]
    [Column(TypeName = "numeric(18,2)")]
    public decimal PrecoUnitario { get; set; }

    [Required]
    public int Quantidade { get; set; }

    [ForeignKey(nameof(OrdemServicoId))]
    public OrdemServicoEntity OrdemServico { get; set; } = null!;
}