using FIAP.TechChallenge.OS.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence;

public sealed class PostgresUnitOfWork(AppDbContext context, ILogger<PostgresUnitOfWork> logger) : IUnitOfWork
{
    public async Task<ITransaction> BeginAsync(CancellationToken cancellationToken = default) =>
        new Transaction(await context.Database.BeginTransactionAsync(cancellationToken), logger);

    private sealed class Transaction(IDbContextTransaction transaction, ILogger logger) : ITransaction
    {
        private bool committed;
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try { await transaction.CommitAsync(cancellationToken); committed = true; }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Falha no commit PostgreSQL. Verifique o estado confirmado e as mensagens entregues antes de repetir.");
                throw;
            }
        }
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!committed)
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await transaction.RollbackAsync(timeout.Token);
                }
            }
            catch (Exception exception) { logger.LogCritical(exception, "Rollback PostgreSQL não confirmado. Reconciliação necessária."); }
            finally { await transaction.DisposeAsync(); }
        }
    }
}
