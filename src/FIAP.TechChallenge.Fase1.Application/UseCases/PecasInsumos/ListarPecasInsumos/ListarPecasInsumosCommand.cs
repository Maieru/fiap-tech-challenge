namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.ListarPecasInsumos;

public sealed class ListarPecasInsumosCommand
{
    [StringLength(50, MinimumLength = 2)]
    [Description("Filtra por codigo da peca ou insumo.")]
    public string? Codigo { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Numero da pagina.")]
    public int PageNumber { get; init; } = 1;

    [Range(1, int.MaxValue)]
    [Description("Quantidade de itens por pagina.")]
    public int PageSize { get; init; } = 10;
}
