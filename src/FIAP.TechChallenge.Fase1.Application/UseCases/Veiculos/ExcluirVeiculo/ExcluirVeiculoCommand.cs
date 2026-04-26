namespace FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ExcluirVeiculo;

public sealed class ExcluirVeiculoCommand
{
    [Description("Identificador do veiculo a excluir.")]
    public Guid Id { get; set; }
}
