using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Veiculos.CriarVeiculo;

public interface ICriarVeiculoUseCase
{
    Task<Result<CriarVeiculoResponse>> ExecuteAsync(CriarVeiculoCommand command, CancellationToken cancellationToken = default);
}
