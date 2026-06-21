using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;

public interface IAtualizarVeiculoUseCase
{
    Task<Result<AtualizarVeiculoResponse>> ExecuteAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken = default);
}

