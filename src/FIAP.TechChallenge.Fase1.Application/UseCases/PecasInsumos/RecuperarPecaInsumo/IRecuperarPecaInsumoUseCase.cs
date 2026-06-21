using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.RecuperarPecaInsumo;

public interface IRecuperarPecaInsumoUseCase
{
    Task<Result<RecuperarPecaInsumoResponse>> ExecuteAsync(RecuperarPecaInsumoCommand command, CancellationToken cancellationToken = default);
}

