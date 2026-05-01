namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;

public sealed class CriarOrdemServicoCommand
{
    public Guid ClienteId { get; init; }
    public Guid VeiculoId { get; init; }
    public string DescricaoProblema { get; init; } = string.Empty;
}
