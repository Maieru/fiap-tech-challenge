using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ExcluirOrdemServico;

public interface IExcluirOrdemServicoUseCase
{
    Task<Result<ExcluirOrdemServicoResponse>> ExecuteAsync(ExcluirOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
