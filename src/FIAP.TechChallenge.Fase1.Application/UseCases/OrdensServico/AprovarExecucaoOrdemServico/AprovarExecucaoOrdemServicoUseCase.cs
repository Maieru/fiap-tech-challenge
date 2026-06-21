using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;

public sealed class AprovarExecucaoOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IAprovarExecucaoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;

    public async Task<Result<AprovarExecucaoOrdemServicoResponse>> ExecuteAsync(AprovarExecucaoOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<AprovarExecucaoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var aprovarOrcamentoResult = ordemServico.AprovarOrcamento();

        if (!aprovarOrcamentoResult.IsSuccess)
            return Result<AprovarExecucaoOrdemServicoResponse>.Failure(aprovarOrcamentoResult.Error);

        await _ordemServicoRepository.UpdateAsync(ordemServico, cancellationToken);

        return Result<AprovarExecucaoOrdemServicoResponse>.Success(new AprovarExecucaoOrdemServicoResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            DataInicioExecucao = ordemServico.DataInicioExecucao!.Value
        });
    }
}

