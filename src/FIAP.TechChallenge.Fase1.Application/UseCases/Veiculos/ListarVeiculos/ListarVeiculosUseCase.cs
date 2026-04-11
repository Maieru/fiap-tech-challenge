using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;

public sealed class ListarVeiculosUseCase(IVeiculoRepository veiculoRepository) : IListarVeiculosUseCase
{
    private readonly IVeiculoRepository _veiculoRepository = veiculoRepository;

    public async Task<Result<ListarVeiculosResponse>> ExecuteAsync(ListarVeiculosCommand command, CancellationToken cancellationToken = default)
    {
        var paginationValidationResult = ValidatePagination(command.PageNumber, command.PageSize);

        if (!paginationValidationResult.IsSuccess)
            return Result<ListarVeiculosResponse>.Failure(paginationValidationResult.Error);

        var filtersCount = 0;

        if (!string.IsNullOrWhiteSpace(command.Placa))
            filtersCount++;

        if (command.ClienteId.HasValue)
            filtersCount++;

        if (filtersCount > 1)
            return Result<ListarVeiculosResponse>.Failure(new Error("Informe apenas um filtro por vez: placa ou clienteId."));

        if (!string.IsNullOrWhiteSpace(command.Placa))
            return await GetByPlacaAsync(command.Placa, cancellationToken);

        if (command.ClienteId.HasValue)
            return await GetByClienteIdAsync(command.ClienteId.Value, cancellationToken);

        var pagedResult = await _veiculoRepository.GetPagedAsync(command.PageNumber, command.PageSize, cancellationToken);

        if (!pagedResult.IsSuccess)
            return Result<ListarVeiculosResponse>.Failure(pagedResult.Error);

        return Result<ListarVeiculosResponse>.Success(new ListarVeiculosResponse
        {
            PageNumber = command.PageNumber,
            PageSize = command.PageSize,
            TotalItems = pagedResult.Value.TotalItems,
            Veiculos = pagedResult.Value.Veiculos.Select(ToItemResponse).ToArray()
        });
    }

    private async Task<Result<ListarVeiculosResponse>> GetByPlacaAsync(string placa, CancellationToken cancellationToken)
    {
        var placaResult = Placa.Create(placa);

        if (!placaResult.IsSuccess || placaResult.Value is null)
            return Result<ListarVeiculosResponse>.Failure(placaResult.Error);

        var veiculoResult = await _veiculoRepository.GetByPlacaAsync(placaResult.Value.Unformatted, cancellationToken);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<ListarVeiculosResponse>.Failure(veiculoResult.Error);

        return Result<ListarVeiculosResponse>.Success(CreateSingleItemResponse(veiculoResult.Value));
    }

    private async Task<Result<ListarVeiculosResponse>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        if (clienteId == Guid.Empty)
            return Result<ListarVeiculosResponse>.Failure(new Error("O identificador do cliente deve ser válido."));

        var veiculosResult = await _veiculoRepository.GetByClienteIdAsync(clienteId, cancellationToken);

        if (!veiculosResult.IsSuccess)
            return Result<ListarVeiculosResponse>.Failure(veiculosResult.Error);

        return Result<ListarVeiculosResponse>.Success(new ListarVeiculosResponse
        {
            PageNumber = 1,
            PageSize = veiculosResult.Value.Veiculos.Count,
            TotalItems = veiculosResult.Value.TotalItems,
            Veiculos = veiculosResult.Value.Veiculos.Select(ToItemResponse).ToArray()
        });
    }

    private static Result<bool> ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0)
            return Result<bool>.Failure(new Error("O número da página deve ser maior que zero."));

        if (pageSize <= 0)
            return Result<bool>.Failure(new Error("O tamanho da página deve ser maior que zero."));

        return Result<bool>.Success(true);
    }

    private static ListarVeiculosResponse CreateSingleItemResponse(Veiculo veiculo)
    {
        return new ListarVeiculosResponse
        {
            PageNumber = 1,
            PageSize = 1,
            TotalItems = 1,
            Veiculos = [ToItemResponse(veiculo)]
        };
    }

    private static ListarVeiculoItemResponse ToItemResponse(Veiculo veiculo)
    {
        return new ListarVeiculoItemResponse
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
