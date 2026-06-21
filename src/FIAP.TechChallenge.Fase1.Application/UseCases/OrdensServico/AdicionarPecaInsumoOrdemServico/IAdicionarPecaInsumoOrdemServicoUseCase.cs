using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;

public interface IAdicionarPecaInsumoOrdemServicoUseCase
{
    Task<Result<AdicionarPecaInsumoOrdemServicoResponse>> ExecuteAsync(AdicionarPecaInsumoOrdemServicoCommand command, CancellationToken cancellationToken = default);
}

