using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.ExcluirServico;

public interface IExcluirServicoUseCase
{
    Task<Result<ExcluirServicoResponse>> ExecuteAsync(ExcluirServicoCommand command, CancellationToken cancellationToken = default);
}
