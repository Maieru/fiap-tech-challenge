using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Veiculos.ExcluirVeiculo;

public interface IExcluirVeiculoUseCase
{
    Task<Result<ExcluirVeiculoResponse>> ExecuteAsync(ExcluirVeiculoCommand command, CancellationToken cancellationToken = default);
}
