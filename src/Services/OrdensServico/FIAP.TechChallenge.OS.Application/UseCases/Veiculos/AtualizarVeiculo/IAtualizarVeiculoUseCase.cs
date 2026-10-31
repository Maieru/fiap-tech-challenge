using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Veiculos.AtualizarVeiculo;

public interface IAtualizarVeiculoUseCase
{
    Task<Result<AtualizarVeiculoResponse>> ExecuteAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken = default);
}
