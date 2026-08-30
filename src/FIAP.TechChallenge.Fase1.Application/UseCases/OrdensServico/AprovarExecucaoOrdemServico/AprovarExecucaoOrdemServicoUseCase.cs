using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;

public sealed class AprovarExecucaoOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository, IClienteRepository clienteRepository) : IAprovarExecucaoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<Result<AprovarExecucaoOrdemServicoResponse>> ExecuteAsync(AprovarExecucaoOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<AprovarExecucaoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var clienteResult = await _clienteRepository.GetByIdAsync(ordemServico.ClienteId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value?.Cpf is null ||
            !CpfAccessToken.Matches(clienteResult.Value.Cpf, ordemServico.CodigoAprovacao, command.Token))
        {
            return Result<AprovarExecucaoOrdemServicoResponse>.Failure(new Error("O token de acesso informado e invalido."));
        }

        var aprovarOrcamentoResult = ordemServico.AprovarOrcamento(ordemServico.CodigoAprovacao);

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

