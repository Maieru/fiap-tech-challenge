namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.EntregarOrdemServico;

public sealed class EntregarOrdemServicoCommand
{
    [Description("Identificador da ordem de servico a entregar.")]
    public Guid OrdemServicoId { get; init; }
}
