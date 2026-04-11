using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;

public interface IListarClientesUseCase
{
    Task<Result<ListarClientesResponse>> ExecuteAsync(ListarClientesCommand command, CancellationToken cancellationToken = default);
}
