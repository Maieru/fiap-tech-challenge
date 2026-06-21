using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

[Table("ServicosDaOrdemDeServico")]
public sealed class ServicoDaOrdemDeServicoEntity
{
    [Key]
    public Guid Id { get; set; }

    public bool Ativo { get; set; } = true;

    [Required]
    public Guid OrdemServicoId { get; set; }

    // Aqui optei por não criar uma relação direta com a entidade de serviço, pois a ideia é que esse campo seja só histórico, ou seja,
    // mesmo que o serviço seja excluído ou alterado, a ordem de serviço deve manter as informações do serviço conforme estavam no momento da inclusão na ordem de serviço.
    [Required]
    public Guid ServicoId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "numeric(18,2)")]
    public decimal ValorUnitario { get; set; }

    [Required]
    public int Quantidade { get; set; }

    public int? TempoGastoMinutos { get; set; }

    [Required]
    public bool Concluido { get; set; }

    [ForeignKey(nameof(OrdemServicoId))]
    public OrdemServicoEntity OrdemServico { get; set; } = null!;
}

