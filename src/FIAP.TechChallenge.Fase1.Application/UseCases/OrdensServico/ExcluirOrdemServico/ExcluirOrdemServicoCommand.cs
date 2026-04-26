namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ExcluirOrdemServico;

public sealed class ExcluirOrdemServicoCommand
{
    [Description("Identificador da ordem de servico a excluir.")]
    public Guid Id { get; set; }
}
