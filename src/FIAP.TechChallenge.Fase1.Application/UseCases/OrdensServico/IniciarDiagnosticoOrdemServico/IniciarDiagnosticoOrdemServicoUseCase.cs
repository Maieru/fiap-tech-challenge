using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;

public sealed class IniciarDiagnosticoOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IIniciarDiagnosticoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;

    public async Task<Result<IniciarDiagnosticoOrdemServicoResponse>> ExecuteAsync(IniciarDiagnosticoOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<IniciarDiagnosticoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();

        if (!iniciarDiagnosticoResult.IsSuccess)
            return Result<IniciarDiagnosticoOrdemServicoResponse>.Failure(iniciarDiagnosticoResult.Error);

        await _ordemServicoRepository.UpdateAsync(ordemServico, cancellationToken);

        return Result<IniciarDiagnosticoOrdemServicoResponse>.Success(new IniciarDiagnosticoOrdemServicoResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            DataInicioDiagnostico = ordemServico.DataInicioDiagnostico!.Value
        });
    }
}

