using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.IncluirPecaInsumo;

public interface IIncluirPecaInsumoUseCase
{
    Task<Result<IncluirPecaInsumoResponse>> ExecuteAsync(IncluirPecaInsumoCommand command, CancellationToken cancellationToken = default);
}
