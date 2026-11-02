using FIAP.TechChallenge.OS.Domain.Abstractions;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.EntregarOrdemServico;
public interface IEntregarOrdemServicoUseCase { Task<Result<EntregarOrdemServicoResponse>> ExecuteAsync(EntregarOrdemServicoCommand command, CancellationToken cancellationToken = default); }
