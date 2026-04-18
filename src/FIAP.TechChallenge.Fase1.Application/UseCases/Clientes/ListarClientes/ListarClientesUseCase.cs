using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;

public sealed class ListarClientesUseCase(IClienteRepository clienteRepository) : IListarClientesUseCase
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<Result<ListarClientesResponse>> ExecuteAsync(ListarClientesCommand command, CancellationToken cancellationToken = default)
    {
        var paginationValidationResult = ValidatePagination(command.PageNumber, command.PageSize);

        if (!paginationValidationResult.IsSuccess)
            return Result<ListarClientesResponse>.Failure(paginationValidationResult.Error);

        var filtersCount = 0;

        if (!string.IsNullOrWhiteSpace(command.Cpf))
            filtersCount++;

        if (!string.IsNullOrWhiteSpace(command.Cnpj))
            filtersCount++;

        if (filtersCount > 1)
            return Result<ListarClientesResponse>.Failure(new Error("Informe apenas um filtro por vez: cpf ou cnpj."));

        if (!string.IsNullOrWhiteSpace(command.Cpf))
            return await GetByCpfAsync(command.Cpf, cancellationToken);

        if (!string.IsNullOrWhiteSpace(command.Cnpj))
            return await GetByCnpjAsync(command.Cnpj, cancellationToken);

        var pagedResult = await _clienteRepository.GetPagedAsync(command.PageNumber, command.PageSize, cancellationToken);

        if (!pagedResult.IsSuccess)
            return Result<ListarClientesResponse>.Failure(pagedResult.Error);

        var response = new ListarClientesResponse
        {
            PageNumber = command.PageNumber,
            PageSize = command.PageSize,
            TotalItems = pagedResult.Value.TotalItems,
            Clientes = pagedResult.Value.Clientes.Select(ToItemResponse).ToArray()
        };

        return Result<ListarClientesResponse>.Success(response);
    }

    private async Task<Result<ListarClientesResponse>> GetByCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        var cpfResult = Cpf.Create(cpf);

        if (!cpfResult.IsSuccess || cpfResult.Value is null)
            return Result<ListarClientesResponse>.Failure(cpfResult.Error);

        var clienteResult = await _clienteRepository.GetByCpfAsync(cpfResult.Value.Unformatted, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<ListarClientesResponse>.Failure(clienteResult.Error);

        return Result<ListarClientesResponse>.Success(CreateSingleItemResponse(clienteResult.Value));
    }

    private async Task<Result<ListarClientesResponse>> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken)
    {
        var cnpjResult = Cnpj.Create(cnpj);

        if (!cnpjResult.IsSuccess || cnpjResult.Value is null)
            return Result<ListarClientesResponse>.Failure(cnpjResult.Error);

        var clienteResult = await _clienteRepository.GetByCnpjAsync(cnpjResult.Value.Unformatted, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<ListarClientesResponse>.Failure(clienteResult.Error);

        return Result<ListarClientesResponse>.Success(CreateSingleItemResponse(clienteResult.Value));
    }

    private static Result<bool> ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0)
            return Result<bool>.Failure(new Error("O número da página deve ser maior que zero."));

        if (pageSize <= 0)
            return Result<bool>.Failure(new Error("O tamanho da página deve ser maior que zero."));

        return Result<bool>.Success(true);
    }

    private static ListarClientesResponse CreateSingleItemResponse(Cliente cliente)
    {
        return new ListarClientesResponse
        {
            PageNumber = 1,
            PageSize = 1,
            TotalItems = 1,
            Clientes = [ToItemResponse(cliente)]
        };
    }

    private static ListarClienteItemResponse ToItemResponse(Cliente cliente)
    {
        return new ListarClienteItemResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Cpf = cliente.Cpf?.Formatted,
            Cnpj = cliente.Cnpj?.Formatted,
            Telefone = cliente.Telefone.Formatted,
            Email = cliente.Email?.Value
        };
    }
}
