using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.SolicitarCancelamento;

public interface ISolicitarCancelamentoUseCase
{
    Task<Result<DecisaoExecucaoResponse>> ExecuteAsync(SolicitarCancelamentoCommand command, CancellationToken cancellationToken = default);
}
