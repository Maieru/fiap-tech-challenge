namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;

public sealed class RecuperarVeiculoResponse
{
    public Guid Id { get; init; }
    public Guid ClienteId { get; init; }
    public string Placa { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public int Ano { get; init; }
}

