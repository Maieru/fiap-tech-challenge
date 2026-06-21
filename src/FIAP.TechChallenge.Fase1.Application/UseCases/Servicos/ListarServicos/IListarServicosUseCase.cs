using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.ListarServicos;

public interface IListarServicosUseCase
{
    Task<Result<ListarServicosResponse>> ExecuteAsync(ListarServicosCommand command, CancellationToken cancellationToken = default);
}

