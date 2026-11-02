using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using Maieru.FiapTechChallenge.Chassis;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.CriarOrdemServico;

public sealed class CriarOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository, IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository, IUnitOfWork unitOfWork, IMessagingService messaging, MessageContext context) : ICriarOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository = veiculoRepository;

    public async Task<Result<CriarOrdemServicoResponse>> ExecuteAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var clienteResult = await _clienteRepository.GetByIdAsync(command.ClienteId, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<CriarOrdemServicoResponse>.Failure(clienteResult.Error);

        var veiculoResult = await _veiculoRepository.GetByIdAsync(command.VeiculoId, cancellationToken);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<CriarOrdemServicoResponse>.Failure(veiculoResult.Error);

        if (veiculoResult.Value.ClienteId != command.ClienteId)
            return Result<CriarOrdemServicoResponse>.Failure(new Error("O veiculo informado nao pertence ao cliente informado."));

        var ordemServicoResult = OrdemServico.Create(command.ClienteId, command.VeiculoId, command.DescricaoProblema);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<CriarOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value;

        await using var transaction = await unitOfWork.BeginAsync(cancellationToken);
        await _ordemServicoRepository.AddAsync(ordemServico, cancellationToken);
        await messaging.PublishAsync(OSMessaging.Topic, ordemServico.Id.ToString("D"), context.Create(
            OrdemServicoCriada.Type, new OrdemServicoCriada(ordemServico.Id, ordemServico.ClienteId,
                ordemServico.VeiculoId, ordemServico.DescricaoProblema), new DateTimeOffset(ordemServico.DataCriacao)), cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<CriarOrdemServicoResponse>.Success(new CriarOrdemServicoResponse
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            VeiculoId = ordemServico.VeiculoId,
            DescricaoProblema = ordemServico.DescricaoProblema,
            Status = ordemServico.Status,
            DataCriacao = ordemServico.DataCriacao
        });
    }
}
