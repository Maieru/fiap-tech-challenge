namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.ListarServicos;

public sealed class ListarServicosCommand
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

