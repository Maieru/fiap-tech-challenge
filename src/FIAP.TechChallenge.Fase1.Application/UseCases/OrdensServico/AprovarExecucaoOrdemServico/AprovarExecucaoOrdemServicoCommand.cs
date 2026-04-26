namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;

public sealed class AprovarExecucaoOrdemServicoCommand
{
    [Description("Identificador da ordem de servico a aprovar para execucao.")]
    public Guid OrdemServicoId { get; init; }
}
