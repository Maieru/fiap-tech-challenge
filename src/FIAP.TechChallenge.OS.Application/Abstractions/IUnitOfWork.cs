namespace FIAP.TechChallenge.OS.Application.Abstractions;

public interface IUnitOfWork
{
    Task<ITransaction> BeginAsync(CancellationToken cancellationToken = default);
}
