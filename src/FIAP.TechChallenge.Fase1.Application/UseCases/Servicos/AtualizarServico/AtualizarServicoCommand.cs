namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.AtualizarServico;

public sealed class AtualizarServicoCommand
{
    [Description("Identificador do servico. Em atualizacoes pela API, este valor e preenchido pela rota.")]
    public Guid Id { get; init; }

    [Required]
    [StringLength(1000)]
    [Description("Descricao do servico.")]
    public string Descricao { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Description("Valor unitario do servico.")]
    public decimal ValorUnitario { get; init; }
}
