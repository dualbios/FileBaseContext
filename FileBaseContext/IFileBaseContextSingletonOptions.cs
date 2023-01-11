using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace kDg.FileBaseContext;

public interface IFileBaseContextSingletonOptions : ISingletonOptions
{
    FileBaseContextDatabaseRoot DatabaseRoot { get; }
    bool IsNullabilityCheckEnabled { get; }
}