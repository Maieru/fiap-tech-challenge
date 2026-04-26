namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;

public sealed class AdicionarServicoOrdemServicoCommand
{
    [Description("Identificador da ordem de servico. Em chamadas pela API, este valor e preenchido pela rota.")]
    public Guid OrdemServicoId { get; init; }

    [Description("Identificador do servico do catalogo.")]
    public Guid ServicoId { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Quantidade do servico a adicionar na ordem de servico.")]
    public int Quantidade { get; init; }
}
