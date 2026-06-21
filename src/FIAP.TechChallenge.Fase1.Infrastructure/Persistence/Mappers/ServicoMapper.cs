using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class ServicoMapper
{
    public static ServicoEntity ToEntity(Servico servico)
    {
        return new ServicoEntity
        {
            Id = servico.Id,
            Descricao = servico.Descricao,
            ValorUnitario = servico.ValorUnitario,
        };
    }

    public static Result<Servico> ToDomain(ServicoEntity entity)
    {
        return Servico.Rehydrate(entity.Id, entity.Descricao, entity.ValorUnitario);
    }
}

