using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.CadastrarServico;

public interface ICadastrarServicoUseCase
{
    Task<Result<CadastrarServicoResponse>> ExecuteAsync(CadastrarServicoCommand command, CancellationToken cancellationToken = default);
}
