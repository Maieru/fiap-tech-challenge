using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.FinalizarOrdemServico;

public sealed class FinalizarOrdemServicoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IServicoDaOrdemDeServicoRepository servicoDaOrdemDeServicoRepository,
    IClienteRepository clienteRepository,
    IMailService mailService) : IFinalizarOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IServicoDaOrdemDeServicoRepository _servicoDaOrdemDeServicoRepository = servicoDaOrdemDeServicoRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IMailService _mailService = mailService;

    public async Task<Result<FinalizarOrdemServicoResponse>> ExecuteAsync(FinalizarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<FinalizarOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;
        var servicosDaOrdemResult = await _servicoDaOrdemDeServicoRepository.GetByOrdemServicoIdAsync(ordemServico.Id, cancellationToken);

        if (!servicosDaOrdemResult.IsSuccess || servicosDaOrdemResult.Value is null)
            return Result<FinalizarOrdemServicoResponse>.Failure(servicosDaOrdemResult.Error);

        var finalizarResult = ordemServico.Finalizar(servicosDaOrdemResult.Value);

        if (!finalizarResult.IsSuccess)
            return Result<FinalizarOrdemServicoResponse>.Failure(finalizarResult.Error);

        var clienteResult = await _clienteRepository.GetByIdAsync(ordemServico.ClienteId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<FinalizarOrdemServicoResponse>.Failure(clienteResult.Error);

        var cliente = clienteResult.Value;

        if (cliente.Email is not null)
        {
            var sendMailResult = await _mailService.SendMail(
                cliente.Email.Value,
                "Ordem de servico finalizada",
                $"Sua ordem de servico {ordemServico.Id} foi finalizada e esta pronta para retirada.");

            if (!sendMailResult.IsSuccess || !sendMailResult.Value)
                return Result<FinalizarOrdemServicoResponse>.Failure(sendMailResult.Error);
        }

        await _ordemServicoRepository.UpdateAsync(ordemServico, cancellationToken);

        return Result<FinalizarOrdemServicoResponse>.Success(new FinalizarOrdemServicoResponse
        {
            Id = ordemServico.Id,
            Status = ordemServico.Status,
            DataFinalizacao = ordemServico.DataFinalizacao!.Value
        });
    }
}

