namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;

public sealed class AdicionarPecaInsumoOrdemServicoCommand
{
    public Guid OrdemServicoId { get; init; }
    public Guid PecaInsumoId { get; init; }
    public int Quantidade { get; init; }
}

