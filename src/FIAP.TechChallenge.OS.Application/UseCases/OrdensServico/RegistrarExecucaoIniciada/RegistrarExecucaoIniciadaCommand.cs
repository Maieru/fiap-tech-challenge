namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoIniciada;

public sealed record RegistrarExecucaoIniciadaCommand(Guid OrdemServicoId, DateTimeOffset OcorridoEmUtc);
