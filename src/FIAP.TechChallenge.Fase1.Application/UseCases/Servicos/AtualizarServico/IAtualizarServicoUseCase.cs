using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.AtualizarServico;

public interface IAtualizarServicoUseCase
{
    Task<Result<AtualizarServicoResponse>> ExecuteAsync(AtualizarServicoCommand command, CancellationToken cancellationToken = default);
}
