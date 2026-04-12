using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;

public interface IIniciarDiagnosticoOrdemServicoUseCase
{
    Task<Result<IniciarDiagnosticoOrdemServicoResponse>> ExecuteAsync(IniciarDiagnosticoOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
