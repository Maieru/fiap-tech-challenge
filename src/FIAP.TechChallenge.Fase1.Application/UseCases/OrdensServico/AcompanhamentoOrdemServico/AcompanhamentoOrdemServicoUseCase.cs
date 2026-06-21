using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AcompanhamentoOrdemServico;

public sealed class AcompanhamentoOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository,
    IServicoDaOrdemDeServicoRepository servicoDaOrdemDeServicoRepository,
    IPecaOuInsumoDaOrdemDeServicoRepository pecaOuInsumoDaOrdemDeServicoRepository) : IAcompanhamentoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository = veiculoRepository;
    private readonly IServicoDaOrdemDeServicoRepository _servicoDaOrdemDeServicoRepository = servicoDaOrdemDeServicoRepository;
    private readonly IPecaOuInsumoDaOrdemDeServicoRepository _pecaOuInsumoDaOrdemDeServicoRepository = pecaOuInsumoDaOrdemDeServicoRepository;

    public async Task<Result<AcompanhamentoOrdemServicoResponse>> ExecuteAsync(AcompanhamentoOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrdemServicoId == Guid.Empty)
            return Result<AcompanhamentoOrdemServicoResponse>.Failure(new Error("O identificador da ordem de servico deve ser valido."));

        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<AcompanhamentoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var clienteResult = await _clienteRepository.GetByIdAsync(ordemServico.ClienteId, cancellationToken);
        var veiculoResult = await _veiculoRepository.GetByIdAsync(ordemServico.VeiculoId, cancellationToken);
        var servicosDaOrdemResult = await _servicoDaOrdemDeServicoRepository.GetByOrdemServicoIdAsync(command.OrdemServicoId, cancellationToken);
        var pecasInsumosDaOrdemResult = await _pecaOuInsumoDaOrdemDeServicoRepository.GetByOrdemServicoIdAsync(command.OrdemServicoId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<AcompanhamentoOrdemServicoResponse>.Failure(clienteResult.Error);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<AcompanhamentoOrdemServicoResponse>.Failure(veiculoResult.Error);

        if (!servicosDaOrdemResult.IsSuccess || servicosDaOrdemResult.Value is null)
            return Result<AcompanhamentoOrdemServicoResponse>.Failure(servicosDaOrdemResult.Error);

        if (!pecasInsumosDaOrdemResult.IsSuccess || pecasInsumosDaOrdemResult.Value is null)
            return Result<AcompanhamentoOrdemServicoResponse>.Failure(pecasInsumosDaOrdemResult.Error);

        var servicosDaOrdem = servicosDaOrdemResult.Value;
        var pecasInsumosDaOrdem = pecasInsumosDaOrdemResult.Value;
        var valorTotalServicos = servicosDaOrdem.Sum(x => x.ValorTotal);
        var valorTotalPecasInsumos = pecasInsumosDaOrdem.Sum(x => x.ValorTotal);
        var veiculo = veiculoResult.Value;

        return Result<AcompanhamentoOrdemServicoResponse>.Success(new AcompanhamentoOrdemServicoResponse
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            ClienteNome = clienteResult.Value.Nome,
            VeiculoId = ordemServico.VeiculoId,
            VeiculoMarca = veiculo.Marca,
            VeiculoModelo = veiculo.Modelo,
            VeiculoPlaca = veiculo.Placa.Value,
            VeiculoAno = veiculo.Ano,
            DescricaoProblema = ordemServico.DescricaoProblema,
            Status = ordemServico.Status,
            DataCriacao = ordemServico.DataCriacao,
            DataInicioDiagnostico = ordemServico.DataInicioDiagnostico,
            DataEnvioAprovacao = ordemServico.DataEnvioAprovacao,
            DataInicioExecucao = ordemServico.DataInicioExecucao,
            DataFinalizacao = ordemServico.DataFinalizacao,
            DataEntrega = ordemServico.DataEntrega,
            Servicos = servicosDaOrdem.Select(ToServicoItemResponse).ToArray(),
            PecasInsumos = pecasInsumosDaOrdem.Select(ToPecaInsumoItemResponse).ToArray(),
            ValorTotalServicos = valorTotalServicos,
            ValorTotalPecasInsumos = valorTotalPecasInsumos,
            ValorTotalOrdemServico = valorTotalServicos + valorTotalPecasInsumos
        });
    }

    private static AcompanhamentoServicoItemResponse ToServicoItemResponse(ServicoDaOrdemDeServico servicoDaOrdemDeServico)
    {
        return new AcompanhamentoServicoItemResponse
        {
            Id = servicoDaOrdemDeServico.Id,
            Descricao = servicoDaOrdemDeServico.Descricao,
            ValorUnitario = servicoDaOrdemDeServico.ValorUnitario,
            Quantidade = servicoDaOrdemDeServico.Quantidade,
            ValorTotal = servicoDaOrdemDeServico.ValorTotal,
            TempoGastoMinutos = servicoDaOrdemDeServico.TempoGastoMinutos,
            Concluido = servicoDaOrdemDeServico.Concluido
        };
    }

    private static AcompanhamentoPecaInsumoItemResponse ToPecaInsumoItemResponse(PecaOuInsumoDaOrdemDeServico pecaOuInsumoDaOrdemDeServico)
    {
        return new AcompanhamentoPecaInsumoItemResponse
        {
            Id = pecaOuInsumoDaOrdemDeServico.Id,
            Nome = pecaOuInsumoDaOrdemDeServico.Nome,
            Codigo = pecaOuInsumoDaOrdemDeServico.Codigo,
            Descricao = pecaOuInsumoDaOrdemDeServico.Descricao,
            PrecoUnitario = pecaOuInsumoDaOrdemDeServico.PrecoUnitario,
            Quantidade = pecaOuInsumoDaOrdemDeServico.Quantidade,
            ValorTotal = pecaOuInsumoDaOrdemDeServico.ValorTotal
        };
    }
}

