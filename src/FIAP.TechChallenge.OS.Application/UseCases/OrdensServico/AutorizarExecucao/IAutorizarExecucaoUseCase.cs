using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.AutorizarExecucao;

public interface IAutorizarExecucaoUseCase
{
    Task<Result<DecisaoExecucaoResponse>> ExecuteAsync(AutorizarExecucaoCommand command, CancellationToken cancellationToken = default);
}
