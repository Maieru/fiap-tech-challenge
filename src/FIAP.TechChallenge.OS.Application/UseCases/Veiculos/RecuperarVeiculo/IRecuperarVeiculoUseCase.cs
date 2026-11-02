using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Veiculos.RecuperarVeiculo;

public interface IRecuperarVeiculoUseCase
{
    Task<Result<RecuperarVeiculoResponse>> ExecuteAsync(RecuperarVeiculoCommand command, CancellationToken cancellationToken = default);
}
