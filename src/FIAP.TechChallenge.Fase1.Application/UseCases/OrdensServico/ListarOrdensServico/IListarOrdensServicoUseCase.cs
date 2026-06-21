using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;

public interface IListarOrdensServicoUseCase
{
    Task<Result<ListarOrdensServicoResponse>> ExecuteAsync(ListarOrdensServicoCommand command, CancellationToken cancellationToken = default);
}

