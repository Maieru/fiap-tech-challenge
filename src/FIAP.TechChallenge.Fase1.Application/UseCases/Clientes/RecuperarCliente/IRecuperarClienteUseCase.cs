using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.RecuperarCliente;

public interface IRecuperarClienteUseCase
{
    Task<Result<RecuperarClienteResponse>> ExecuteAsync(RecuperarClienteCommand command, CancellationToken cancellationToken = default);
}
