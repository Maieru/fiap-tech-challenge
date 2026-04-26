namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.ExcluirPecaInsumo;

public sealed class ExcluirPecaInsumoCommand
{
    [Description("Identificador da peca ou insumo a excluir.")]
    public Guid Id { get; set; }
}
