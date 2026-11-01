using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;

public sealed class OrdemServicoEntity
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
    public StatusOrdemServico Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public DecisaoExecucao Decisao { get; set; }
    public DateTime? DataDecisaoUtc { get; set; }
    public DateTime? DataInicioDiagnostico { get; set; }
    public DateTime? DataEnvioAprovacao { get; set; }
    public DateTime? DataInicioExecucao { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public long Version { get; set; }
}

public sealed class HistoricoOrdemEntity
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime OcorridoEmUtc { get; set; }
    public string? CorrelationId { get; set; }
}
