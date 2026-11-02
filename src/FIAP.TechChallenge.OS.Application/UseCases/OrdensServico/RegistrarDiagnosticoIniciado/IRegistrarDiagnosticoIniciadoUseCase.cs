using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarDiagnosticoIniciado;

public interface IRegistrarDiagnosticoIniciadoUseCase
{
    Task<Result<bool>> ExecuteAsync(RegistrarDiagnosticoIniciadoCommand command, CancellationToken cancellationToken = default);
}
