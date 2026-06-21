using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;

public interface ICriarOrdemServicoComClienteEVeiculoUseCase
{
    Task<Result<CriarOrdemServicoResponse>> ExecuteAsync(CriarOrdemServicoComClienteEVeiculoCommand command, CancellationToken cancellationToken = default);
}

