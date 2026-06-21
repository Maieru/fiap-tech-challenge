using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.EntradaEstoquePecaInsumo;

public interface IEntradaEstoquePecaInsumoUseCase
{
    Task<Result<EntradaEstoquePecaInsumoResponse>> ExecuteAsync(EntradaEstoquePecaInsumoCommand command, CancellationToken cancellationToken = default);
}

