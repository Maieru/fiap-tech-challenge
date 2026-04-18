using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;

public interface IRecuperarOrdemServicoUseCase
{
    Task<Result<RecuperarOrdemServicoResponse>> ExecuteAsync(RecuperarOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
