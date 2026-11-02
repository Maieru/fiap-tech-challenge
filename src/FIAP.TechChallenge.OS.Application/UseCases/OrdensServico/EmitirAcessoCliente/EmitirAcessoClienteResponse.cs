namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.EmitirAcessoCliente;
public sealed record EmitirAcessoClienteResponse(Guid OrdemServicoId, string Token, string AcompanhamentoUrl);
