using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
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
using Testcontainers.Kafka;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace FIAP.TechChallenge.OS.Integration.Tests;

public sealed class OsTestHost : IAsyncDisposable
{
    private static readonly string SigningKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private string? previousSigningKey;
    private bool signingKeyConfigured;
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.3").WithDatabase("fiap_os").Build();
    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.9.2").WithKRaft()
        .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "false").Build();
    private WebApplicationFactory<API.Program> _factory = null!;
    public HttpClient Client { get; private set; } = null!;
    public const string Root = "/api/os/v1";
    public string ConnectionString => _database.GetConnectionString();
    public string BootstrapServers => _kafka.GetBootstrapAddress();
    public IServiceProvider Services => _factory.Services;
    public Task StopKafkaAsync() => _kafka.StopAsync();

    public async Task StartAsync()
    {
        await Task.WhenAll(_database.StartAsync(), _kafka.StartAsync());
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = BootstrapServers }).Build();
        await admin.CreateTopicsAsync([
            new TopicSpecification { Name = "oficina.os.events.v1", NumPartitions = 1, ReplicationFactor = 1 },
            new TopicSpecification { Name = "oficina.execution.events.v1", NumPartitions = 1, ReplicationFactor = 1 }
        ]);
        await using (var db = CreateDbContext()) await db.Database.MigrateAsync();
        previousSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", SigningKey);
        signingKeyConfigured = true;
        try
        {
            _factory = CreateFactory();
            Client = _factory.CreateClient();
            Authenticate();
        }
        catch { RestoreSigningKey(); throw; }
    }

    public AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options);

    internal WebApplicationFactory<API.Program> CreateFactory() => new Factory(ConnectionString, BootstrapServers);

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
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"MensagensProcessadas\", \"HistoricoOrdens\", \"OrdensServico\", \"Veiculos\", \"Clientes\" CASCADE");
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
        try
        {
            Client?.Dispose();
            if (_factory is not null) await _factory.DisposeAsync();
            await _database.DisposeAsync();
            await _kafka.DisposeAsync();
        }
        finally { RestoreSigningKey(); }
    }

    private void RestoreSigningKey()
    {
        if (!signingKeyConfigured) return;
        Environment.SetEnvironmentVariable("Jwt__SigningKey", previousSigningKey);
        signingKeyConfigured = false;
    }

    private sealed class Factory(string connectionString, string bootstrapServers) : WebApplicationFactory<API.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Kafka:BootstrapServers"] = bootstrapServers,
                ["Logging:LogLevel:Default"] = "Warning",
                ["Jwt:Issuer"] = "test-issuer", ["Jwt:Audience"] = "test-audience",
                ["Observability:EnableOtlp"] = "false", ["Database:ApplyMigrations"] = "false"
            }));
        }
    }
}
