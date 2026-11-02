using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoConcluida;

public interface IRegistrarExecucaoConcluidaUseCase
{
    Task<Result<bool>> ExecuteAsync(RegistrarExecucaoConcluidaCommand command, CancellationToken cancellationToken = default);
}
