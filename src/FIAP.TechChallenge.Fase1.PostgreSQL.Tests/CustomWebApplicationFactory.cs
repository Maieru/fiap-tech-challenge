using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Testcontainers.PostgreSql;

namespace FIAP.TechChallenge.Fase1
{

[SetUpFixture]
public sealed class PostgreSqlSuite
{
    private static readonly PostgreSqlContainer Container = new PostgreSqlBuilder("postgres:18.3").Build();
    internal static string ConnectionString => Container.GetConnectionString();
    [OneTimeSetUp] public Task StartAsync() => Container.StartAsync();
    [OneTimeTearDown] public async Task StopAsync() => await Container.DisposeAsync();
}
}

namespace FIAP.TechChallenge.Fase1.API.Tests
{

// The HTTP scenarios are the same source files used by the InMemory suite.
// Only the database provider/lifecycle changes; authentication and business run unchanged.
public sealed class CustomWebApplicationFactory(bool configureDatabase = true) : WebApplicationFactory<Program>
{
    private readonly string _database = "legacy_" + Guid.NewGuid().ToString("N");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            if (configureDatabase)
            {
                var connectionString = new NpgsqlConnectionStringBuilder(PostgreSqlSuite.ConnectionString) { Database = _database }.ConnectionString;
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
            }
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (configureDatabase)
        {
            using var connection = new NpgsqlConnection(PostgreSqlSuite.ConnectionString);
            connection.Open();
            using var command = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", connection);
            command.ExecuteNonQuery();
        }
        var host = base.CreateHost(builder);
        if (configureDatabase)
        {
            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
        }
        return host;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (!configureDatabase) return;
        await using var connection = new NpgsqlConnection(PostgreSqlSuite.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }
}
}
