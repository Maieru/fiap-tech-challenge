using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FIAP.TechChallenge.Fase1.API.Tests;
using Reqnroll;

namespace FIAP.TechChallenge.Fase1.PostgreSQL.Tests.Features;

[Binding]
public sealed class FluxoLegadoSteps
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private Guid _ordemId;
    private string? _previousSigningKey;

    [Given("que a aplicação legada está autenticada com PostgreSQL")]
    public async Task GivenApplication()
    {
        _previousSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "bdd-test-signing-key-with-at-least-32-bytes");
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await TestAuthenticationHelper.ConfigureAuthenticatedClientAsync(_client);
    }

    [When("uma ordem percorre diagnóstico, aprovação e execução dos reparos")]
    public async Task WhenFlow()
    {
        var customer = await Post("/api/clientes", new { Nome = "Cliente BDD", Cpf = "52998224725", Telefone = "11987654321" });
        var vehicle = await Post("/api/veiculos", new { ClienteId = Id(customer), Placa = "ABC1234", Marca = "Toyota", Modelo = "Corolla", Ano = 2024 });
        var order = await Post("/api/ordensservico", new { ClienteId = Id(customer), VeiculoId = Id(vehicle), DescricaoProblema = "Revisao com reparo" });
        _ordemId = Id(order);
        var service = await Post("/api/servicos", new { Descricao = "Troca de oleo", ValorUnitario = 150m });
        await Put($"/api/ordensservico/{_ordemId}/iniciar-diagnostico", new { });
        var item = await Post($"/api/ordensservico/{_ordemId}/addservico", new { ServicoId = Id(service), Quantidade = 1 });
        await Put($"/api/ordensservico/{_ordemId}/solicitar-aprovacao", new { });
        using var approval = await _client.PutAsync($"/api/ordensservico/{_ordemId}/aprovar-execucao?token={order.GetProperty("token").GetString()}", null);
        Assert.That(approval.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await Put($"/api/ordensservico/servicos/{Id(item)}/concluir", new { TempoGastoMinutos = 30 });
    }

    [Then("a ordem deve ser finalizada e entregue com seus valores preservados")]
    public async Task ThenDelivered()
    {
        await Put($"/api/ordensservico/{_ordemId}/finalizar", new { });
        await Put($"/api/ordensservico/{_ordemId}/entregar", new { });
        var order = await _client.GetFromJsonAsync<JsonElement>($"/api/ordensservico/{_ordemId}");
        Assert.Multiple(() =>
        {
            Assert.That(order.GetProperty("status").GetInt32(), Is.EqualTo(6));
            Assert.That(order.GetProperty("valorTotalOrdemServico").GetDecimal(), Is.EqualTo(150m));
        });
    }

    [AfterScenario]
    public async Task Dispose()
    {
        _client?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        Environment.SetEnvironmentVariable("Jwt__SigningKey", _previousSigningKey);
    }

    private static Guid Id(JsonElement item) => item.GetProperty("id").GetGuid();
    private async Task<JsonElement> Post(string path, object body)
    {
        using var response = await _client.PostAsJsonAsync(path, body);
        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), text);
        return JsonDocument.Parse(text).RootElement.Clone();
    }
    private async Task Put(string path, object body)
    {
        using var response = await _client.PutAsJsonAsync(path, body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await response.Content.ReadAsStringAsync());
    }
}
