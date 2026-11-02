using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarAcompanhamento;
public sealed record AcompanhamentoResponse(Guid OrdemServicoId, string DescricaoProblema, StatusOrdemServico Status,
    DateTime DataCriacao, DateTime? DataInicioDiagnostico, DateTime? DataEnvioAprovacao, DateTime? DataInicioExecucao,
    DateTime? DataFinalizacao, DateTime? DataEntrega, DateTime? DataCancelamento, decimal? ValorOrcamento,
    OrcamentoOrdem? Orcamento, EtapaPagamento? EtapaPagamento)
{
    public static AcompanhamentoResponse From(OrdemServico ordem) => new(ordem.Id, ordem.DescricaoProblema, ordem.Status,
        ordem.DataCriacao, ordem.DataInicioDiagnostico, ordem.DataEnvioAprovacao, ordem.DataInicioExecucao,
        ordem.DataFinalizacao, ordem.DataEntrega, ordem.DataCancelamento, ordem.ValorOrcamento, ordem.Orcamento, ordem.Pagamento?.Etapa);
}
