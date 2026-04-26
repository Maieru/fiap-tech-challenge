namespace FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.ExcluirServico;

public sealed class ExcluirServicoCommand
{
    [Description("Identificador do servico a excluir.")]
    public Guid Id { get; set; }
}
