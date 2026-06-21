using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ExcluirVeiculo;

public interface IExcluirVeiculoUseCase
{
    Task<Result<ExcluirVeiculoResponse>> ExecuteAsync(ExcluirVeiculoCommand command, CancellationToken cancellationToken = default);
}

