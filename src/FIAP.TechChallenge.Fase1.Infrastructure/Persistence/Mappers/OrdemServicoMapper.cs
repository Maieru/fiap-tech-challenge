using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class OrdemServicoMapper
{
    public static OrdemServicoEntity ToEntity(OrdemServico ordemServico)
    {
        return new OrdemServicoEntity
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            VeiculoId = ordemServico.VeiculoId,
            DescricaoProblema = ordemServico.DescricaoProblema,
            Status = ordemServico.Status,
            DataCriacao = ordemServico.DataCriacao,
            DataInicioDiagnostico = ordemServico.DataInicioDiagnostico,
            DataEnvioAprovacao = ordemServico.DataEnvioAprovacao,
            DataInicioExecucao = ordemServico.DataInicioExecucao,
            DataFinalizacao = ordemServico.DataFinalizacao,
            DataEntrega = ordemServico.DataEntrega
        };
    }

    public static Result<OrdemServico> ToDomain(OrdemServicoEntity entity)
    {
        var snapshot = new OrdemServicoSnapshot(
            entity.Id,
            entity.ClienteId,
            entity.VeiculoId,
            entity.DescricaoProblema,
            entity.Status,
            entity.DataCriacao,
            entity.DataInicioDiagnostico,
            entity.DataEnvioAprovacao,
            entity.DataInicioExecucao,
            entity.DataFinalizacao,
            entity.DataEntrega);

        return OrdemServico.Rehydrate(snapshot);
    }
}
