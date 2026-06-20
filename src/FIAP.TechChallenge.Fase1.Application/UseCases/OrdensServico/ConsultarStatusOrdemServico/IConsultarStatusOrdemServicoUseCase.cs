using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConsultarStatusOrdemServico;

public interface IConsultarStatusOrdemServicoUseCase
{
    Task<Result<ConsultarStatusOrdemServicoResponse>> ExecuteAsync(ConsultarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
