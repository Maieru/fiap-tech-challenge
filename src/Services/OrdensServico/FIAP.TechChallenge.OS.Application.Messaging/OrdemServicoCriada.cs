namespace FIAP.TechChallenge.OS.Application.Messaging;

public sealed record OrdemServicoCriada(Guid OrdemServicoId, Guid ClienteId, Guid VeiculoId, string DescricaoProblema)
{
    public const string Type = "os.ordem-servico-criada";
}
