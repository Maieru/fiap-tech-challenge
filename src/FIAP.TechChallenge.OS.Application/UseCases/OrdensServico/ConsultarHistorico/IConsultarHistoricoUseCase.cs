using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarHistorico;

public interface IConsultarHistoricoUseCase
{
    Task<Result<IReadOnlyCollection<HistoricoOrdem>>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}
