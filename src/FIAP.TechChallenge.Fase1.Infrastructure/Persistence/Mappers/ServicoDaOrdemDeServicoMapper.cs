using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class ServicoDaOrdemDeServicoMapper
{
    public static ServicoDaOrdemDeServicoEntity ToEntity(ServicoDaOrdemDeServico domain)
    {
        return new ServicoDaOrdemDeServicoEntity
        {
            Id = domain.Id,
            OrdemServicoId = domain.OrdemServicoId,
            ServicoId = domain.ServicoId,
            Descricao = domain.Descricao,
            ValorUnitario = domain.ValorUnitario,
            Quantidade = domain.Quantidade,
            TempoGastoMinutos = domain.TempoGastoMinutos,
            Concluido = domain.Concluido
        };
    }

    public static Result<ServicoDaOrdemDeServico> ToDomain(ServicoDaOrdemDeServicoEntity entity)
    {
        return ServicoDaOrdemDeServico.Rehydrate(
            entity.Id,
            entity.OrdemServicoId,
            entity.ServicoId,
            entity.Descricao,
            entity.ValorUnitario,
            entity.Quantidade,
            entity.TempoGastoMinutos,
            entity.Concluido);
    }
}
