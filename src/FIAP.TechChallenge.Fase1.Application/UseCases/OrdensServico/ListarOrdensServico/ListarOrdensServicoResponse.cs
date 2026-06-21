using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;

public sealed class ListarOrdensServicoResponse
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public IReadOnlyCollection<ListarOrdemServicoItemResponse> OrdensServico { get; init; } = [];
}

public sealed class ListarOrdemServicoItemResponse
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public Guid VeiculoId { get; init; }
    public string DescricaoProblema { get; init; } = string.Empty;
    public StatusOrdemServico Status { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime? DataInicioDiagnostico { get; init; }
    public DateTime? DataEnvioAprovacao { get; init; }
    public DateTime? DataInicioExecucao { get; init; }
    public DateTime? DataFinalizacao { get; init; }
    public DateTime? DataEntrega { get; init; }
}

