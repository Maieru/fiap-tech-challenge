using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;

public sealed class ConcluirServicoOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IServicoDaOrdemDeServicoRepository servicoDaOrdemDeServicoRepository) : IConcluirServicoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IServicoDaOrdemDeServicoRepository _servicoDaOrdemDeServicoRepository = servicoDaOrdemDeServicoRepository;

    public async Task<Result<ConcluirServicoOrdemServicoResponse>> ExecuteAsync(
        ConcluirServicoOrdemServicoCommand command,
        CancellationToken cancellationToken = default)
    {
        var servicoDaOrdemResult = await _servicoDaOrdemDeServicoRepository.GetByIdAsync(command.ServicoDaOrdemDeServicoId, cancellationToken);

        if (!servicoDaOrdemResult.IsSuccess || servicoDaOrdemResult.Value is null)
            return Result<ConcluirServicoOrdemServicoResponse>.Failure(servicoDaOrdemResult.Error);

        var servicoDaOrdem = servicoDaOrdemResult.Value;

        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(servicoDaOrdem.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<ConcluirServicoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var validacaoConclusaoServicoResult = ordemServicoResult.Value.ValidarConclusaoServico();

        if (!validacaoConclusaoServicoResult.IsSuccess)
            return Result<ConcluirServicoOrdemServicoResponse>.Failure(validacaoConclusaoServicoResult.Error);

        var concluirServicoResult = servicoDaOrdem.Concluir(command.TempoGastoMinutos);

        if (!concluirServicoResult.IsSuccess)
            return Result<ConcluirServicoOrdemServicoResponse>.Failure(concluirServicoResult.Error);

        await _servicoDaOrdemDeServicoRepository.UpdateAsync(servicoDaOrdem, cancellationToken);

        return Result<ConcluirServicoOrdemServicoResponse>.Success(new ConcluirServicoOrdemServicoResponse
        {
            Id = servicoDaOrdem.Id,
            OrdemServicoId = servicoDaOrdem.OrdemServicoId,
            ServicoId = servicoDaOrdem.ServicoId,
            TempoGastoMinutos = servicoDaOrdem.TempoGastoMinutos!.Value,
            Concluido = servicoDaOrdem.Concluido
        });
    }
}

