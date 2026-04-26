namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.FinalizarOrdemServico;

public sealed class FinalizarOrdemServicoCommand
{
    [Description("Identificador da ordem de servico a finalizar.")]
    public Guid OrdemServicoId { get; init; }
}
