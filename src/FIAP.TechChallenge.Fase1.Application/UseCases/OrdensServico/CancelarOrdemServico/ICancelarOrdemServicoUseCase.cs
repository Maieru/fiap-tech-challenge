using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CancelarOrdemServico;

public interface ICancelarOrdemServicoUseCase
{
    Task<Result<CancelarOrdemServicoResponse>> ExecuteAsync(CancelarOrdemServicoCommand command, CancellationToken cancellationToken = default);
}

