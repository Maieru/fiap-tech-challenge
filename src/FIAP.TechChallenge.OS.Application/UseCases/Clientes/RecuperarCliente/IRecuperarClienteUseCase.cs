using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Clientes.RecuperarCliente;

public interface IRecuperarClienteUseCase
{
    Task<Result<RecuperarClienteResponse>> ExecuteAsync(RecuperarClienteCommand command, CancellationToken cancellationToken = default);
}
