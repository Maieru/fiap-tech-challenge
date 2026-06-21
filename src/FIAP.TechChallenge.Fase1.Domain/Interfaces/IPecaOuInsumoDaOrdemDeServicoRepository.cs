using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IPecaOuInsumoDaOrdemDeServicoRepository
{
    Task<Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>> GetByOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
    Task AddAsync(PecaOuInsumoDaOrdemDeServico pecaOuInsumoDaOrdemDeServico, CancellationToken cancellationToken = default);
}

