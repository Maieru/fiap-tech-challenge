using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.RecuperarCliente;

public sealed class RecuperarClienteUseCase(IClienteRepository clienteRepository) : IRecuperarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<Result<RecuperarClienteResponse>> ExecuteAsync(RecuperarClienteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ClienteId == Guid.Empty)
            return Result<RecuperarClienteResponse>.Failure(new Error("O identificador do cliente deve ser valido."));

        var clienteResult = await _clienteRepository.GetByIdAsync(command.ClienteId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<RecuperarClienteResponse>.Failure(clienteResult.Error);

        return Result<RecuperarClienteResponse>.Success(ToResponse(clienteResult.Value));
    }

    private static RecuperarClienteResponse ToResponse(Cliente cliente)
    {
        return new RecuperarClienteResponse
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
