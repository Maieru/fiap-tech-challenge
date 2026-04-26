namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;

public sealed class ConcluirServicoOrdemServicoCommand
{
    [Description("Identificador do servico adicionado a ordem de servico. Em chamadas pela API, este valor e preenchido pela rota.")]
    public Guid ServicoDaOrdemDeServicoId { get; init; }

    [Range(1, int.MaxValue)]
    [Description("Tempo gasto para concluir o servico, em minutos.")]
    public int TempoGastoMinutos { get; init; }
}
