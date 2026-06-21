using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.FinalizarOrdemServico;

public interface IFinalizarOrdemServicoUseCase
{
    Task<Result<FinalizarOrdemServicoResponse>> ExecuteAsync(FinalizarOrdemServicoCommand command, CancellationToken cancellationToken = default);
}

