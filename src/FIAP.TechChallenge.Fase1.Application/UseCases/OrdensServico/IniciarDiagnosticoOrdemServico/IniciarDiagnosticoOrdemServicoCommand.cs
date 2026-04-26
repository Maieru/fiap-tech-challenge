namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;

public sealed class IniciarDiagnosticoOrdemServicoCommand
{
    [Description("Identificador da ordem de servico para iniciar diagnostico.")]
    public Guid OrdemServicoId { get; init; }
}
