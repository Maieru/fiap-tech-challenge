namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarAcompanhamento;
public sealed record ConsultarAcompanhamentoCommand(Guid OrdemServicoId, string Token);
