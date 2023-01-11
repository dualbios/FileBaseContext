using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Extensions;

public class FileBaseContextTransactionManager : IDbContextTransactionManager
{
    private static readonly FileBaseContextTransaction _stubTransaction = new FileBaseContextTransaction();

    public IDbContextTransaction CurrentTransaction { get; } = null;

    public IDbContextTransaction BeginTransaction()
    {
        return _stubTransaction;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult<IDbContextTransaction>(_stubTransaction);
    }

    public void CommitTransaction()
    {
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public void ResetState()
    {
    }

    public Task ResetStateAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public void RollbackTransaction()
    {
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }
}