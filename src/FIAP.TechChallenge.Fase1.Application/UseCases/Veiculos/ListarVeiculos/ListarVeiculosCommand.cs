namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;

public sealed class ListarVeiculosCommand
{
    [StringLength(7, MinimumLength = 7)]
    [RegularExpression(@"^[A-Za-z]{3}\d{4}$|^[A-Za-z]{3}\d[A-Za-z]\d{2}$")]
    [Description("Filtra por placa no padrao antigo (AAA1234) ou Mercosul (AAA1A23).")]
    public string? Placa { get; init; }

    [Description("Filtra por cliente.")]
    public Guid? ClienteId { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Numero da pagina.")]
    public int PageNumber { get; init; } = 1;

    [Range(1, int.MaxValue)]
    [Description("Quantidade de itens por pagina.")]
    public int PageSize { get; init; } = 10;
}
