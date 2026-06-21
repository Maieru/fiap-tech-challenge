using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;

public interface IListarVeiculosUseCase
{
    Task<Result<ListarVeiculosResponse>> ExecuteAsync(ListarVeiculosCommand command, CancellationToken cancellationToken = default);
}

