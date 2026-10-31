using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Interfaces;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.SolicitarFluxo;

public interface ISolicitarFluxoUseCase
{
    Task<Result<SolicitacaoOrdem>> ExecuteAsync(Guid ordemId, string acao, string? operadorId, CancellationToken ct);
}
public sealed class SolicitarFluxoUseCase(IFluxoOrdemRepository repository) : ISolicitarFluxoUseCase
{
    public Task<Result<SolicitacaoOrdem>> ExecuteAsync(Guid ordemId, string acao, string? operadorId, CancellationToken ct)
    {
        if (ordemId == Guid.Empty)
            return Task.FromResult(Result<SolicitacaoOrdem>.Failure(new Error("O id da ordem de servico e invalido.")));

        if (string.IsNullOrWhiteSpace(operadorId))
            return Task.FromResult(Result<SolicitacaoOrdem>.Failure(new Error("O token deve identificar o operador.", ErrorCode.Forbidden)));

        return repository.SolicitarAsync(ordemId, acao, operadorId, ct);
    }
}
