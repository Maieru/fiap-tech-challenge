using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;

public sealed class AdicionarPecaInsumoOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IPecaInsumoRepository pecaInsumoRepository,
    IPecaOuInsumoDaOrdemDeServicoRepository pecaOuInsumoDaOrdemDeServicoRepository) : IAdicionarPecaInsumoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IPecaInsumoRepository _pecaInsumoRepository = pecaInsumoRepository;
    private readonly IPecaOuInsumoDaOrdemDeServicoRepository _pecaOuInsumoDaOrdemDeServicoRepository = pecaOuInsumoDaOrdemDeServicoRepository;

    public async Task<Result<AdicionarPecaInsumoOrdemServicoResponse>> ExecuteAsync(AdicionarPecaInsumoOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<AdicionarPecaInsumoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var validacaoAdicaoPecaInsumoResult = ordemServicoResult.Value.ValidarAdicaoPecaInsumo();

        if (!validacaoAdicaoPecaInsumoResult.IsSuccess)
            return Result<AdicionarPecaInsumoOrdemServicoResponse>.Failure(validacaoAdicaoPecaInsumoResult.Error);

        var pecaInsumoResult = await _pecaInsumoRepository.GetByIdAsync(command.PecaInsumoId, cancellationToken);

        if (!pecaInsumoResult.IsSuccess || pecaInsumoResult.Value is null)
            return Result<AdicionarPecaInsumoOrdemServicoResponse>.Failure(pecaInsumoResult.Error);

        var pecaOuInsumoDaOrdemDeServicoResult = PecaOuInsumoDaOrdemDeServico.Create(command.OrdemServicoId, pecaInsumoResult.Value, command.Quantidade);

        if (!pecaOuInsumoDaOrdemDeServicoResult.IsSuccess || pecaOuInsumoDaOrdemDeServicoResult.Value is null)
            return Result<AdicionarPecaInsumoOrdemServicoResponse>.Failure(pecaOuInsumoDaOrdemDeServicoResult.Error);

        var pecaOuInsumoDaOrdemDeServico = pecaOuInsumoDaOrdemDeServicoResult.Value;

        await _pecaOuInsumoDaOrdemDeServicoRepository.AddAsync(pecaOuInsumoDaOrdemDeServico, cancellationToken);

        return Result<AdicionarPecaInsumoOrdemServicoResponse>.Success(new AdicionarPecaInsumoOrdemServicoResponse
        {
            Id = pecaOuInsumoDaOrdemDeServico.Id,
            OrdemServicoId = pecaOuInsumoDaOrdemDeServico.OrdemServicoId,
            PecaInsumoId = pecaOuInsumoDaOrdemDeServico.PecaInsumoId,
            Nome = pecaOuInsumoDaOrdemDeServico.Nome,
            Codigo = pecaOuInsumoDaOrdemDeServico.Codigo,
            Descricao = pecaOuInsumoDaOrdemDeServico.Descricao,
            PrecoUnitario = pecaOuInsumoDaOrdemDeServico.PrecoUnitario,
            Quantidade = pecaOuInsumoDaOrdemDeServico.Quantidade,
            ValorTotal = pecaOuInsumoDaOrdemDeServico.ValorTotal
        });
    }
}
