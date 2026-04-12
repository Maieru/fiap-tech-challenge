using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IServicoDaOrdemDeServicoRepository
{
    Task AddAsync(ServicoDaOrdemDeServico servicoDaOrdemDeServico, CancellationToken cancellationToken = default);
}
