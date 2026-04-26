using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;

public sealed class ListarOrdensServicoCommand
{
    [Description("Filtra ordens de servico por cliente.")]
    public Guid? ClienteId { get; init; }

    [Description("Filtra ordens de servico por veiculo.")]
    public Guid? VeiculoId { get; init; }

    [Description("Filtra ordens de servico por status.")]
    public StatusOrdemServico? Status { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Numero da pagina.")]
    public int PageNumber { get; init; } = 1;

    [Range(1, int.MaxValue)]
    [Description("Quantidade de itens por pagina.")]
    public int PageSize { get; init; } = 10;
}
