using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IPecaOuInsumoDaOrdemDeServicoRepository
{
    Task AddAsync(PecaOuInsumoDaOrdemDeServico pecaOuInsumoDaOrdemDeServico, CancellationToken cancellationToken = default);
}
