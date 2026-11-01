namespace FIAP.TechChallenge.OS.Domain.Entities;

public sealed record HistoricoOrdem(Guid Id, Guid OrdemServicoId, string Tipo, DateTime OcorridoEmUtc, string? CorrelationId);
