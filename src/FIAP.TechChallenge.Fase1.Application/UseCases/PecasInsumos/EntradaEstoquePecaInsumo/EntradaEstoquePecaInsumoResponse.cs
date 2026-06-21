namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.EntradaEstoquePecaInsumo;

public sealed class EntradaEstoquePecaInsumoResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public int QuantidadeEntrada { get; init; }
    public int QuantidadeEstoque { get; init; }
}

