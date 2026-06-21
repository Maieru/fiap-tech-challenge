using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.VerificarTempoMedioServico;

public interface IVerificarTempoMedioServicoUseCase
{
    Task<Result<VerificarTempoMedioServicoResponse>> ExecuteAsync(VerificarTempoMedioServicoCommand command, CancellationToken cancellationToken = default);
}

