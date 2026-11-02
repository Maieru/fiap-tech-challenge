using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ListarOrdensServico;

public interface IListarOrdensServicoUseCase
{
    Task<Result<ListarOrdensServicoResponse>> ExecuteAsync(ListarOrdensServicoCommand command, CancellationToken cancellationToken = default);
}
