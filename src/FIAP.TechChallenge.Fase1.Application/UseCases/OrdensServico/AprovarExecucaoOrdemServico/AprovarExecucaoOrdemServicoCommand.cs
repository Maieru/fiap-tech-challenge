namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;

public sealed class AprovarExecucaoOrdemServicoCommand
{
    public Guid OrdemServicoId { get; init; }
    public string Token { get; init; } = string.Empty;
}
