using kDg.FileBaseContext.Storage;

namespace kDg.FileBaseContext;

public interface IFileBaseContextStoreCache
{
    IFileBaseContextStore GetStore(IFileBaseContextScopedOptions options);
}