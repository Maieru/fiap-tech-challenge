using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;

public interface ICriarOrdemServicoUseCase
{
    Task<Result<CriarOrdemServicoResponse>> ExecuteAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
