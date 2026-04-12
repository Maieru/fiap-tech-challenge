using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.ListarPecasInsumos;

public interface IListarPecasInsumosUseCase
{
    Task<Result<ListarPecasInsumosResponse>> ExecuteAsync(ListarPecasInsumosCommand command, CancellationToken cancellationToken = default);
}
