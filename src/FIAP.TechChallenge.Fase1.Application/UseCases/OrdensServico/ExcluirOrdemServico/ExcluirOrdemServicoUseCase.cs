using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ExcluirOrdemServico;

public sealed class ExcluirOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IExcluirOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;

    public async Task<Result<ExcluirOrdemServicoResponse>> ExecuteAsync(ExcluirOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.Id, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<ExcluirOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        await _ordemServicoRepository.DeleteAsync(ordemServicoResult.Value, cancellationToken);

        return Result<ExcluirOrdemServicoResponse>.Success(new ExcluirOrdemServicoResponse { Id = command.Id });
    }
}
