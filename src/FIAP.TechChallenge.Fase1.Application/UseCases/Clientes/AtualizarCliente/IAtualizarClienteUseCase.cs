using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;

public interface IAtualizarClienteUseCase
{
    Task<Result<AtualizarClienteResponse>> ExecuteAsync(AtualizarClienteCommand command, CancellationToken cancellationToken = default);
}
