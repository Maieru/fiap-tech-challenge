using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoIniciada;

public interface IRegistrarExecucaoIniciadaUseCase
{
    Task<Result<bool>> ExecuteAsync(RegistrarExecucaoIniciadaCommand command, CancellationToken cancellationToken = default);
}
