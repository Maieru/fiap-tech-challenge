namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;

public sealed class AdicionarPecaInsumoOrdemServicoCommand
{
    [Description("Identificador da ordem de servico. Em chamadas pela API, este valor e preenchido pela rota.")]
    public Guid OrdemServicoId { get; init; }

    [Description("Identificador da peca ou insumo do catalogo.")]
    public Guid PecaInsumoId { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Quantidade da peca ou insumo a adicionar na ordem de servico.")]
    public int Quantidade { get; init; }
}
