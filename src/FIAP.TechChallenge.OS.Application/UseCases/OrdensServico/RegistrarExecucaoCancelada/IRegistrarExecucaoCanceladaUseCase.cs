using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoCancelada;

public interface IRegistrarExecucaoCanceladaUseCase
{
    Task<Result<bool>> ExecuteAsync(RegistrarExecucaoCanceladaCommand command, CancellationToken cancellationToken = default);
}
