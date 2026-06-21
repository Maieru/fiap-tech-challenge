using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.FinalizarOrdemServico;

public sealed class FinalizarOrdemServicoResponse
{
    public Guid Id { get; init; }
    public StatusOrdemServico Status { get; init; }
    public DateTime DataFinalizacao { get; init; }
}

