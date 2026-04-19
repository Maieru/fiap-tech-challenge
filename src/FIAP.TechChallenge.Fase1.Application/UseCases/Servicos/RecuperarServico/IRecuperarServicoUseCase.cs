using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.RecuperarServico;

public interface IRecuperarServicoUseCase
{
    Task<Result<RecuperarServicoResponse>> ExecuteAsync(RecuperarServicoCommand command, CancellationToken cancellationToken = default);
}
