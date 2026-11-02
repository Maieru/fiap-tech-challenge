using FIAP.TechChallenge.OS.Domain.Interfaces;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;

public sealed class ProcessedMessageRepository(AppDbContext context) : IProcessedMessageRepository
{
    private static string Key(Guid id) => $"oficina-os-v1/{id:D}";
    public Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default) =>
        context.MensagensProcessadas.AnyAsync(x => x.Id == Key(messageId), cancellationToken);
    public async Task AddAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        context.MensagensProcessadas.Add(new ProcessedMessageEntity { Id = Key(messageId), ProcessedAtUtc = DateTime.UtcNow });
        await context.SaveChangesAsync(cancellationToken);
    }
}
