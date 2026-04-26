namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.AtualizarPecaInsumo;

public sealed class AtualizarPecaInsumoCommand
{
    [Description("Identificador da peca ou insumo. Em atualizacoes pela API, este valor e preenchido pela rota.")]
    public Guid Id { get; init; }

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

    [Description("Indica se a peca ou insumo permanece ativo no catalogo.")]
    public bool Ativo { get; init; }
}
