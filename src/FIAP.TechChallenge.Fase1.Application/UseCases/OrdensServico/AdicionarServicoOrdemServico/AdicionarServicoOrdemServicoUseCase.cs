using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;

public sealed class AdicionarServicoOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IServicoRepository servicoRepository,
    IServicoDaOrdemDeServicoRepository servicoDaOrdemDeServicoRepository) : IAdicionarServicoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IServicoRepository _servicoRepository = servicoRepository;
    private readonly IServicoDaOrdemDeServicoRepository _servicoDaOrdemDeServicoRepository = servicoDaOrdemDeServicoRepository;

    public async Task<Result<AdicionarServicoOrdemServicoResponse>> ExecuteAsync(AdicionarServicoOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<AdicionarServicoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var validacaoAdicaoServicoResult = ordemServicoResult.Value.ValidarAdicaoServico();

        if (!validacaoAdicaoServicoResult.IsSuccess)
            return Result<AdicionarServicoOrdemServicoResponse>.Failure(validacaoAdicaoServicoResult.Error);

        var servicoResult = await _servicoRepository.GetByIdAsync(command.ServicoId, cancellationToken);

        if (!servicoResult.IsSuccess || servicoResult.Value is null)
            return Result<AdicionarServicoOrdemServicoResponse>.Failure(servicoResult.Error);

        var servicoDaOrdemDeServicoResult = ServicoDaOrdemDeServico.Create(command.OrdemServicoId, servicoResult.Value, command.Quantidade);

        if (!servicoDaOrdemDeServicoResult.IsSuccess || servicoDaOrdemDeServicoResult.Value is null)
            return Result<AdicionarServicoOrdemServicoResponse>.Failure(servicoDaOrdemDeServicoResult.Error);

        var servicoDaOrdemDeServico = servicoDaOrdemDeServicoResult.Value;

        await _servicoDaOrdemDeServicoRepository.AddAsync(servicoDaOrdemDeServico, cancellationToken);

        return Result<AdicionarServicoOrdemServicoResponse>.Success(new AdicionarServicoOrdemServicoResponse
        {
            Id = servicoDaOrdemDeServico.Id,
            OrdemServicoId = servicoDaOrdemDeServico.OrdemServicoId,
            ServicoId = servicoDaOrdemDeServico.ServicoId,
            Descricao = servicoDaOrdemDeServico.Descricao,
            ValorUnitario = servicoDaOrdemDeServico.ValorUnitario,
            Quantidade = servicoDaOrdemDeServico.Quantidade,
            ValorTotal = servicoDaOrdemDeServico.ValorTotal
        });
    }
}
