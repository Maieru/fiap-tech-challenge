using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;

public sealed class RecuperarOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IClienteRepository clienteRepository,
    IServicoDaOrdemDeServicoRepository servicoDaOrdemDeServicoRepository,
    IPecaOuInsumoDaOrdemDeServicoRepository pecaOuInsumoDaOrdemDeServicoRepository) : IRecuperarOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IServicoDaOrdemDeServicoRepository _servicoDaOrdemDeServicoRepository = servicoDaOrdemDeServicoRepository;
    private readonly IPecaOuInsumoDaOrdemDeServicoRepository _pecaOuInsumoDaOrdemDeServicoRepository = pecaOuInsumoDaOrdemDeServicoRepository;

    public async Task<Result<RecuperarOrdemServicoResponse>> ExecuteAsync(RecuperarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrdemServicoId == Guid.Empty)
            return Result<RecuperarOrdemServicoResponse>.Failure(new Error("O identificador da ordem de servico deve ser valido."));

        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<RecuperarOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var clienteResult = await _clienteRepository.GetByIdAsync(ordemServicoResult.Value.ClienteId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<RecuperarOrdemServicoResponse>.Failure(clienteResult.Error);

        var servicosDaOrdemResult = await _servicoDaOrdemDeServicoRepository.GetByOrdemServicoIdAsync(command.OrdemServicoId, cancellationToken);
        var pecasInsumosDaOrdemResult = await _pecaOuInsumoDaOrdemDeServicoRepository.GetByOrdemServicoIdAsync(command.OrdemServicoId, cancellationToken);

        if (!servicosDaOrdemResult.IsSuccess || servicosDaOrdemResult.Value is null)
            return Result<RecuperarOrdemServicoResponse>.Failure(servicosDaOrdemResult.Error);

        if (!pecasInsumosDaOrdemResult.IsSuccess || pecasInsumosDaOrdemResult.Value is null)
            return Result<RecuperarOrdemServicoResponse>.Failure(pecasInsumosDaOrdemResult.Error);

        var servicosDaOrdem = servicosDaOrdemResult.Value;
        var pecasInsumosDaOrdem = pecasInsumosDaOrdemResult.Value;

        var valorTotalServicos = servicosDaOrdem.Sum(x => x.ValorTotal);
        var valorTotalPecasInsumos = pecasInsumosDaOrdem.Sum(x => x.ValorTotal);

        return Result<RecuperarOrdemServicoResponse>.Success(new RecuperarOrdemServicoResponse
        {
            Id = ordemServicoResult.Value.Id,
            Token = clienteResult.Value.Cpf is null
                ? null
                : CpfAccessToken.Create(clienteResult.Value.Cpf, ordemServicoResult.Value.CodigoAprovacao),
            ClienteId = ordemServicoResult.Value.ClienteId,
            VeiculoId = ordemServicoResult.Value.VeiculoId,
            DescricaoProblema = ordemServicoResult.Value.DescricaoProblema,
            Status = ordemServicoResult.Value.Status,
            DataCriacao = ordemServicoResult.Value.DataCriacao,
            DataInicioDiagnostico = ordemServicoResult.Value.DataInicioDiagnostico,
            DataEnvioAprovacao = ordemServicoResult.Value.DataEnvioAprovacao,
            DataInicioExecucao = ordemServicoResult.Value.DataInicioExecucao,
            DataFinalizacao = ordemServicoResult.Value.DataFinalizacao,
            DataEntrega = ordemServicoResult.Value.DataEntrega,
            Servicos = servicosDaOrdem.Select(ToServicoItemResponse).ToArray(),
            PecasInsumos = pecasInsumosDaOrdem.Select(ToPecaInsumoItemResponse).ToArray(),
            ValorTotalServicos = valorTotalServicos,
            ValorTotalPecasInsumos = valorTotalPecasInsumos,
            ValorTotalOrdemServico = valorTotalServicos + valorTotalPecasInsumos
        });
    }

    private static RecuperarServicoDaOrdemServicoItemResponse ToServicoItemResponse(ServicoDaOrdemDeServico servicoDaOrdemDeServico)
    {
        return new RecuperarServicoDaOrdemServicoItemResponse
        {
            Id = servicoDaOrdemDeServico.Id,
            OrdemServicoId = servicoDaOrdemDeServico.OrdemServicoId,
            ServicoId = servicoDaOrdemDeServico.ServicoId,
            Descricao = servicoDaOrdemDeServico.Descricao,
            ValorUnitario = servicoDaOrdemDeServico.ValorUnitario,
            Quantidade = servicoDaOrdemDeServico.Quantidade,
            ValorTotal = servicoDaOrdemDeServico.ValorTotal,
            TempoGastoMinutos = servicoDaOrdemDeServico.TempoGastoMinutos,
            Concluido = servicoDaOrdemDeServico.Concluido
        };
    }

    private static RecuperarPecaInsumoDaOrdemServicoItemResponse ToPecaInsumoItemResponse(PecaOuInsumoDaOrdemDeServico pecaOuInsumoDaOrdemDeServico)
    {
        return new RecuperarPecaInsumoDaOrdemServicoItemResponse
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
        };
    }
}

