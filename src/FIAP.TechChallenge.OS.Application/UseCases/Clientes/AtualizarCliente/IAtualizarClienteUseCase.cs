using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Clientes.AtualizarCliente;

public interface IAtualizarClienteUseCase
{
    Task<Result<AtualizarClienteResponse>> ExecuteAsync(AtualizarClienteCommand command, CancellationToken cancellationToken = default);
}
