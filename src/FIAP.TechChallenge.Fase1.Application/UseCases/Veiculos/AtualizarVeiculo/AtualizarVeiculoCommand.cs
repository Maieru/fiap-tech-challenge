namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;

public sealed class AtualizarVeiculoCommand
{
    [Description("Identificador do veiculo. Em atualizacoes pela API, este valor e preenchido pela rota.")]
    public Guid Id { get; init; }

    [Required]
    [StringLength(7, MinimumLength = 7)]
    [RegularExpression(@"^[A-Za-z]{3}\d{4}$|^[A-Za-z]{3}\d[A-Za-z]\d{2}$")]
    [Description("Placa do veiculo no padrao antigo (AAA1234) ou Mercosul (AAA1A23).")]
    public string Placa { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    [Description("Marca do veiculo.")]
    public string Marca { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    [Description("Modelo do veiculo.")]
    public string Modelo { get; init; } = string.Empty;

    [Range(1, 9999)]
    [Description("Ano de fabricacao/modelo do veiculo.")]
    public int Ano { get; init; }
}
