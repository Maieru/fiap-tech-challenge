using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Clientes.ExcluirCliente;

public interface IExcluirClienteUseCase
{
    Task<Result<ExcluirClienteResponse>> ExecuteAsync(ExcluirClienteCommand command, CancellationToken cancellationToken = default);
}
