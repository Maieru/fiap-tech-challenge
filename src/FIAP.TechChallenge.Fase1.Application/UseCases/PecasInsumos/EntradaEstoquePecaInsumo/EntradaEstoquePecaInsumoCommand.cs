namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.EntradaEstoquePecaInsumo;

public sealed class EntradaEstoquePecaInsumoCommand
{
    public Guid Id { get; init; }
    public int Quantidade { get; init; }
}
