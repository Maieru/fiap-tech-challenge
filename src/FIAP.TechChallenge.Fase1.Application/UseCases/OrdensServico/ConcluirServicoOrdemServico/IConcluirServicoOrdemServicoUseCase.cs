using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;

public interface IConcluirServicoOrdemServicoUseCase
{
    Task<Result<ConcluirServicoOrdemServicoResponse>> ExecuteAsync(
        ConcluirServicoOrdemServicoCommand command,
        CancellationToken cancellationToken = default);
}

