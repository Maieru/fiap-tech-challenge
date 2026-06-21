namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.RecuperarServico;

public sealed class RecuperarServicoResponse
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal ValorUnitario { get; init; }
}

