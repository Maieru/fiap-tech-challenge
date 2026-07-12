using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoCompleta;

public sealed class CriarOrdemServicoCompletaCommand
{
    public CriarClienteCommand Cliente { get; init; } = new();
    public CriarVeiculoOrdemServicoCommand Veiculo { get; init; } = new();
    public string DescricaoProblema { get; init; } = string.Empty;
    public IReadOnlyCollection<ItemServicoOrdemServicoCommand> Servicos { get; init; } = [];
    public IReadOnlyCollection<ItemPecaInsumoOrdemServicoCommand> PecasInsumos { get; init; } = [];
}

public sealed class ItemServicoOrdemServicoCommand
{
    public Guid ServicoId { get; init; }
    public int Quantidade { get; init; }
}

public sealed class ItemPecaInsumoOrdemServicoCommand
{
    public Guid PecaInsumoId { get; init; }
    public int Quantidade { get; init; }
}
