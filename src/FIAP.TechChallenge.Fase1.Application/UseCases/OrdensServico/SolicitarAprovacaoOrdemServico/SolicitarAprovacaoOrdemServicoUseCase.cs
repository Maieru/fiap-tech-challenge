using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Observability;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;

public sealed class SolicitarAprovacaoOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IClienteRepository clienteRepository,
    IMailService mailService) : ISolicitarAprovacaoOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IMailService _mailService = mailService;

    public async Task<Result<SolicitarAprovacaoOrdemServicoResponse>> ExecuteAsync(SolicitarAprovacaoOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<SolicitarAprovacaoOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var aguardarAprovacaoResult = ordemServico.AguardarAprovacao();

        if (!aguardarAprovacaoResult.IsSuccess)
            return Result<SolicitarAprovacaoOrdemServicoResponse>.Failure(aguardarAprovacaoResult.Error);

        var clienteResult = await _clienteRepository.GetByIdAsync(ordemServico.ClienteId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<SolicitarAprovacaoOrdemServicoResponse>.Failure(clienteResult.Error);

        var cliente = clienteResult.Value;

        if (cliente.Email is not null)
        {
            var sendMailResult = await _mailService.SendMail(
                cliente.Email.Value,
                "Aprovacao de ordem de servico",
                $"Sua ordem de servico {ordemServico.Id} esta aguardando aprovacao. Codigo de aprovacao: {ordemServico.CodigoAprovacao}.");

            if (!sendMailResult.IsSuccess || !sendMailResult.Value)
                return Result<SolicitarAprovacaoOrdemServicoResponse>.Failure(sendMailResult.Error);
        }

        await _ordemServicoRepository.UpdateAsync(ordemServico, cancellationToken);
        MetricasNegocio.RegistrarEtapaConcluida(StatusOrdemServico.EmDiagnostico, ordemServico.DataInicioDiagnostico!.Value, ordemServico.DataEnvioAprovacao!.Value);

        return Result<SolicitarAprovacaoOrdemServicoResponse>.Success(new SolicitarAprovacaoOrdemServicoResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            DataEnvioAprovacao = ordemServico.DataEnvioAprovacao!.Value
        });
    }
}

