using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[NonParallelizable]
public sealed class OsApiTests
{
    private OsTestHost _host = null!;
    [OneTimeSetUp] public async Task Start() { _host = new OsTestHost(); await _host.StartAsync(); }
    [SetUp] public Task Reset() => _host.ResetAsync();
    [OneTimeTearDown] public async Task Stop() => await _host.DisposeAsync();
    private HttpClient Client => _host.Client;
    private static Guid Id(JsonElement item) => item.GetProperty("id").GetGuid();

    [Test]
    public async Task CustomerAndVehicle_ShouldUpdateMutableFieldsAndUseRouteIdentifier()
    {
        var customer = await _host.CreateClientAsync();
        var customerId = Id(customer);
        var vehicle = await _host.CreateVehicleAsync(customerId);
        var vehicleId = Id(vehicle);
        using var updateCustomer = await Client.PutAsJsonAsync($"{OsTestHost.Root}/clientes/{customerId}", new
        {
            Id = Guid.NewGuid(), Nome = "Nome Atualizado", Telefone = "11988888888", Email = "novo@example.com", Cpf = "39053344705"
        });
        using var updateVehicle = await Client.PutAsJsonAsync($"{OsTestHost.Root}/veiculos/{vehicleId}", new
        {
            Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), Placa = "DEF5678", Marca = "Honda", Modelo = "Civic", Ano = 2025
        });
        var customerAfter = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/clientes/{customerId}");
        var vehicleAfter = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/veiculos/{vehicleId}");
        Assert.Multiple(() =>
        {
            Assert.That(updateCustomer.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updateVehicle.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(customerAfter.GetProperty("nome").GetString(), Is.EqualTo("Nome Atualizado"));
            Assert.That(customerAfter.GetProperty("cpf").GetString(), Is.EqualTo(customer.GetProperty("cpf").GetString()));
            Assert.That(vehicleAfter.GetProperty("clienteId").GetGuid(), Is.EqualTo(customerId));
            Assert.That(vehicleAfter.GetProperty("placa").GetString(), Is.EqualTo("DEF5678"));
        });
    }

    [Test]
    public async Task InvalidUpdate_ShouldLeavePersistedCustomerAndVehicleUnchanged()
    {
        var customer = await _host.CreateClientAsync();
        var vehicle = await _host.CreateVehicleAsync(Id(customer));
        using var customerUpdate = await Client.PutAsJsonAsync($"{OsTestHost.Root}/clientes/{Id(customer)}", new { Nome = " ", Telefone = "11988888888" });
        using var vehicleUpdate = await Client.PutAsJsonAsync($"{OsTestHost.Root}/veiculos/{Id(vehicle)}", new { Placa = "DEF5678", Marca = "Honda", Modelo = "Civic", Ano = 0 });
        var customerAfter = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/clientes/{Id(customer)}");
        var vehicleAfter = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/veiculos/{Id(vehicle)}");
        Assert.Multiple(() =>
        {
            Assert.That(customerUpdate.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(vehicleUpdate.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(customerAfter.GetProperty("nome").GetString(), Is.EqualTo(customer.GetProperty("nome").GetString()));
            Assert.That(vehicleAfter.GetProperty("placa").GetString(), Is.EqualTo(vehicle.GetProperty("placa").GetString()));
        });
    }

    [Test]
    public async Task SoftDelete_ShouldPreserveOrderAndHistoryAndAllowNewActiveIdentities()
    {
        var customer = await _host.CreateClientAsync();
        var vehicle = await _host.CreateVehicleAsync(Id(customer));
        Client.DefaultRequestHeaders.Add("X-Correlation-ID", "historical-order-123");
        var order = await _host.CreateOrderAsync(Id(customer), Id(vehicle));
        using var deleteCustomer = await Client.DeleteAsync($"{OsTestHost.Root}/clientes/{Id(customer)}");
        using var vehicleStillActive = await Client.GetAsync($"{OsTestHost.Root}/veiculos/{Id(vehicle)}");
        using var deleteVehicle = await Client.DeleteAsync($"{OsTestHost.Root}/veiculos/{Id(vehicle)}");
        using var hiddenCustomer = await Client.GetAsync($"{OsTestHost.Root}/clientes/{Id(customer)}");
        using var hiddenVehicle = await Client.GetAsync($"{OsTestHost.Root}/veiculos/{Id(vehicle)}");
        using var repeatedDelete = await Client.DeleteAsync($"{OsTestHost.Root}/clientes/{Id(customer)}");
        using var updateDeleted = await Client.PutAsJsonAsync($"{OsTestHost.Root}/veiculos/{Id(vehicle)}", new { Placa = "DEF5678", Marca = "Honda", Modelo = "Civic", Ano = 2025 });
        using var newOrderRejected = await Client.PostAsJsonAsync($"{OsTestHost.Root}/ordens", new { ClienteId = Id(customer), VeiculoId = Id(vehicle), DescricaoProblema = "Nova falha" });
        var replacementCustomer = await _host.CreateClientAsync();
        var replacementVehicle = await _host.CreateVehicleAsync(Id(replacementCustomer));
        var historical = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{Id(order)}");
        var history = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{Id(order)}/historico");
        Assert.Multiple(() =>
        {
            Assert.That(deleteCustomer.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(vehicleStillActive.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteVehicle.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(hiddenCustomer.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(hiddenVehicle.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(repeatedDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(updateDeleted.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(newOrderRejected.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(Id(replacementCustomer), Is.Not.EqualTo(Id(customer)));
            Assert.That(Id(replacementVehicle), Is.Not.EqualTo(Id(vehicle)));
            Assert.That(historical.GetProperty("clienteId").GetGuid(), Is.EqualTo(Id(customer)));
            Assert.That(historical.GetProperty("veiculoId").GetGuid(), Is.EqualTo(Id(vehicle)));
            Assert.That(history.GetArrayLength(), Is.EqualTo(1));
            Assert.That(history[0].GetProperty("tipo").GetString(), Is.EqualTo("OrdemCriada"));
            Assert.That(history[0].GetProperty("correlationId").GetString(), Is.EqualTo("historical-order-123"));
        });
    }

    [Test]
    public async Task Repository_ShouldRetainInactiveRowsAndRollbackOrderWhenHistoryFails()
    {
        var customer = await _host.CreateClientAsync();
        var vehicle = await _host.CreateVehicleAsync(Id(customer));
        await using var db = _host.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"HistoricoOrdens\" ADD CONSTRAINT \"test_history_failure\" CHECK (\"Tipo\" <> 'OrdemCriada')");
        try
        {
            var order = OrdemServico.Create(Id(customer), Id(vehicle), "Teste transacional").Value!;
            var repository = new OrdemServicoRepository(db);
            Assert.ThrowsAsync<DbUpdateException>(async () => await repository.AddAsync(order));
            db.ChangeTracker.Clear();
            Assert.Multiple(() =>
            {
                Assert.That(db.OrdensServico.Count(), Is.Zero);
                Assert.That(db.HistoricoOrdens.Count(), Is.Zero);
            });
        }
        finally
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"HistoricoOrdens\" DROP CONSTRAINT \"test_history_failure\"");
        }
        await Client.DeleteAsync($"{OsTestHost.Root}/clientes/{Id(customer)}");
        await Client.DeleteAsync($"{OsTestHost.Root}/veiculos/{Id(vehicle)}");
        var archivedCustomer = await db.Clientes.IgnoreQueryFilters().SingleAsync();
        var archivedVehicle = await db.Veiculos.IgnoreQueryFilters().SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(archivedCustomer.Ativo, Is.False);
            Assert.That(archivedVehicle.Ativo, Is.False);
            Assert.That(db.Database.HasPendingModelChanges(), Is.False);
        });
    }

    [Test]
    public async Task ConflictsAndFilters_ShouldUseNormalizedActiveValues()
    {
        var customer = await _host.CreateClientAsync();
        var vehicle = await _host.CreateVehicleAsync(Id(customer));
        using var conflictCustomer = await Client.PostAsJsonAsync($"{OsTestHost.Root}/clientes", new { Nome = "Duplicado", Cpf = "529.982.247-25", Telefone = "11987654321" });
        using var conflictVehicle = await Client.PostAsJsonAsync($"{OsTestHost.Root}/veiculos", new { ClienteId = Id(customer), Placa = "abc-1234", Marca = "Toyota", Modelo = "Corolla", Ano = 2024 });
        var filteredCustomer = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/clientes?cpf=52998224725");
        var filteredVehicle = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/veiculos?placa=ABC1234");
        var page = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/veiculos?clienteId={Id(customer)}");
        Assert.Multiple(() =>
        {
            Assert.That(conflictCustomer.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(conflictVehicle.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(Id(filteredCustomer), Is.EqualTo(Id(customer)));
            Assert.That(Id(filteredVehicle), Is.EqualTo(Id(vehicle)));
            Assert.That(page.GetProperty("totalItems").GetInt32(), Is.EqualTo(1));
        });
    }

    [TestCase("GET", "/clientes")]
    [TestCase("GET", "/clientes/11111111-1111-1111-1111-111111111111")]
    [TestCase("POST", "/clientes")]
    [TestCase("PUT", "/clientes/11111111-1111-1111-1111-111111111111")]
    [TestCase("DELETE", "/clientes/11111111-1111-1111-1111-111111111111")]
    [TestCase("GET", "/veiculos")]
    [TestCase("GET", "/veiculos/11111111-1111-1111-1111-111111111111")]
    [TestCase("POST", "/veiculos")]
    [TestCase("PUT", "/veiculos/11111111-1111-1111-1111-111111111111")]
    [TestCase("DELETE", "/veiculos/11111111-1111-1111-1111-111111111111")]
    [TestCase("GET", "/ordens")]
    [TestCase("POST", "/ordens")]
    [TestCase("GET", "/ordens/11111111-1111-1111-1111-111111111111")]
    [TestCase("GET", "/ordens/11111111-1111-1111-1111-111111111111/historico")]
    public async Task ProtectedRoute_ShouldRejectMissingToken(string method, string path)
    {
        Client.DefaultRequestHeaders.Authorization = null;
        using var request = new HttpRequestMessage(new HttpMethod(method), OsTestHost.Root + path) { Content = JsonContent.Create(new { }) };
        using var response = await Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase(true, "test-issuer", false)]
    [TestCase(false, "other-issuer", false)]
    [TestCase(false, "test-issuer", true)]
    public async Task Authentication_ShouldValidateRealToken(bool expired, string issuer, bool wrongKey)
    {
        _host.Authenticate(expired, issuer, wrongKey ? "different-signing-key-with-at-least-32-bytes" : null);
        using var response = await Client.GetAsync(OsTestHost.Root + "/ordens");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("/ordens?pageNumber=0")]
    [TestCase("/clientes?pageSize=0")]
    [TestCase("/veiculos?pageSize=0")]
    [TestCase("/veiculos?placa=ABC1234&clienteId=11111111-1111-1111-1111-111111111111")]
    public async Task Pagination_ShouldRejectInvalidValues(string path)
    {
        using var response = await Client.GetAsync(OsTestHost.Root + path);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body.GetProperty("error").GetString(), Is.Not.Empty);
        });
    }

    [Test]
    public async Task Orders_ShouldListWithFiltersAndRejectWrongOwnershipOrInactiveVehicle()
    {
        var first = await _host.CreateClientAsync();
        var second = await _host.CreateClientAsync("39053344705");
        var vehicle = await _host.CreateVehicleAsync(Id(first));
        await _host.CreateOrderAsync(Id(first), Id(vehicle));
        await _host.CreateOrderAsync(Id(first), Id(vehicle));
        var page = await Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens?clienteId={Id(first)}&veiculoId={Id(vehicle)}&status=1&pageNumber=2&pageSize=1&dataAberturaSortDirection=Asc");
        using var wrongOwner = await Client.PostAsJsonAsync($"{OsTestHost.Root}/ordens", new { ClienteId = Id(second), VeiculoId = Id(vehicle), DescricaoProblema = "Nova falha" });
        await Client.DeleteAsync($"{OsTestHost.Root}/veiculos/{Id(vehicle)}");
        using var inactiveVehicle = await Client.PostAsJsonAsync($"{OsTestHost.Root}/ordens", new { ClienteId = Id(first), VeiculoId = Id(vehicle), DescricaoProblema = "Nova falha" });
        using var missingHistory = await Client.GetAsync($"{OsTestHost.Root}/ordens/{Guid.NewGuid()}/historico");
        Assert.Multiple(() =>
        {
            Assert.That(page.GetProperty("totalItems").GetInt32(), Is.EqualTo(2));
            Assert.That(page.GetProperty("ordensServico").GetArrayLength(), Is.EqualTo(1));
            Assert.That(wrongOwner.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(inactiveVehicle.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(missingHistory.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }
}
