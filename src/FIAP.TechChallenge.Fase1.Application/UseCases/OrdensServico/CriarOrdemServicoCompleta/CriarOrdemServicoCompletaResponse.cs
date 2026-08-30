using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoCompleta;

public sealed class CriarOrdemServicoCompletaResponse
{
    public Guid Id { get; init; }
    public string? Token { get; init; }
    public Guid ClienteId { get; init; }
    public Guid VeiculoId { get; init; }
    public string DescricaoProblema { get; init; } = string.Empty;
    public StatusOrdemServico Status { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime DataInicioDiagnostico { get; init; }
    public IReadOnlyCollection<AdicionarServicoOrdemServicoResponse> Servicos { get; init; } = [];
    public IReadOnlyCollection<AdicionarPecaInsumoOrdemServicoResponse> PecasInsumos { get; init; } = [];
}
