namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;

public sealed class ProcessedMessageEntity
{
    public string Id { get; set; } = "";
    public DateTime ProcessedAtUtc { get; set; }
}
