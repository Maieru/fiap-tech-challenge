using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Veiculos.ListarVeiculos;

public interface IListarVeiculosUseCase
{
    Task<Result<ListarVeiculosResponse>> ExecuteAsync(ListarVeiculosCommand command, CancellationToken cancellationToken = default);
}
