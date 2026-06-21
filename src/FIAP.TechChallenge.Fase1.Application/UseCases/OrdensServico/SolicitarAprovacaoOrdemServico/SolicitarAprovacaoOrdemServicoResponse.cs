using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;

public sealed class SolicitarAprovacaoOrdemServicoResponse
{
    public Guid Id { get; init; }
    public StatusOrdemServico Status { get; init; }
    public DateTime DataEnvioAprovacao { get; init; }
}

