using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IServicoDaOrdemDeServicoRepository
{
    Task<Result<ServicoDaOrdemDeServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>> GetByOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>> GetConcluidosByServicoIdAsync(Guid servicoId, CancellationToken cancellationToken = default);
    Task AddAsync(ServicoDaOrdemDeServico servicoDaOrdemDeServico, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServicoDaOrdemDeServico servicoDaOrdemDeServico, CancellationToken cancellationToken = default);
}
