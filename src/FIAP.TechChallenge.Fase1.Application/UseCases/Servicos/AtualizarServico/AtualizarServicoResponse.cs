namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.AtualizarServico;

public sealed class AtualizarServicoResponse
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal ValorUnitario { get; init; }
}

