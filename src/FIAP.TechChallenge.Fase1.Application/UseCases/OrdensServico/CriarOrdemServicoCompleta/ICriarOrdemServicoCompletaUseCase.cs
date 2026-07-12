using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoCompleta;

public interface ICriarOrdemServicoCompletaUseCase
{
    Task<Result<CriarOrdemServicoCompletaResponse>> ExecuteAsync(CriarOrdemServicoCompletaCommand command, CancellationToken cancellationToken = default);
}
