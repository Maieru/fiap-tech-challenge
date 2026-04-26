using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.ExcluirPecaInsumo;

public interface IExcluirPecaInsumoUseCase
{
    Task<Result<ExcluirPecaInsumoResponse>> ExecuteAsync(ExcluirPecaInsumoCommand command, CancellationToken cancellationToken = default);
}
