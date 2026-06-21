using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.EntregarOrdemServico;

public interface IEntregarOrdemServicoUseCase
{
    Task<Result<EntregarOrdemServicoResponse>> ExecuteAsync(EntregarOrdemServicoCommand command, CancellationToken cancellationToken = default);
}

