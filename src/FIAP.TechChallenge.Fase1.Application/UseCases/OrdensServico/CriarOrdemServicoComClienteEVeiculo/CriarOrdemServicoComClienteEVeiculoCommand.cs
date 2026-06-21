using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;

public sealed class CriarOrdemServicoComClienteEVeiculoCommand
{
    public CriarClienteCommand Cliente { get; init; } = new();
    public CriarVeiculoOrdemServicoCommand Veiculo { get; init; } = new();
    public string DescricaoProblema { get; init; } = string.Empty;
}

public sealed class CriarVeiculoOrdemServicoCommand
{
    public string Placa { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public int Ano { get; init; }
}

