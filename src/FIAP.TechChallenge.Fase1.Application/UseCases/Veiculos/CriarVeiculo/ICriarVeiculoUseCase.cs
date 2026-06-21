using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;

public interface ICriarVeiculoUseCase
{
    Task<Result<CriarVeiculoResponse>> ExecuteAsync(CriarVeiculoCommand command, CancellationToken cancellationToken = default);
}

