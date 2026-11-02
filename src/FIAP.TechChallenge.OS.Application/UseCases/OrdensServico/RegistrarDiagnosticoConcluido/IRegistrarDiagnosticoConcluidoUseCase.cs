using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarDiagnosticoConcluido;

public interface IRegistrarDiagnosticoConcluidoUseCase
{
    Task<Result<bool>> ExecuteAsync(RegistrarDiagnosticoConcluidoCommand command, CancellationToken cancellationToken = default);
}
