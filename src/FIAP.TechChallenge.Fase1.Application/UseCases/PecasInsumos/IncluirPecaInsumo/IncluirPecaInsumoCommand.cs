namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.IncluirPecaInsumo;

public sealed class IncluirPecaInsumoCommand
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    [Description("Nome da peca ou insumo.")]
    public string Nome { get; init; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    [Description("Codigo unico da peca ou insumo.")]
    public string Codigo { get; init; } = string.Empty;

    [StringLength(500)]
    [Description("Descricao complementar da peca ou insumo.")]
    public string? Descricao { get; init; }

    [Range(0, double.MaxValue)]
    [Description("Preco unitario da peca ou insumo.")]
    public decimal PrecoUnitario { get; init; }

    [Range(0, int.MaxValue)]
    [Description("Quantidade inicial disponivel em estoque.")]
    public int QuantidadeEstoque { get; init; }
}
