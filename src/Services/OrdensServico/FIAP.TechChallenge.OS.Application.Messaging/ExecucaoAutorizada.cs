namespace FIAP.TechChallenge.OS.Application.Messaging;

public sealed record ExecucaoAutorizada(Guid OrdemServicoId)
{
    public const string Type = "os.execucao-autorizada";
}
