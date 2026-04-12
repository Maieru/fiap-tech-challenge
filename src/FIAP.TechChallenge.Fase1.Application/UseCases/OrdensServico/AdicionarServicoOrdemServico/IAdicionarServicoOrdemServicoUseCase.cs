using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;

public interface IAdicionarServicoOrdemServicoUseCase
{
    Task<Result<AdicionarServicoOrdemServicoResponse>> ExecuteAsync(AdicionarServicoOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
