namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;

public sealed class SolicitarAprovacaoOrdemServicoCommand
{
    [Description("Identificador da ordem de servico a enviar para aprovacao.")]
    public Guid OrdemServicoId { get; init; }
}
