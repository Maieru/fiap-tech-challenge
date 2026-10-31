using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Infrastructure.Messaging;

// Access to messages awaiting publication. It does not change orders or business history.
public sealed class PostgresOutbox(AppDbContext db) : IOutboxStore
{
    public async Task<bool> AcquireLeaseAsync(string owner, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var until = now.AddSeconds(20);
        var affected = await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "DispatcherLeases" ("Id", "Owner", "UntilUtc") VALUES ('dispatcher', {owner}, {until})
            ON CONFLICT ("Id") DO UPDATE SET "Owner" = {owner}, "UntilUtc" = {until}
            WHERE "DispatcherLeases"."UntilUtc" < {now} OR "DispatcherLeases"."Owner" = {owner}
            """, ct);
        return affected == 1;
    }

    public async Task<PendingMessage?> NextAsync(CancellationToken ct) =>
        await db.Outbox.AsNoTracking().Where(x => !x.Published)
            .OrderBy(x => x.Sequence).ThenBy(x => x.CreatedAtUtc)
            .Select(x => new PendingMessage(x.Id, x.OrdemServicoId, x.Sequence, x.Body)).FirstOrDefaultAsync(ct);

    public async Task MarkPublishedAsync(Guid id, CancellationToken ct) =>
        await db.Outbox.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(p => p.Published, true), ct);

    public Task<long> PendingCountAsync(CancellationToken ct) => db.Outbox.LongCountAsync(x => !x.Published, ct);
}
