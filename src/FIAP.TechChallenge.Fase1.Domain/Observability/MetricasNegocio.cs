using FIAP.TechChallenge.Fase1.Domain.Enums;
using System.Diagnostics.Metrics;
using System.Transactions;

namespace FIAP.TechChallenge.Fase1.Domain.Observability;

public static class MetricasNegocio
{
    public const string MeterName = "Fiap.TechChallenge.Negocio";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> OrdensCriadas = Meter.CreateCounter<long>("oficina.ordens.criadas");
    private static readonly Histogram<double> DuracaoEtapa = Meter.CreateHistogram<double>("oficina.ordens.etapa.duracao", "s");
    private static readonly Counter<long> FalhasIntegracao = Meter.CreateCounter<long>("oficina.integracoes.falhas");

    public static void RegistrarOrdemCriada() => AposCommit(() => OrdensCriadas.Add(1));

    public static void RegistrarEtapaConcluida(StatusOrdemServico etapa, DateTime inicio, DateTime fim)
    {
        var nomeEtapa = etapa switch
        {
            StatusOrdemServico.EmDiagnostico => "diagnostico",
            StatusOrdemServico.EmExecucao => "execucao",
            StatusOrdemServico.Finalizada => "finalizacao",
            _ => throw new ArgumentOutOfRangeException(nameof(etapa), etapa, "Etapa invalida.")
        };
        var segundos = (fim - inicio).TotalSeconds;
        if (segundos < 0) return;
        AposCommit(() => DuracaoEtapa.Record(segundos, new KeyValuePair<string, object?>("etapa", nomeEtapa)));
    }

    public static void RegistrarFalhaIntegracao(string integracao) =>
        FalhasIntegracao.Add(1, new KeyValuePair<string, object?>("integracao", integracao));

    private static void AposCommit(Action registrar)
    {
        var transaction = Transaction.Current;
        if (transaction is null)
        {
            registrar();
            return;
        }

        // SaveChanges pode estar dentro da transacao da criacao completa.
        transaction.TransactionCompleted += (_, args) =>
        {
            if (args.Transaction?.TransactionInformation.Status == TransactionStatus.Committed)
                registrar();
        };
    }
}
