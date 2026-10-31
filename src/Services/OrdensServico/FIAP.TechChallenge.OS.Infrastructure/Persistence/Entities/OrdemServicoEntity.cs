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
    public DateTime? DataInicioDiagnostico { get; set; }
    public DateTime? DataEnvioAprovacao { get; set; }
    public DateTime? DataInicioExecucao { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public long Version { get; set; }
    public long OutputSequence { get; set; } = 1;
    public long InputSequence { get; set; }
    public string? AcaoPendente { get; set; }
    public Guid? SolicitacaoPendenteId { get; set; }
    public string? DiagnosticoJson { get; set; }
}

public sealed class HistoricoOrdemEntity
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime OcorridoEmUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? Detalhes { get; set; }
}

public sealed class OutboxEntity
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public long Sequence { get; set; }
    public string Body { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public bool Published { get; set; }
}
public sealed class InboxEntity { public Guid Id { get; set; } public DateTime ProcessedAtUtc { get; set; } }
public sealed class DispatcherLeaseEntity
{
    public string Id { get; set; } = "dispatcher";
    public string Owner { get; set; } = "";
    public DateTime UntilUtc { get; set; }
}
