using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace kDg.FileBaseContext;

public class FileBaseContextSingletonOptions : IFileBaseContextSingletonOptions
{
    public virtual FileBaseContextDatabaseRoot DatabaseRoot { get; private set; }

    public virtual bool IsNullabilityCheckEnabled { get; private set; }

    public virtual void Initialize(IDbContextOptions options)
    {
        var inMemoryOptions = options.FindExtension<FileBaseContextOptionsExtension>();

        if (inMemoryOptions != null)
        {
            DatabaseRoot = inMemoryOptions.DatabaseRoot;
            IsNullabilityCheckEnabled = inMemoryOptions.IsNullabilityCheckEnabled;
        }
    }

    public virtual void Validate(IDbContextOptions options)
    {
    }
}