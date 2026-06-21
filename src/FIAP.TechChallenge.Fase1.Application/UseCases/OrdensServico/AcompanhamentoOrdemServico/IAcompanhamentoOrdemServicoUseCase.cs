using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AcompanhamentoOrdemServico;

public interface IAcompanhamentoOrdemServicoUseCase
{
    Task<Result<AcompanhamentoOrdemServicoResponse>> ExecuteAsync(AcompanhamentoOrdemServicoCommand command, CancellationToken cancellationToken = default);
}

