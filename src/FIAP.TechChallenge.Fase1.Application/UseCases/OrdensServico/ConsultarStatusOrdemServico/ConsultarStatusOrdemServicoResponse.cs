using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConsultarStatusOrdemServico;

public sealed class ConsultarStatusOrdemServicoResponse
{
    public Guid Id { get; init; }
    public StatusOrdemServico Status { get; init; }
}
