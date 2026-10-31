using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[Category("ContainerImage")]
[NonParallelizable]
public sealed class OsImageSmokeTests
{
    [Test]
    public async Task Image_ShouldPreserveOrderAfterSoftDeleteAndRestart()
    {
        var image = Environment.GetEnvironmentVariable("OS_TEST_IMAGE");
        if (string.IsNullOrWhiteSpace(image))
            Assert.Ignore("Build the OS image and set OS_TEST_IMAGE before selecting Category=ContainerImage.");

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await using var network = new NetworkBuilder().Build();
        await network.CreateAsync(timeout.Token);
        var password = Guid.NewGuid().ToString("N");
        var signingKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        await using var database = new PostgreSqlBuilder("postgres:18.3")
            .WithNetwork(network).WithNetworkAliases("database")
            .WithDatabase("fiap_os").WithUsername("fiap_os").WithPassword(password).Build();
        await database.StartAsync(timeout.Token);
        await using var api = new ContainerBuilder(image!)
            .WithNetwork(network).WithPortBinding(8080, true)
            .WithEnvironment(new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development", ["ASPNETCORE_HTTP_PORTS"] = "8080",
                ["ConnectionStrings__DefaultConnection"] = $"Host=database;Database=fiap_os;Username=fiap_os;Password={password};GSS Encryption Mode=Disable",
                ["Database__ApplyMigrations"] = "true", ["Observability__EnableOtlp"] = "false",
                ["Jwt__Issuer"] = "test-issuer", ["Jwt__Audience"] = "test-audience", ["Jwt__SigningKey"] = signingKey
            })
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request =>
                request.ForPort(8080).ForPath(OsTestHost.Root + "/health/ready").ForStatusCode(HttpStatusCode.OK)))
            .Build();
        await api.StartAsync(timeout.Token);

        HttpClient CreateClient()
        {
            var client = new HttpClient { BaseAddress = new UriBuilder("http", api.Hostname, api.GetMappedPublicPort(8080)).Uri };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OsTestHost.CreateToken(signingKey: signingKey));
            client.DefaultRequestHeaders.Add("X-Correlation-ID", "image-smoke");
            return client;
        }

        using var client = CreateClient();
        var customer = await PostAsync(client, "/clientes", new { Nome = "Cliente Imagem", Cpf = "52998224725", Telefone = "11987654321" });
        var customerId = customer.GetProperty("id").GetGuid();
        var vehicle = await PostAsync(client, "/veiculos", new { ClienteId = customerId, Placa = "ABC1234", Marca = "Toyota", Modelo = "Corolla", Ano = 2024 });
        var vehicleId = vehicle.GetProperty("id").GetGuid();
        var order = await PostAsync(client, "/ordens", new { ClienteId = customerId, VeiculoId = vehicleId, DescricaoProblema = "Verificar motor" });
        var orderId = order.GetProperty("id").GetGuid();
        using var update = await client.PutAsJsonAsync($"{OsTestHost.Root}/clientes/{customerId}", new { Nome = "Nome Atualizado", Telefone = "11987654321" }, timeout.Token);
        using var deleteCustomer = await client.DeleteAsync($"{OsTestHost.Root}/clientes/{customerId}", timeout.Token);
        using var deleteVehicle = await client.DeleteAsync($"{OsTestHost.Root}/veiculos/{vehicleId}", timeout.Token);
        using var anonymous = new HttpClient { BaseAddress = client.BaseAddress };
        using var unauthorized = await anonymous.GetAsync(OsTestHost.Root + "/clientes", timeout.Token);
        Assert.Multiple(() =>
        {
            Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteCustomer.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deleteVehicle.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(unauthorized.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });

        await api.StopAsync(timeout.Token);
        await api.StartAsync(timeout.Token);
        using var restarted = CreateClient();
        var persisted = await restarted.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{orderId}", timeout.Token);
        var history = await restarted.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{orderId}/historico", timeout.Token);
        var spec = await restarted.GetFromJsonAsync<JsonElement>("/openapi/v1.json", timeout.Token);
        Assert.Multiple(() =>
        {
            Assert.That(persisted.GetProperty("clienteId").GetGuid(), Is.EqualTo(customerId));
            Assert.That(history.GetArrayLength(), Is.EqualTo(1));
            Assert.That(history[0].GetProperty("correlationId").GetString(), Is.EqualTo("image-smoke"));
            Assert.That(spec.GetProperty("paths").GetProperty("/api/os/v1/clientes/{id}").TryGetProperty("delete", out _), Is.True);
            Assert.That(spec.GetProperty("paths").GetProperty("/api/os/v1/veiculos/{id}").TryGetProperty("put", out _), Is.True);
        });
        var output = Environment.GetEnvironmentVariable("OS_OPENAPI_OUTPUT");
        if (!string.IsNullOrWhiteSpace(output))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
            await File.WriteAllTextAsync(output, spec.GetRawText(), timeout.Token);
        }
    }

    private static async Task<JsonElement> PostAsync(HttpClient client, string path, object payload)
    {
        using var response = await client.PostAsJsonAsync(OsTestHost.Root + path, payload);
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), body);
        return JsonDocument.Parse(body).RootElement.Clone();
    }
}
