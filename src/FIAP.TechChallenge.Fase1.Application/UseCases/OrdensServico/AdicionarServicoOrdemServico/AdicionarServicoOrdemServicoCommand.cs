namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;

public sealed class AdicionarServicoOrdemServicoCommand
{
    public Guid OrdemServicoId { get; init; }
    public Guid ServicoId { get; init; }
    public int Quantidade { get; init; }
}

