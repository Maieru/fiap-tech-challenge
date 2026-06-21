namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;

public sealed class ListarVeiculosResponse
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public IReadOnlyCollection<ListarVeiculoItemResponse> Veiculos { get; init; } = [];
}

public sealed class ListarVeiculoItemResponse
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public string Placa { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public int Ano { get; init; }
}

