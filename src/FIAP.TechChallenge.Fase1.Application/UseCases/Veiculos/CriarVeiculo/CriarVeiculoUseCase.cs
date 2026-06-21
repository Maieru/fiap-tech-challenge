using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;

public sealed class CriarVeiculoUseCase(IVeiculoRepository veiculoRepository, IClienteRepository clienteRepository) : ICriarVeiculoUseCase
{
    private readonly IVeiculoRepository _veiculoRepository = veiculoRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<Result<CriarVeiculoResponse>> ExecuteAsync(CriarVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        var placaResult = CreatePlaca(command.Placa);

        if (!placaResult.IsSuccess || placaResult.Value is null)
            return Result<CriarVeiculoResponse>.Failure(placaResult.Error);

        var clienteResult = await _clienteRepository.GetByIdAsync(command.ClienteId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<CriarVeiculoResponse>.Failure(clienteResult.Error);

        var placaJaExiste = await _veiculoRepository.ExistsByPlacaAsync(placaResult.Value.Unformatted, cancellationToken);

        if (placaJaExiste)
            return Result<CriarVeiculoResponse>.Failure(new Error("Ja existe um veiculo cadastrado com esta placa."));

        var veiculoResult = Veiculo.Create(command.ClienteId, placaResult.Value, command.Marca, command.Modelo, command.Ano);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<CriarVeiculoResponse>.Failure(veiculoResult.Error);

        var veiculo = veiculoResult.Value;

        await _veiculoRepository.AddAsync(veiculo, cancellationToken);

        return Result<CriarVeiculoResponse>.Success(new CriarVeiculoResponse
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

