namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarDiagnosticoConcluido;

public sealed record RegistrarDiagnosticoConcluidoCommand(Guid OrdemServicoId, DateTimeOffset OcorridoEmUtc, Guid? OrcamentoId = null, decimal? Valor = null, FIAP.TechChallenge.OS.Domain.Entities.OrcamentoOrdem? Orcamento = null);
