namespace FIAP.TechChallenge.OS.Application.Messaging;
public sealed record PagamentoSolicitado(Guid OrdemServicoId, Guid ClienteId, Guid OrcamentoId, decimal Valor, string Moeda) { public const string Type = "os.pagamento-solicitado"; }
