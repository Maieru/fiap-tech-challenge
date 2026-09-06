using FIAP.TechChallenge.Fase1.Domain.Observability;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;

public sealed class CriarOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository, IClienteRepository clienteRepository, IVeiculoRepository veiculoRepository) : ICriarOrdemServicoUseCase
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

        await _ordemServicoRepository.AddAsync(ordemServico, cancellationToken);
        MetricasNegocio.RegistrarOrdemCriada();

        return Result<CriarOrdemServicoResponse>.Success(new CriarOrdemServicoResponse
        {
            Id = ordemServico.Id,
            Token = clienteResult.Value.Cpf is null
                ? null
                : CpfAccessToken.Create(clienteResult.Value.Cpf, ordemServico.CodigoAprovacao),
            ClienteId = ordemServico.ClienteId,
            VeiculoId = ordemServico.VeiculoId,
            DescricaoProblema = ordemServico.DescricaoProblema,
            Status = ordemServico.Status,
            DataCriacao = ordemServico.DataCriacao
        });
    }
}

