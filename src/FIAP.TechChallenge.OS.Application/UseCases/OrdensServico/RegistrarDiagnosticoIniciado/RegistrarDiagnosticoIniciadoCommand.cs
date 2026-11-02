namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarDiagnosticoIniciado;

public sealed record RegistrarDiagnosticoIniciadoCommand(Guid OrdemServicoId, DateTimeOffset OcorridoEmUtc);
