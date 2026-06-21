using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class PecaInsumoMapper
{
    public static PecaInsumoEntity ToEntity(PecaInsumo pecaInsumo)
    {
        return new PecaInsumoEntity
        {
            Id = pecaInsumo.Id,
            Nome = pecaInsumo.Nome,
            Codigo = pecaInsumo.Codigo,
            Descricao = pecaInsumo.Descricao,
            PrecoUnitario = pecaInsumo.PrecoUnitario,
            QuantidadeEstoque = pecaInsumo.QuantidadeEstoque,
            Ativo = pecaInsumo.Ativo
        };
    }

    public static Result<PecaInsumo> ToDomain(PecaInsumoEntity entity)
    {
        return PecaInsumo.Rehydrate(
            entity.Id,
            entity.Nome,
            entity.Codigo,
            entity.Descricao,
            entity.PrecoUnitario,
            entity.QuantidadeEstoque,
            entity.Ativo);
    }
}

