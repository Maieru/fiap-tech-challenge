using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[NonParallelizable]
public sealed class DesignTimeFactoryTests
{
    private const string ConnectionVariable = "ConnectionStrings__DefaultConnection";
    private string? originalConnection;

    [SetUp]
    public void SaveEnvironment() => originalConnection = Environment.GetEnvironmentVariable(ConnectionVariable);

    [TearDown]
    public void RestoreEnvironment() => Environment.SetEnvironmentVariable(ConnectionVariable, originalConnection);

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void CreateDbContext_ShouldRejectMissingConnection_WhenEnvironmentIsNotConfigured(string? connection)
    {
        Environment.SetEnvironmentVariable(ConnectionVariable, connection);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new DesignTimeFactory().CreateDbContext([]));

        Assert.That(exception!.Message, Does.Contain(ConnectionVariable));
    }

    [Test]
    public void CreateDbContext_ShouldUseConfiguredConnection_WithoutOpeningDatabase()
    {
        const string connection = "Host=localhost;Database=design_time_test;Username=design_time_test";
        Environment.SetEnvironmentVariable(ConnectionVariable, connection);

        using var context = new DesignTimeFactory().CreateDbContext([]);

        Assert.Multiple(() =>
        {
            Assert.That(context.Database.ProviderName, Is.EqualTo("Npgsql.EntityFrameworkCore.PostgreSQL"));
            Assert.That(context.Database.GetConnectionString(), Is.EqualTo(connection));
            Assert.That(context.Database.GetDbConnection().State, Is.EqualTo(System.Data.ConnectionState.Closed));
            Assert.That(context.Model.FindEntityType(typeof(FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities.OrdemServicoEntity)), Is.Not.Null);
        });
    }
}
