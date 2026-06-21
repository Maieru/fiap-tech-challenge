using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CancelarOrdemServico;

public sealed class CancelarOrdemServicoResponse
{
    public Guid Id { get; set; }
    public StatusOrdemServico Status { get; set; }
}

