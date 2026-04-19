namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;

public sealed class AdicionarServicoOrdemServicoResponse
{
    public Guid Id { get; init; }
    public Guid OrdemServicoId { get; init; }
    public Guid ServicoId { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal ValorUnitario { get; init; }
    public int Quantidade { get; init; }
    public decimal ValorTotal { get; init; }
    public int? TempoGastoMinutos { get; init; }
    public bool Concluido { get; init; }
}
