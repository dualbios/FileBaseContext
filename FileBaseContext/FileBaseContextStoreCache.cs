using kDg.FileBaseContext.Storage;
using System.Collections.Concurrent;

namespace kDg.FileBaseContext;

public class FileBaseContextStoreCache : IFileBaseContextStoreCache
{
    private readonly ConcurrentDictionary<IFileBaseContextScopedOptions, IFileBaseContextStore> _namedStores;
    private readonly IServiceProvider _serviceProvider;

    private readonly bool _useNameMatching;

    public FileBaseContextStoreCache(
        IServiceProvider serviceProvider,
        IFileBaseContextSingletonOptions options)
    {
        _serviceProvider = serviceProvider;

        if (options?.DatabaseRoot != null)
        {
            _useNameMatching = true;

            LazyInitializer.EnsureInitialized(
                ref options.DatabaseRoot.Instance,
                () => new ConcurrentDictionary<IFileBaseContextScopedOptions, IFileBaseContextStore>());

            _namedStores = (ConcurrentDictionary<IFileBaseContextScopedOptions, IFileBaseContextStore>)options.DatabaseRoot.Instance;
        }
        else
        {
            _namedStores = new ConcurrentDictionary<IFileBaseContextScopedOptions, IFileBaseContextStore>();
        }
    }

    public virtual IFileBaseContextStore GetStore(IFileBaseContextScopedOptions options)
    {
        return _namedStores.GetOrAdd(options, _ => new FileBaseContextStore(new FileBaseContextTableFactory(_serviceProvider, options), _useNameMatching));
    }
}