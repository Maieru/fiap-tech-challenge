using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ExcluirCliente;

public interface IExcluirClienteUseCase
{
    Task<Result<ExcluirClienteResponse>> ExecuteAsync(ExcluirClienteCommand command, CancellationToken cancellationToken = default);
}

