using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Domain.Entities;

public sealed record OrdemServicoSnapshot(
    Guid Id, Guid ClienteId, Guid VeiculoId, string DescricaoProblema, DateTime DataCriacao,
    StatusOrdemServico Status, DecisaoExecucao Decisao, DateTime? DataDecisaoUtc,
    DateTime? DataInicioDiagnostico, DateTime? DataEnvioAprovacao, DateTime? DataInicioExecucao,
    DateTime? DataFinalizacao, DateTime? DataCancelamento, long Version);
