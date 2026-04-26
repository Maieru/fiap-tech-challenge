namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.CadastrarServico;

public sealed class CadastrarServicoCommand
{
    [Required]
    [StringLength(1000)]
    [Description("Descricao do servico.")]
    public string Descricao { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Description("Valor unitario do servico.")]
    public decimal ValorUnitario { get; init; }
}
