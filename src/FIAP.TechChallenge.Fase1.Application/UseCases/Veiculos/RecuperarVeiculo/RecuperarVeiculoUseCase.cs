using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;

public sealed class RecuperarVeiculoUseCase(IVeiculoRepository veiculoRepository) : IRecuperarVeiculoUseCase
{
    private readonly IVeiculoRepository _veiculoRepository = veiculoRepository;

    public async Task<Result<RecuperarVeiculoResponse>> ExecuteAsync(RecuperarVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.VeiculoId == Guid.Empty)
            return Result<RecuperarVeiculoResponse>.Failure(new Error("O identificador do veiculo deve ser valido."));

        var veiculoResult = await _veiculoRepository.GetByIdAsync(command.VeiculoId, cancellationToken);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<RecuperarVeiculoResponse>.Failure(veiculoResult.Error);

        return Result<RecuperarVeiculoResponse>.Success(ToResponse(veiculoResult.Value));
    }

    private static RecuperarVeiculoResponse ToResponse(Veiculo veiculo)
    {
        return new RecuperarVeiculoResponse
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            Placa = veiculo.Placa.Value,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano
        };
    }
}

