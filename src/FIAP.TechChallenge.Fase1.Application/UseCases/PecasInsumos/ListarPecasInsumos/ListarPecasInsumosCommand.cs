namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.ListarPecasInsumos;

public sealed class ListarPecasInsumosCommand
{
    public Guid? Id { get; init; }
    public string? Codigo { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
