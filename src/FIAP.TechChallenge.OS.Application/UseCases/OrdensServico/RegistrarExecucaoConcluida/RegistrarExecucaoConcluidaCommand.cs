namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoConcluida;

public sealed record RegistrarExecucaoConcluidaCommand(Guid OrdemServicoId, DateTimeOffset OcorridoEmUtc);
