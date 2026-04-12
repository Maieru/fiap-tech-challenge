using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class PecaOuInsumoDaOrdemDeServicoMapper
{
    public static PecaOuInsumoDaOrdemDeServicoEntity ToEntity(PecaOuInsumoDaOrdemDeServico domain)
    {
        return new PecaOuInsumoDaOrdemDeServicoEntity
        {
            Id = domain.Id,
            OrdemServicoId = domain.OrdemServicoId,
            PecaInsumoId = domain.PecaInsumoId,
            Nome = domain.Nome,
            Codigo = domain.Codigo,
            Descricao = domain.Descricao,
            PrecoUnitario = domain.PrecoUnitario,
            Quantidade = domain.Quantidade
        };
    }

    public static Result<PecaOuInsumoDaOrdemDeServico> ToDomain(PecaOuInsumoDaOrdemDeServicoEntity entity)
    {
        var snapshot = new PecaOuInsumoDaOrdemDeServicoSnapshot(
            entity.Id,
            entity.OrdemServicoId,
            entity.PecaInsumoId,
            entity.Nome,
            entity.Codigo,
            entity.Descricao,
            entity.PrecoUnitario,
            entity.Quantidade);

        return PecaOuInsumoDaOrdemDeServico.Rehydrate(snapshot);
    }
}