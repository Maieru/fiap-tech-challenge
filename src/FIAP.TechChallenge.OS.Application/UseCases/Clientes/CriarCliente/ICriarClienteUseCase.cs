using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Clientes.CriarCliente;

public interface ICriarClienteUseCase
{
    Task<Result<CriarClienteResponse>> ExecuteAsync(CriarClienteCommand command, CancellationToken cancellationToken = default);
}
