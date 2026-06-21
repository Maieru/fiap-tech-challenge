using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;

public interface IRecuperarVeiculoUseCase
{
    Task<Result<RecuperarVeiculoResponse>> ExecuteAsync(RecuperarVeiculoCommand command, CancellationToken cancellationToken = default);
}

