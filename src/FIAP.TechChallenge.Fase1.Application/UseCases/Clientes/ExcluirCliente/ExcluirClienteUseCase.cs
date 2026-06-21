using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ExcluirCliente;

public sealed class ExcluirClienteUseCase(IClienteRepository clienteRepository) : IExcluirClienteUseCase
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<Result<ExcluirClienteResponse>> ExecuteAsync(ExcluirClienteCommand command, CancellationToken cancellationToken = default)
    {
        var clienteResult = await _clienteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<ExcluirClienteResponse>.Failure(clienteResult.Error);

        await _clienteRepository.DeleteAsync(clienteResult.Value, cancellationToken);

        return Result<ExcluirClienteResponse>.Success(new ExcluirClienteResponse { Id = command.Id });
    }
}

