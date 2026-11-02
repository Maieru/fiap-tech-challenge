using System.Text.Json;
using System.Text.Json.Nodes;

namespace FIAP.TechChallenge.OS.Application.Messaging.Tests;

public sealed class ContractTests
{
    private static readonly Guid OrderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static IEnumerable<TestCaseData> Messages()
    {
        yield return new TestCaseData(new OrdemServicoCriada(OrderId,
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Guid.Parse("33333333-3333-3333-3333-333333333333"), "Motor com falha"),
            """{"ordemServicoId":"11111111-1111-1111-1111-111111111111","clienteId":"22222222-2222-2222-2222-222222222222","veiculoId":"33333333-3333-3333-3333-333333333333","descricaoProblema":"Motor com falha"}""");
        yield return new TestCaseData(new ExecucaoAutorizada(OrderId),
            """{"ordemServicoId":"11111111-1111-1111-1111-111111111111"}""");
        yield return new TestCaseData(new CancelamentoSolicitado(OrderId),
            """{"ordemServicoId":"11111111-1111-1111-1111-111111111111"}""");
    }

    [TestCaseSource(nameof(Messages))]
    public void Messages_ShouldPreserveVersionOneJson(object message, string expectedJson)
    {
        var actual = JsonSerializer.Serialize(message, message.GetType(), Options);
        var recovered = JsonSerializer.Deserialize(expectedJson, message.GetType(), Options);
        Assert.Multiple(() =>
        {
            Assert.That(JsonNode.DeepEquals(JsonNode.Parse(actual), JsonNode.Parse(expectedJson)), Is.True);
            Assert.That(recovered, Is.EqualTo(message));
        });
    }

    [Test]
    public void Routing_ShouldKeepPublishedIdentifiersStable()
    {
        Assert.Multiple(() =>
        {
            Assert.That(OSMessaging.Topic, Is.EqualTo("oficina.os.events.v1"));
            Assert.That(OSMessaging.Version, Is.EqualTo(1));
            Assert.That(OrdemServicoCriada.Type, Is.EqualTo("os.ordem-servico-criada"));
            Assert.That(ExecucaoAutorizada.Type, Is.EqualTo("os.execucao-autorizada"));
            Assert.That(CancelamentoSolicitado.Type, Is.EqualTo("os.cancelamento-solicitado"));
            Assert.That(typeof(OrdemServicoCriada).Assembly.GetReferencedAssemblies()
                .Any(x => x.Name!.StartsWith("FIAP.TechChallenge.") || x.Name.StartsWith("Confluent.")), Is.False);
        });
    }
}
