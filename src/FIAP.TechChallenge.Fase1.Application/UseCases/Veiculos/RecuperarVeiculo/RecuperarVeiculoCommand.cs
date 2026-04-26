namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;

public sealed class RecuperarVeiculoCommand
{
    [Description("Identificador do veiculo.")]
    public Guid VeiculoId { get; init; }
}
