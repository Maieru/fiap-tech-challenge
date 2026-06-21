using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConsultarStatusOrdemServico;

public sealed class ConsultarStatusOrdemServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IConsultarStatusOrdemServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;

    public async Task<Result<ConsultarStatusOrdemServicoResponse>> ExecuteAsync(ConsultarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrdemServicoId == Guid.Empty)
            return Result<ConsultarStatusOrdemServicoResponse>.Failure(new Error("O identificador da ordem de servico deve ser valido."));

        var ordemServicoResult = await _ordemServicoRepository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
            return Result<ConsultarStatusOrdemServicoResponse>.Failure(ordemServicoResult.Error);

        return Result<ConsultarStatusOrdemServicoResponse>.Success(new ConsultarStatusOrdemServicoResponse
        {
            Id = ordemServicoResult.Value.Id,
            Status = ordemServicoResult.Value.Status
        });
    }
}

