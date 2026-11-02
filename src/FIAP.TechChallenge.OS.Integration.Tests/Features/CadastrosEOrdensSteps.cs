using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Reqnroll;

namespace FIAP.TechChallenge.OS.Integration.Tests.Features;

[Binding]
public sealed class CadastrosEOrdensSteps
{
    private OsTestHost _host = null!;
    private Guid _customerId;
    private Guid _vehicleId;
    private Guid _orderId;

    [BeforeScenario]
    public async Task Start() { _host = new OsTestHost(); await _host.StartAsync(); }
    [AfterScenario]
    public async Task Stop() { if (_host is not null) await _host.DisposeAsync(); }

    [Given("um cliente e um veículo cadastrados no OS Service")]
    public async Task GivenRegistrations()
    {
        _customerId = (await _host.CreateClientAsync()).GetProperty("id").GetGuid();
        _vehicleId = (await _host.CreateVehicleAsync(_customerId)).GetProperty("id").GetGuid();
    }

    [When("atualizo o nome do cliente e a placa do veículo")]
    public async Task WhenUpdated()
    {
        using var customer = await _host.Client.PutAsJsonAsync($"{OsTestHost.Root}/clientes/{_customerId}", new { Nome = "Nome Editado", Telefone = "11987654321" });
        using var vehicle = await _host.Client.PutAsJsonAsync($"{OsTestHost.Root}/veiculos/{_vehicleId}", new { Placa = "DEF5678", Marca = "Toyota", Modelo = "Corolla", Ano = 2024 });
        Assert.Multiple(() =>
        {
            Assert.That(customer.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(vehicle.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [Then("os novos dados devem ser consultáveis com os mesmos identificadores")]
    public async Task ThenUpdated()
    {
        var customer = await _host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/clientes/{_customerId}");
        var vehicle = await _host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/veiculos/{_vehicleId}");
        Assert.Multiple(() =>
        {
            Assert.That(customer.GetProperty("id").GetGuid(), Is.EqualTo(_customerId));
            Assert.That(customer.GetProperty("nome").GetString(), Is.EqualTo("Nome Editado"));
            Assert.That(vehicle.GetProperty("id").GetGuid(), Is.EqualTo(_vehicleId));
            Assert.That(vehicle.GetProperty("placa").GetString(), Is.EqualTo("DEF5678"));
        });
    }

    [Given("uma ordem aberta para um cliente e um veículo ativos")]
    public async Task GivenOrder()
    {
        await GivenRegistrations();
        _orderId = (await _host.CreateOrderAsync(_customerId, _vehicleId)).GetProperty("id").GetGuid();
    }

    [When("excluo logicamente o cliente e o veículo")]
    public async Task WhenArchived()
    {
        using var customer = await _host.Client.DeleteAsync($"{OsTestHost.Root}/clientes/{_customerId}");
        using var vehicle = await _host.Client.DeleteAsync($"{OsTestHost.Root}/veiculos/{_vehicleId}");
        Assert.Multiple(() =>
        {
            Assert.That(customer.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(vehicle.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        });
    }

    [Then("os cadastros ficam ocultos e a ordem mantém seu histórico")]
    public async Task ThenHistoryPreserved()
    {
        using var customer = await _host.Client.GetAsync($"{OsTestHost.Root}/clientes/{_customerId}");
        using var vehicle = await _host.Client.GetAsync($"{OsTestHost.Root}/veiculos/{_vehicleId}");
        var order = await _host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{_orderId}");
        var history = await _host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{_orderId}/historico");
        Assert.Multiple(() =>
        {
            Assert.That(customer.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(vehicle.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(order.GetProperty("clienteId").GetGuid(), Is.EqualTo(_customerId));
            Assert.That(order.GetProperty("veiculoId").GetGuid(), Is.EqualTo(_vehicleId));
            Assert.That(history.GetArrayLength(), Is.EqualTo(1));
        });
    }
}
