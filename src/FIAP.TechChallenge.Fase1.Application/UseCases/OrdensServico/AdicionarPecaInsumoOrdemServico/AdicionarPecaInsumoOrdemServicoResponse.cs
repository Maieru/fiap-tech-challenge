namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;

public sealed class AdicionarPecaInsumoOrdemServicoResponse
{
    public Guid Id { get; init; }
    public Guid OrdemServicoId { get; init; }
    public Guid PecaInsumoId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public decimal PrecoUnitario { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorTotal { get; init; }
}

