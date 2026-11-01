using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico;

public sealed record DecisaoExecucaoResponse(Guid OrdemServicoId, StatusOrdemServico Status, DecisaoExecucao Decisao, bool Pendente)
{
    public static DecisaoExecucaoResponse From(OrdemServico ordem) =>
        new(ordem.Id, ordem.Status, ordem.Decisao, ordem.Status == StatusOrdemServico.AguardandoAprovacao);
}
