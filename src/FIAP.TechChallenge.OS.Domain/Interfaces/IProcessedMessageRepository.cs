namespace FIAP.TechChallenge.OS.Domain.Interfaces;

public interface IProcessedMessageRepository
{
    Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task AddAsync(Guid messageId, CancellationToken cancellationToken = default);
}
