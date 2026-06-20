using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;

public sealed class CriarOrdemServicoComClienteEVeiculoUseCase(ICriarClienteUseCase criarClienteUseCase, ICriarVeiculoUseCase criarVeiculoUseCase, ICriarOrdemServicoUseCase criarOrdemServicoUseCase) : ICriarOrdemServicoComClienteEVeiculoUseCase
{
    private readonly ICriarClienteUseCase _criarClienteUseCase = criarClienteUseCase;
    private readonly ICriarVeiculoUseCase _criarVeiculoUseCase = criarVeiculoUseCase;
    private readonly ICriarOrdemServicoUseCase _criarOrdemServicoUseCase = criarOrdemServicoUseCase;

    public async Task<Result<CriarOrdemServicoResponse>> ExecuteAsync(CriarOrdemServicoComClienteEVeiculoCommand command, CancellationToken cancellationToken = default)
    {
        var clienteResult = await _criarClienteUseCase.ExecuteAsync(command.Cliente, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<CriarOrdemServicoResponse>.Failure(clienteResult.Error);

        var veiculoResult = await _criarVeiculoUseCase.ExecuteAsync(new CriarVeiculoCommand
        {
            ClienteId = clienteResult.Value.Id,
            Placa = command.Veiculo.Placa,
            Marca = command.Veiculo.Marca,
            Modelo = command.Veiculo.Modelo,
            Ano = command.Veiculo.Ano
        }, cancellationToken);

        if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
            return Result<CriarOrdemServicoResponse>.Failure(veiculoResult.Error);

        return await _criarOrdemServicoUseCase.ExecuteAsync(new CriarOrdemServicoCommand
        {
            ClienteId = clienteResult.Value.Id,
            VeiculoId = veiculoResult.Value.Id,
            DescricaoProblema = command.DescricaoProblema
        }, cancellationToken);
    }
}
