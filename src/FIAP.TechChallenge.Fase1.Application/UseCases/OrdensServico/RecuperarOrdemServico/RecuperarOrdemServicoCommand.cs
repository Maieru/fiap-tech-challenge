namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;

public sealed class RecuperarOrdemServicoCommand
{
    [Description("Identificador da ordem de servico.")]
    public Guid OrdemServicoId { get; init; }
}
