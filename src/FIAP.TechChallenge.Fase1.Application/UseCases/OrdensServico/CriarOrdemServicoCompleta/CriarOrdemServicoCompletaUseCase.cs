using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using System.Transactions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoCompleta;

public sealed class CriarOrdemServicoCompletaUseCase(
    ICriarOrdemServicoComClienteEVeiculoUseCase criarOrdemServico,
    IAdicionarServicoOrdemServicoUseCase adicionarServico,
    IAdicionarPecaInsumoOrdemServicoUseCase adicionarPecaInsumo,
    IIniciarDiagnosticoOrdemServicoUseCase iniciarDiagnostico) : ICriarOrdemServicoCompletaUseCase
{
    public async Task<Result<CriarOrdemServicoCompletaResponse>> ExecuteAsync(CriarOrdemServicoCompletaCommand command, CancellationToken cancellationToken = default)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var ordemResult = await criarOrdemServico.ExecuteAsync(new CriarOrdemServicoComClienteEVeiculoCommand
        {
            Cliente = command.Cliente,
            Veiculo = command.Veiculo,
            DescricaoProblema = command.DescricaoProblema
        }, cancellationToken);

        if (!ordemResult.IsSuccess || ordemResult.Value is null)
            return Result<CriarOrdemServicoCompletaResponse>.Failure(ordemResult.Error);

        var diagnosticoResult = await iniciarDiagnostico.ExecuteAsync(
            new IniciarDiagnosticoOrdemServicoCommand { OrdemServicoId = ordemResult.Value.Id }, cancellationToken);
        if (!diagnosticoResult.IsSuccess || diagnosticoResult.Value is null)
            return Result<CriarOrdemServicoCompletaResponse>.Failure(diagnosticoResult.Error);

        var servicos = new List<AdicionarServicoOrdemServicoResponse>();
        foreach (var item in command.Servicos)
        {
            var result = await adicionarServico.ExecuteAsync(new AdicionarServicoOrdemServicoCommand
            {
                OrdemServicoId = ordemResult.Value.Id,
                ServicoId = item.ServicoId,
                Quantidade = item.Quantidade
            }, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
                return Result<CriarOrdemServicoCompletaResponse>.Failure(result.Error);
            servicos.Add(result.Value);
        }

        var pecas = new List<AdicionarPecaInsumoOrdemServicoResponse>();
        foreach (var item in command.PecasInsumos)
        {
            var result = await adicionarPecaInsumo.ExecuteAsync(new AdicionarPecaInsumoOrdemServicoCommand
            {
                OrdemServicoId = ordemResult.Value.Id,
                PecaInsumoId = item.PecaInsumoId,
                Quantidade = item.Quantidade
            }, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
                return Result<CriarOrdemServicoCompletaResponse>.Failure(result.Error);
            pecas.Add(result.Value);
        }

        transaction.Complete();
        return Result<CriarOrdemServicoCompletaResponse>.Success(new CriarOrdemServicoCompletaResponse
        {
            Id = ordemResult.Value.Id,
            Token = ordemResult.Value.Token,
            ClienteId = ordemResult.Value.ClienteId,
            VeiculoId = ordemResult.Value.VeiculoId,
            DescricaoProblema = ordemResult.Value.DescricaoProblema,
            Status = diagnosticoResult.Value.Status,
            DataCriacao = ordemResult.Value.DataCriacao,
            DataInicioDiagnostico = diagnosticoResult.Value.DataInicioDiagnostico,
            Servicos = servicos,
            PecasInsumos = pecas
        });
    }
}
