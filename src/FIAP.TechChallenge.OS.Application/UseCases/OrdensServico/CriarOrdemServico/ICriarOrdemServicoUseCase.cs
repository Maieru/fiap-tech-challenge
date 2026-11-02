using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.CriarOrdemServico;

public interface ICriarOrdemServicoUseCase
{
    Task<Result<CriarOrdemServicoResponse>> ExecuteAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
