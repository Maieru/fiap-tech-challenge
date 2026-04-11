using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;

public sealed class CriarOrdemServicoResponse
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public Guid VeiculoId { get; init; }
    public string DescricaoProblema { get; init; } = string.Empty;
    public StatusOrdemServico Status { get; init; }
    public DateTime DataCriacao { get; init; }
}
