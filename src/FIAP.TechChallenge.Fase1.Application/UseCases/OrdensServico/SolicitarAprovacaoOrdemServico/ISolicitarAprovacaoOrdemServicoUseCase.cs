using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;

public interface ISolicitarAprovacaoOrdemServicoUseCase
{
    Task<Result<SolicitarAprovacaoOrdemServicoResponse>> ExecuteAsync(SolicitarAprovacaoOrdemServicoCommand command, CancellationToken cancellationToken = default);
}

