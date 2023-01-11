using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextDatabaseCreator : IDatabaseCreator
{
    private readonly IDatabase _database;

    public FileBaseContextDatabaseCreator(IDatabase database)
    {
        _database = database;
    }

    protected virtual FileBaseContextDatabase Database => (FileBaseContextDatabase)_database;

    public bool CanConnect()
    {
        return true;
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(true);
    }

    public bool EnsureCreated()
    {
        return true;
    }

    public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(true);
    }

    public bool EnsureDeleted()
    {
        return true;
    }

    public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(true);
    }
}