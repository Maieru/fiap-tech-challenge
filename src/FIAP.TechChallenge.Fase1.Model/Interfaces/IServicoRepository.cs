using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IServicoRepository
{
    Task AddAsync(Servico servico, CancellationToken cancellationToken = default);
}
