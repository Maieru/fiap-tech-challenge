namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.VerificarTempoMedioServico;

public sealed class VerificarTempoMedioServicoCommand
{
    [Description("Identificador do servico para calculo do tempo medio.")]
    public Guid ServicoId { get; init; }
}
