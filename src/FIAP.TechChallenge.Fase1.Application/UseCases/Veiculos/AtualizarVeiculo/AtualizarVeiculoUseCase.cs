using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;

public sealed class AtualizarVeiculoUseCase(IVeiculoRepository veiculoRepository) : IAtualizarVeiculoUseCase
{
    private readonly IVeiculoRepository _veiculoRepository = veiculoRepository;

    public async Task<Result<AtualizarVeiculoResponse>> ExecuteAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        var veiculoResult = await _veiculoRepository.GetByIdAsync(command.Id, cancellationToken);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<AtualizarVeiculoResponse>.Failure(veiculoResult.Error);

        var placaResult = CreatePlaca(command.Placa);

        if (!placaResult.IsSuccess || placaResult.Value is null)
            return Result<AtualizarVeiculoResponse>.Failure(placaResult.Error);

        var veiculo = veiculoResult.Value;
        var placaFoiAlterada = !string.Equals(veiculo.Placa.Unformatted, placaResult.Value.Unformatted, StringComparison.OrdinalIgnoreCase);

        if (placaFoiAlterada)
        {
            var placaJaExiste = await _veiculoRepository.ExistsByPlacaAsync(placaResult.Value.Unformatted, cancellationToken);

            if (placaJaExiste)
                return Result<AtualizarVeiculoResponse>.Failure(new Error("Ja existe um veiculo cadastrado com esta placa."));
        }

        var updatePlacaResult = veiculo.UpdatePlaca(placaResult.Value);

        if (!updatePlacaResult.IsSuccess)
            return Result<AtualizarVeiculoResponse>.Failure(updatePlacaResult.Error);

        var updateMarcaResult = veiculo.UpdateMarca(command.Marca);

        if (!updateMarcaResult.IsSuccess)
            return Result<AtualizarVeiculoResponse>.Failure(updateMarcaResult.Error);

        var updateModeloResult = veiculo.UpdateModelo(command.Modelo);

        if (!updateModeloResult.IsSuccess)
            return Result<AtualizarVeiculoResponse>.Failure(updateModeloResult.Error);

        var updateAnoResult = veiculo.UpdateAno(command.Ano);

        if (!updateAnoResult.IsSuccess)
            return Result<AtualizarVeiculoResponse>.Failure(updateAnoResult.Error);

        await _veiculoRepository.UpdateAsync(veiculo, cancellationToken);

        return Result<AtualizarVeiculoResponse>.Success(new AtualizarVeiculoResponse
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            Placa = veiculo.Placa.Value,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano
        });
    }

    private static Result<Placa> CreatePlaca(string placa)
    {
        var placaResult = Placa.Create(placa);
        return !placaResult.IsSuccess || placaResult.Value is null
            ? Result<Placa>.Failure(placaResult.Error)
            : Result<Placa>.Success(placaResult.Value);
    }
}
