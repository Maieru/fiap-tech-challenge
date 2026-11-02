using FIAP.TechChallenge.OS.Domain.Enums;
using FIAP.TechChallenge.OS.Domain.Entities;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico;

public sealed record DecisaoClienteResponse(Guid OrdemServicoId, StatusOrdemServico Status,
    DecisaoExecucao Decisao, bool Pendente, EtapaPagamento? EtapaPagamento)
{
    public static DecisaoClienteResponse From(DecisaoExecucaoResponse response) =>
        new(response.OrdemServicoId, response.Status, response.Decisao, response.Pendente, response.Pagamento?.Etapa);
}
