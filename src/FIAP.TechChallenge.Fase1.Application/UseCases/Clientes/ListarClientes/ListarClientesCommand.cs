namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;

public sealed class ListarClientesCommand
{
    [StringLength(14, MinimumLength = 11)]
    [Description("Filtra por CPF. Use este filtro ou CNPJ, nao ambos.")]
    public string? Cpf { get; init; }

    [StringLength(18, MinimumLength = 14)]
    [Description("Filtra por CNPJ. Use este filtro ou CPF, nao ambos.")]
    public string? Cnpj { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Numero da pagina.")]
    public int PageNumber { get; init; } = 1;

    [Range(1, int.MaxValue)]
    [Description("Quantidade de itens por pagina.")]
    public int PageSize { get; init; } = 10;
}
