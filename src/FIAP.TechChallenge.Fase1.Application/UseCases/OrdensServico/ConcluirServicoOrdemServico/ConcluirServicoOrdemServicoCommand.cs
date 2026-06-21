namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;

public sealed class ConcluirServicoOrdemServicoCommand
{
    public Guid ServicoDaOrdemDeServicoId { get; init; }
    public int TempoGastoMinutos { get; init; }
}

