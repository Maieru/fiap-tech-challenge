using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ListarOrdensServico;

public sealed class ListarOrdensServicoCommand
{
    public Guid? ClienteId { get; init; }
    public Guid? VeiculoId { get; init; }
    public StatusOrdemServico[] Status { get; init; } = [];
    public SortDirection? StatusSortDirection { get; init; }
    public SortDirection? DataAberturaSortDirection { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
