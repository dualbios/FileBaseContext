using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure;

internal class FileBaseContextQueryContext : QueryContext
{
    public FileBaseContextQueryContext(QueryContextDependencies dependencies, IFileBaseContextStore store)
        : base(dependencies)
    {
        Store = store;
    }

    public IFileBaseContextStore Store { get; }
}