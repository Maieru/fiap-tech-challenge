namespace FIAP.TechChallenge.OS.Domain.Entities;

public enum EtapaPagamento { AguardandoPagamento, AutorizacaoSolicitada, ExecucaoIniciada, Concluida, CompensacaoSolicitada, Compensada }
public sealed record PagamentoOrdem(Guid OrcamentoId, decimal Valor, string Moeda, EtapaPagamento Etapa, Guid? CobrancaId = null, string? GatewayPaymentId = null, string? Motivo = null);
