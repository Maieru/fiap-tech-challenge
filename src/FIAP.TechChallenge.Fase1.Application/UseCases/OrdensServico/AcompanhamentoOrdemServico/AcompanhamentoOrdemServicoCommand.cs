namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AcompanhamentoOrdemServico;

public sealed class AcompanhamentoOrdemServicoCommand
{
    [Description("Identificador da ordem de servico para consulta publica de acompanhamento.")]
    public Guid OrdemServicoId { get; init; }
}
