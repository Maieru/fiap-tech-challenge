namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;

public sealed class CriarOrdemServicoCommand
{
    [Description("Identificador do cliente dono do veiculo.")]
    public Guid ClienteId { get; init; }

    [Description("Identificador do veiculo associado a ordem de servico.")]
    public Guid VeiculoId { get; init; }

    [Required]
    [StringLength(1000, MinimumLength = 3)]
    [Description("Descricao do problema relatado pelo cliente.")]
    public string DescricaoProblema { get; init; } = string.Empty;
}
