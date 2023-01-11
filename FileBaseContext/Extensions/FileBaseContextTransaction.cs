using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Extensions;

public class FileBaseContextTransaction : IDbContextTransaction
{
    public virtual Guid TransactionId { get; } = Guid.NewGuid();

    public virtual void Commit()
    {
    }

    public virtual Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
    }

    public virtual ValueTask DisposeAsync()
    {
        return default;
    }

    public virtual void Rollback()
    {
    }

    public virtual Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}