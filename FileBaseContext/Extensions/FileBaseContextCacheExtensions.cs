using kDg.FileBaseContext.Infrastructure;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace kDg.FileBaseContext.Extensions;

public static class FileBaseContextCacheExtensions
{
    public static IFileBaseContextStore GetStore(this IFileBaseContextStoreCache storeCache, IDbContextOptions options)
    {
        return storeCache.GetStore(options.Extensions.OfType<FileBaseContextOptionsExtension>().First().Options);
    }
}