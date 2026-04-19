using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;

public interface IAprovarExecucaoOrdemServicoUseCase
{
    Task<Result<AprovarExecucaoOrdemServicoResponse>> ExecuteAsync(AprovarExecucaoOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
