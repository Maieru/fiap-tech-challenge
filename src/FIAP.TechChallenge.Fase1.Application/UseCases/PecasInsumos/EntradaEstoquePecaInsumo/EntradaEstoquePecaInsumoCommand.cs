namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.EntradaEstoquePecaInsumo;

public sealed class EntradaEstoquePecaInsumoCommand
{
    [Description("Identificador da peca ou insumo. Em chamadas pela API, este valor e preenchido pela rota.")]
    public Guid Id { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Quantidade a acrescentar no estoque.")]
    public int Quantidade { get; init; }
}
