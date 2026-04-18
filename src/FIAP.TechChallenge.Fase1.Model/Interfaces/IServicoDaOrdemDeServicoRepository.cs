using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IServicoDaOrdemDeServicoRepository
{
    Task<Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>> GetByOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
    Task AddAsync(ServicoDaOrdemDeServico servicoDaOrdemDeServico, CancellationToken cancellationToken = default);
}
