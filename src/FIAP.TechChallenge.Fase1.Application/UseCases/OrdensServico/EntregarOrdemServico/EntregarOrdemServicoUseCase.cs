using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.EntregarOrdemServico;

public sealed class EntregarOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IEntregarOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;

    public async Task<Result<EntregarOrdemServicoResponse>> ExecuteAsync(EntregarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<EntregarOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var entregarResult = ordemServico.Entregar();

        if (!entregarResult.IsSuccess)
            return Result<EntregarOrdemServicoResponse>.Failure(entregarResult.Error);

        await _ordemServicoRepository.UpdateAsync(ordemServico, cancellationToken);

        return Result<EntregarOrdemServicoResponse>.Success(new EntregarOrdemServicoResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            DataEntrega = ordemServico.DataEntrega!.Value
        });
    }
}

