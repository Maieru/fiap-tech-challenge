namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoCancelada;

public sealed record RegistrarExecucaoCanceladaCommand(Guid OrdemServicoId, DateTimeOffset OcorridoEmUtc);
