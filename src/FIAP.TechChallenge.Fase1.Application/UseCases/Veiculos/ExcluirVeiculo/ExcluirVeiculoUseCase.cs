using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ExcluirVeiculo;

public sealed class ExcluirVeiculoUseCase(IVeiculoRepository veiculoRepository) : IExcluirVeiculoUseCase
{
    private readonly IVeiculoRepository _veiculoRepository = veiculoRepository;

    public async Task<Result<ExcluirVeiculoResponse>> ExecuteAsync(ExcluirVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        var veiculoResult = await _veiculoRepository.GetByIdAsync(command.Id, cancellationToken);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<ExcluirVeiculoResponse>.Failure(veiculoResult.Error);

        await _veiculoRepository.DeleteAsync(veiculoResult.Value, cancellationToken);

        return Result<ExcluirVeiculoResponse>.Success(new ExcluirVeiculoResponse { Id = command.Id });
    }
}

