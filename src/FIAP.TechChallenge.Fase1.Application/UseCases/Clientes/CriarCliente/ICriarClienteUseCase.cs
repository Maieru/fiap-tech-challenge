using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;

public interface ICriarClienteUseCase
{
    Task<Result<CriarClienteResponse>> ExecuteAsync(CriarClienteCommand command, CancellationToken cancellationToken = default);
}
