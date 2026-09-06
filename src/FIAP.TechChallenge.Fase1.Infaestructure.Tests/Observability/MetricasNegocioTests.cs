using FIAP.TechChallenge.Fase1.Domain.Enums;
using System.Diagnostics.Metrics;
using System.Transactions;
using FIAP.TechChallenge.Fase1.Domain.Observability;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Notification.Mail;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Observability;

[TestFixture, NonParallelizable]
public class MetricasNegocioTests
{
    private MeterListener _listener = null!;
    private readonly List<(string Name, double Value, string? Tag)> _measurements = [];

    [SetUp]
    public void SetUp()
    {
        _measurements.Clear();
        _listener = new MeterListener();
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == MetricasNegocio.MeterName)
                listener.EnableMeasurementEvents(instrument);
        };
        _listener.SetMeasurementEventCallback<long>((i, v, tags, _) =>
            _measurements.Add((i.Name, v, tags.Length > 0 ? tags[0].Value?.ToString() : null)));
        _listener.SetMeasurementEventCallback<double>((i, v, tags, _) =>
            _measurements.Add((i.Name, v, tags.Length > 0 ? tags[0].Value?.ToString() : null)));
        _listener.Start();
    }

    [TearDown]
    public void TearDown() => _listener.Dispose();

    [TestCase(true)]
    [TestCase(false)]
    public void Creation_IsRecordedOnlyAfterCommit(bool commit)
    {
        using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
        {
            MetricasNegocio.RegistrarOrdemCriada();
            Assert.That(_measurements, Is.Empty);
            if (commit) scope.Complete();
        }
        Assert.That(_measurements.Count, Is.EqualTo(commit ? 1 : 0));
        if (commit) Assert.That(_measurements[0], Is.EqualTo(("oficina.ordens.criadas", 1d, (string?)null)));
    }

    [TestCase(StatusOrdemServico.EmDiagnostico, "diagnostico")]
    [TestCase(StatusOrdemServico.EmExecucao, "execucao")]
    [TestCase(StatusOrdemServico.Finalizada, "finalizacao")]
    public void Stage_RecordsElapsedSeconds(StatusOrdemServico etapa, string expectedTag)
    {
        var start = DateTime.UtcNow;
        MetricasNegocio.RegistrarEtapaConcluida(etapa, start, start.AddMinutes(2));
        Assert.That(_measurements.Single(), Is.EqualTo(("oficina.ordens.etapa.duracao", 120d, expectedTag)));
    }

    private sealed class StubMailService(Result<bool> result, Exception? exception) : IMailService
    {
        public Task<Result<bool>> SendMail(string to, string subject, string body) =>
            exception is null ? Task.FromResult(result) : Task.FromException<Result<bool>>(exception);
    }

    [TestCase("success", 0)]
    [TestCase("failure", 1)]
    [TestCase("false", 1)]
    [TestCase("exception", 1)]
    [TestCase("cancel", 0)]
    public async Task Email_RecordsFailuresOnceAndPreservesOutcome(string outcome, int count)
    {
        Exception? exception = outcome switch
        {
            "exception" => new InvalidOperationException("Provider failed"),
            "cancel" => new OperationCanceledException(),
            _ => null
        };
        var result = outcome == "failure" ? Result<bool>.Failure(new Error("Rejected")) : Result<bool>.Success(outcome != "false");
        var service = new MeteredMailService(new StubMailService(result, exception));
        if (exception is not null)
        {
            var thrown = Assert.ThrowsAsync(exception.GetType(), async () => await service.SendMail("to", "subject", "body"));
            Assert.That(thrown, Is.SameAs(exception));
        }
        else
        {
            var actual = await service.SendMail("to", "subject", "body");
            Assert.That(actual, Is.EqualTo(result));
        }
        Assert.That(_measurements.Count, Is.EqualTo(count));
        if (count > 0) Assert.That(_measurements.Single(), Is.EqualTo(("oficina.integracoes.falhas", 1d, "email")));
    }
}
