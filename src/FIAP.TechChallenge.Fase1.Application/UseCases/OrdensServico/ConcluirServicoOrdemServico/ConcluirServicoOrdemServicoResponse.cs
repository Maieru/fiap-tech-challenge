namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;

public sealed class ConcluirServicoOrdemServicoResponse
{
    public Guid Id { get; init; }
    public Guid OrdemServicoId { get; init; }
    public Guid ServicoId { get; init; }
    public int TempoGastoMinutos { get; init; }
    public bool Concluido { get; init; }
}

