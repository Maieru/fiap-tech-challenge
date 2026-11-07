using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[NonParallelizable]
public sealed class MigrationCliTests
{
    [Test]
    public async Task MigrationCommandAppliesSchemaAndPreservesExistingDataWhenRepeated()
    {
        await using var database = new PostgreSqlBuilder("postgres:18.3")
            .WithDatabase("migration_cli_" + Guid.NewGuid().ToString("N")).Build();
        await database.StartAsync();
        var connection = database.GetConnectionString();
        await RunCliAsync(["--ConnectionStrings:DefaultConnection=" + connection])
            .WaitAsync(TimeSpan.FromSeconds(30));
        await using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection).Options);
        var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
        var user = new UsuarioEntity { Id = Guid.NewGuid(), Login = "migration-marker", Senha = "test-hash" };
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await RunCliAsync(["--ConnectionStrings:DefaultConnection=" + connection])
            .WaitAsync(TimeSpan.FromSeconds(30));

        var appliedAgain = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
        var pending = (await context.Database.GetPendingMigrationsAsync()).ToArray();
        var preserved = await context.Usuarios.SingleAsync(x => x.Id == user.Id);
        Assert.Multiple(() =>
        {
            Assert.That(applied, Is.Not.Empty);
            Assert.That(appliedAgain, Is.EqualTo(applied));
            Assert.That(pending, Is.Empty);
            Assert.That(preserved.Login, Is.EqualTo("migration-marker"));
        });
    }

    [Test]
    public async Task MigrationCommandRequiresConnectionWithoutStartingAuthenticationOrConsumers()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), "os-migration-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(contentRoot);
        var previousConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        try
        {
            InvalidOperationException? failure = null;
            try { await RunCliAsync(["--contentRoot=" + contentRoot]).WaitAsync(TimeSpan.FromSeconds(30)); }
            catch (InvalidOperationException exception) { failure = exception; }

            Assert.That(failure?.Message, Is.EqualTo("Configure ConnectionStrings:DefaultConnection."));
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", previousConnection);
            Directory.Delete(contentRoot);
        }
    }

    private static async Task RunCliAsync(string[] settings)
    {
        var previousSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
        try { await API.Program.Main([.. settings, "--Kafka:BootstrapServers=", "--migrate-only"]); }
        finally { Environment.SetEnvironmentVariable("Jwt__SigningKey", previousSigningKey); }
    }
}
