namespace FIAP.TechChallenge.OS.Domain.Entities;

public sealed record ItemOrcamento(Guid ItemId, string Descricao, int Quantidade, decimal ValorUnitario, decimal ValorTotal);
public sealed record OrcamentoOrdem(IReadOnlyList<ItemOrcamento> Servicos, IReadOnlyList<ItemOrcamento> PecasInsumos)
{
    public bool Valido(decimal total)
    {
        if (Servicos is null || PecasInsumos is null) return false;
        var items = Servicos.Concat(PecasInsumos).ToArray();
        try
        {
            return items.All(x => x is not null && x.ItemId != Guid.Empty && !string.IsNullOrWhiteSpace(x.Descricao) && x.Quantidade > 0 && x.ValorUnitario >= 0 && x.ValorTotal == x.ValorUnitario * x.Quantidade)
                && items.Select(x => x.ItemId).Distinct().Count() == items.Length && items.Sum(x => x.ValorTotal) == total;
        }
        catch (OverflowException) { return false; }
    }
}
