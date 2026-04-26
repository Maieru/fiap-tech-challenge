namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.ListarServicos;

public sealed class ListarServicosCommand
{
    [Range(1, int.MaxValue)]
    [Description("Numero da pagina.")]
    public int PageNumber { get; init; } = 1;

    [Range(1, int.MaxValue)]
    [Description("Quantidade de itens por pagina.")]
    public int PageSize { get; init; } = 10;
}
