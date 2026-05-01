using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using System.Transactions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CancelarOrdemServico;

public sealed class CancelarOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IPecaInsumoRepository pecaInsumoRepository,
    IPecaOuInsumoDaOrdemDeServicoRepository pecaOuInsumoDaOrdemDeServicoRepository) : ICancelarOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IPecaInsumoRepository _pecaInsumoRepository = pecaInsumoRepository;
    private readonly IPecaOuInsumoDaOrdemDeServicoRepository _pecaOuInsumoDaOrdemDeServicoRepository = pecaOuInsumoDaOrdemDeServicoRepository;

    public async Task<Result<CancelarOrdemServicoResponse>> ExecuteAsync(CancelarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<CancelarOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var cancelarResult = ordemServico.Cancelar();

        if (!cancelarResult.IsSuccess)
            return Result<CancelarOrdemServicoResponse>.Failure(cancelarResult.Error);

        var pecasOuInsumosResult = await _pecaOuInsumoDaOrdemDeServicoRepository.GetByOrdemServicoIdAsync(ordemServico.Id, cancellationToken);

        if (!pecasOuInsumosResult.IsSuccess || pecasOuInsumosResult.Value is null)
            return Result<CancelarOrdemServicoResponse>.Failure(pecasOuInsumosResult.Error);

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        foreach (var pecaOuInsumoDaOrdem in pecasOuInsumosResult.Value)
        {
            var pecaInsumoResult = await _pecaInsumoRepository.GetByIdAsync(pecaOuInsumoDaOrdem.PecaInsumoId, cancellationToken);

            if (!pecaInsumoResult.IsSuccess || pecaInsumoResult.Value is null)
                return Result<CancelarOrdemServicoResponse>.Failure(pecaInsumoResult.Error);

            var pecaInsumo = pecaInsumoResult.Value;
            var addEstoqueResult = pecaInsumo.AddEstoque(pecaOuInsumoDaOrdem.Quantidade);

            if (!addEstoqueResult.IsSuccess)
                return Result<CancelarOrdemServicoResponse>.Failure(addEstoqueResult.Error);

            await _pecaInsumoRepository.UpdateAsync(pecaInsumo, cancellationToken);
        }

        await _ordemServicoRepository.UpdateAsync(ordemServico, cancellationToken);

        transactionScope.Complete();

        return Result<CancelarOrdemServicoResponse>.Success(new CancelarOrdemServicoResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status
        });
    }
}
