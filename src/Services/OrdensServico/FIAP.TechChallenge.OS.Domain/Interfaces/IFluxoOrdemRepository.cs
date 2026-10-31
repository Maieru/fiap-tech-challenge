using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Domain.Interfaces;

public sealed record SolicitacaoOrdem(Guid SolicitacaoId, string Acao);
public interface IFluxoOrdemRepository
{
    Task<Result<SolicitacaoOrdem>> SolicitarAsync(Guid ordemId, string acao, string operadorId, CancellationToken ct);
}
