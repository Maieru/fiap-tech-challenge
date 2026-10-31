using System.Text.Json;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Testcontainers.PostgreSql;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[TestFixture, NonParallelizable]
public sealed class ExecutionMigrationTests
{
    [Test]
    public async Task Upgrade_ShouldPreserveExistingRecordsAndBackfillAnOpeningOnlyOnce()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18.3").Build(); await postgres.StartAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(postgres.GetConnectionString()).Options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261031132649_InitialOs");
        var customer = Guid.NewGuid(); var vehicle = Guid.NewGuid(); var order = Guid.NewGuid(); var history = Guid.NewGuid();
        var date = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Clientes" ("Id", "Nome", "Cpf", "Telefone", "Ativo") VALUES ({customer}, 'Cliente anterior', '52998224725', '11987654321', true);
            INSERT INTO "Veiculos" ("Id", "ClienteId", "Placa", "Marca", "Modelo", "Ano", "Ativo") VALUES ({vehicle}, {customer}, 'ABC1234', 'Toyota', 'Corolla', 2024, true);
            INSERT INTO "OrdensServico" ("Id", "ClienteId", "VeiculoId", "DescricaoProblema", "Status", "DataCriacao") VALUES ({order}, {customer}, {vehicle}, 'Motor com falha', 1, {date});
            INSERT INTO "HistoricoOrdens" ("Id", "OrdemServicoId", "Tipo", "OcorridoEmUtc") VALUES ({history}, {order}, 'OrdemCriada', {date});
            """);
        await migrator.MigrateAsync(); await db.Database.ExecuteSqlRawAsync(OutboxBackfill.Sql);
        var row = await db.OrdensServico.SingleAsync(); var original = await db.HistoricoOrdens.SingleAsync();
        var message = JsonSerializer.Deserialize<MessageEnvelope<FlowPayload>>((await db.Outbox.SingleAsync()).Body, IntegrationContext.Json)!;
        Assert.Multiple(() =>
        {
            Assert.That(row.Id, Is.EqualTo(order)); Assert.That(row.ClienteId, Is.EqualTo(customer)); Assert.That(row.VeiculoId, Is.EqualTo(vehicle));
            Assert.That(row.DataCriacao, Is.EqualTo(date)); Assert.That(row.InputSequence, Is.Zero); Assert.That(row.OutputSequence, Is.EqualTo(1));
            Assert.That(original.Id, Is.EqualTo(history)); Assert.That(message.Payload.OrdemServicoId, Is.EqualTo(order));
            Assert.That(message.Payload.Sequence, Is.EqualTo(1)); Assert.That(message.OccurredAtUtc.UtcDateTime, Is.EqualTo(date));
        });
        Assert.That(await db.Clientes.CountAsync(), Is.EqualTo(1)); Assert.That(await db.Veiculos.CountAsync(), Is.EqualTo(1));
    }
}
