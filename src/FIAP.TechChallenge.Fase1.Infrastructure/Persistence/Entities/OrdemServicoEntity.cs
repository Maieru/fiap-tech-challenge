using FIAP.TechChallenge.Fase1.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

[Table("OrdensServico")]
public sealed class OrdemServicoEntity
{
    [Key]
    public Guid Id { get; set; }

    public bool Ativo { get; set; } = true;

    [Required]
    [ForeignKey("Clientes")]
    public Guid ClienteId { get; set; }

    [Required]
    [ForeignKey("Veiculos")]
    public Guid VeiculoId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string DescricaoProblema { get; set; } = string.Empty;

    [Required]
    public StatusOrdemServico Status { get; set; }

    [Required]
    public DateTime DataCriacao { get; set; }

    public DateTime? DataInicioDiagnostico { get; set; }
    public DateTime? DataEnvioAprovacao { get; set; }
    public DateTime? DataInicioExecucao { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public DateTime? DataEntrega { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public ClienteEntity Cliente { get; set; } = null!;

    [ForeignKey(nameof(VeiculoId))]
    public VeiculoEntity Veiculo { get; set; } = null!;
}
