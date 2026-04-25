using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AcompanhamentoOrdemServico;

public sealed class AcompanhamentoOrdemServicoResponse
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public string ClienteNome { get; init; } = string.Empty;
    public Guid VeiculoId { get; init; }
    public string VeiculoMarca { get; init; } = string.Empty;
    public string VeiculoModelo { get; init; } = string.Empty;
    public string VeiculoPlaca { get; init; } = string.Empty;
    public int VeiculoAno { get; init; }
    public string DescricaoProblema { get; init; } = string.Empty;
    public StatusOrdemServico Status { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime? DataInicioDiagnostico { get; init; }
    public DateTime? DataEnvioAprovacao { get; init; }
    public DateTime? DataInicioExecucao { get; init; }
    public DateTime? DataFinalizacao { get; init; }
    public DateTime? DataEntrega { get; init; }
    public IReadOnlyCollection<AcompanhamentoServicoItemResponse> Servicos { get; init; } = [];
    public IReadOnlyCollection<AcompanhamentoPecaInsumoItemResponse> PecasInsumos { get; init; } = [];
    public decimal ValorTotalServicos { get; init; }
    public decimal ValorTotalPecasInsumos { get; init; }
    public decimal ValorTotalOrdemServico { get; init; }
}

public sealed class AcompanhamentoServicoItemResponse
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal ValorUnitario { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorTotal { get; init; }
    public int? TempoGastoMinutos { get; init; }
    public bool Concluido { get; init; }
}

public sealed class AcompanhamentoPecaInsumoItemResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public decimal PrecoUnitario { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorTotal { get; init; }
}
