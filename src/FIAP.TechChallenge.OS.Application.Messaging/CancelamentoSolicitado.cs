namespace FIAP.TechChallenge.OS.Application.Messaging;

public sealed record CancelamentoSolicitado(Guid OrdemServicoId)
{
    public const string Type = "os.cancelamento-solicitado";
}
