using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace FIAP.TechChallenge.OS.Integration.Tests;

public sealed class OsTestHost : IAsyncDisposable
{
    private const string SigningKey = "integration-tests-signing-key-at-least-32-bytes";
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.3").WithDatabase("fiap_os").Build();
    private WebApplicationFactory<API.Program> _factory = null!;
    public HttpClient Client { get; private set; } = null!;
    public const string Root = "/api/os/v1";
    public string ConnectionString => _database.GetConnectionString();
    public bool ManualAuthorizationEnabled { get; init; }
    public string EnvironmentName { get; init; } = "Testing";
    public string? KafkaBootstrapServers { get; init; }

    public async Task StartAsync()
    {
        await _database.StartAsync();
        await using (var db = CreateDbContext()) await db.Database.MigrateAsync();
        _factory = new Factory(ConnectionString, ManualAuthorizationEnabled, EnvironmentName, KafkaBootstrapServers);
        Client = _factory.CreateClient();
        Authenticate();
    }

    public AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options);

    public void Authenticate(bool expired = false, string issuer = "test-issuer", string? signingKey = null)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(expired, issuer, signingKey));
    }

    internal static string CreateToken(bool expired = false, string issuer = "test-issuer", string? signingKey = null)
    {
        var now = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer, Audience = "test-audience", Subject = new ClaimsIdentity([new Claim("sub", "operator")]),
            IssuedAt = now.AddHours(-2), NotBefore = now.AddHours(-2), Expires = expired ? now.AddHours(-1) : now.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey ?? SigningKey)), SecurityAlgorithms.HmacSha256)
        });
    }

    public async Task ResetAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Outbox\", \"Inbox\", \"DispatcherLeases\", \"HistoricoOrdens\", \"OrdensServico\", \"Veiculos\", \"Clientes\" CASCADE");
        Client.DefaultRequestHeaders.Remove("X-Correlation-ID");
        Authenticate();
    }

    public async Task<JsonElement> CreateClientAsync(string cpf = "52998224725") =>
        await PostAsync("/clientes", new { Nome = "Cliente Teste", Cpf = cpf, Telefone = "11987654321", Email = "teste@example.com" });

    public Task<JsonElement> CreateVehicleAsync(Guid clienteId, string placa = "ABC1234") =>
        PostAsync("/veiculos", new { ClienteId = clienteId, Placa = placa, Marca = "Toyota", Modelo = "Corolla", Ano = 2024 });

    public Task<JsonElement> CreateOrderAsync(Guid clienteId, Guid veiculoId) =>
        PostAsync("/ordens", new { ClienteId = clienteId, VeiculoId = veiculoId, DescricaoProblema = "Motor com falha" });

    public async Task<JsonElement> PostAsync(string path, object data)
    {
        using var response = await Client.PostAsJsonAsync(Root + path, data);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), body);
            Assert.That(response.Headers.Location, Is.Not.Null);
        });
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        await _database.DisposeAsync();
    }

    private sealed class Factory(string connectionString, bool manualAuthorization, string environment, string? kafka) : WebApplicationFactory<API.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:Issuer"] = "test-issuer", ["Jwt:Audience"] = "test-audience", ["Jwt:SigningKey"] = SigningKey,
                ["Observability:EnableOtlp"] = "false", ["Database:ApplyMigrations"] = "false",
                ["ManualExecutionAuthorizationEnabled"] = manualAuthorization.ToString(), ["Logging:LogLevel:Default"] = "Warning"
                , ["Messaging:Enabled"] = (kafka is not null).ToString(), ["Messaging:BootstrapServers"] = kafka ?? "localhost:9092",
                ["Messaging:RetryDelaysSeconds:0"] = "0", ["Messaging:RetryDelaysSeconds:1"] = "0", ["Messaging:RetryDelaysSeconds:2"] = "0",
                ["Messaging:RetryDelaysSeconds:3"] = "0", ["Messaging:RetryDelaysSeconds:4"] = "0"
            }));
        }
    }
}
