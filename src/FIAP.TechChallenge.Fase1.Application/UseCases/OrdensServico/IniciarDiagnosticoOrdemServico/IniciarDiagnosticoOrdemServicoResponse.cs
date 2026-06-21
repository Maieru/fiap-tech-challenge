using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;

public sealed class IniciarDiagnosticoOrdemServicoResponse
{
    public Guid Id { get; init; }
    public StatusOrdemServico Status { get; init; }
    public DateTime DataInicioDiagnostico { get; init; }
}

