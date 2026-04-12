using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.AtualizarPecaInsumo;

public interface IAtualizarPecaInsumoUseCase
{
    Task<Result<AtualizarPecaInsumoResponse>> ExecuteAsync(AtualizarPecaInsumoCommand command, CancellationToken cancellationToken = default);
}
