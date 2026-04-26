namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.RecuperarPecaInsumo;

public sealed class RecuperarPecaInsumoCommand
{
    [Description("Identificador da peca ou insumo.")]
    public Guid PecaInsumoId { get; init; }
}
