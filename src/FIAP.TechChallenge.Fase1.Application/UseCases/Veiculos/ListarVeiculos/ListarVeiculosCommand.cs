namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;

public sealed class ListarVeiculosCommand
{
    public string? Placa { get; init; }
    public Guid? ClienteId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

