using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarHistorico;

public interface IConsultarHistoricoUseCase
{
    Task<Result<IReadOnlyCollection<HistoricoOrdem>>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class ConsultarHistoricoUseCase(IOrdemServicoRepository ordens, IHistoricoOrdemRepository historico) : IConsultarHistoricoUseCase
{
    public async Task<Result<IReadOnlyCollection<HistoricoOrdem>>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return Result<IReadOnlyCollection<HistoricoOrdem>>.Failure(new Error("O identificador da ordem de servico deve ser valido."));
        var ordem = await ordens.GetByIdAsync(id, cancellationToken);
        if (!ordem.IsSuccess) return Result<IReadOnlyCollection<HistoricoOrdem>>.Failure(ordem.Error);
        return Result<IReadOnlyCollection<HistoricoOrdem>>.Success(await historico.GetByOrdemAsync(id, cancellationToken));
    }
}
