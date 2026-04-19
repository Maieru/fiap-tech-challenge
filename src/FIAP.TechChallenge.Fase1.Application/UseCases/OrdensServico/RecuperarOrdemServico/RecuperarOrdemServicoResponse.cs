using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;

public sealed class RecuperarOrdemServicoResponse
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
    public IReadOnlyCollection<RecuperarServicoDaOrdemServicoItemResponse> Servicos { get; init; } = [];
    public IReadOnlyCollection<RecuperarPecaInsumoDaOrdemServicoItemResponse> PecasInsumos { get; init; } = [];
    public decimal ValorTotalServicos { get; init; }
    public decimal ValorTotalPecasInsumos { get; init; }
    public decimal ValorTotalOrdemServico { get; init; }
}

public sealed class RecuperarServicoDaOrdemServicoItemResponse
{
    public Guid Id { get; init; }
    public Guid OrdemServicoId { get; init; }
    public Guid ServicoId { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal ValorUnitario { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorTotal { get; init; }
    public int? TempoGastoMinutos { get; init; }
    public bool Concluido { get; init; }
}

public sealed class RecuperarPecaInsumoDaOrdemServicoItemResponse
{
    public Guid Id { get; init; }
    public Guid OrdemServicoId { get; init; }
    public Guid PecaInsumoId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public decimal PrecoUnitario { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorTotal { get; init; }
}
