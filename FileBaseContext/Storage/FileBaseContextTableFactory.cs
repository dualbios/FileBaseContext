using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.Reflection;

namespace kDg.FileBaseContext.Storage;

public class FileBaseContextTableFactory : IFileBaseContextTableFactory
{
    private readonly ConcurrentDictionary<(IEntityType EntityType, IFileBaseContextTable BaseTable), Func<IFileBaseContextTable>> _factories = new();
    private readonly IFileBaseContextScopedOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public FileBaseContextTableFactory(IServiceProvider serviceProvider, IFileBaseContextScopedOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public virtual IFileBaseContextTable Create(IEntityType entityType, IFileBaseContextTable baseTable)
        => _factories.GetOrAdd((entityType, baseTable), e => CreateTable(e.EntityType, e.BaseTable))();

    private static Func<IFileBaseContextTable> CreateFactory<TKey>(
        IEntityType entityType,
        IServiceProvider serviceProvider,
        IFileBaseContextScopedOptions options)
        where TKey : notnull
    {
        return () => new FileBaseContextTable<TKey>(entityType, serviceProvider, options);
    }

    private Func<IFileBaseContextTable> CreateTable(IEntityType entityType, IFileBaseContextTable baseTable)
    {
        return (Func<IFileBaseContextTable>)typeof(FileBaseContextTableFactory).GetTypeInfo()
            .GetDeclaredMethod(nameof(CreateFactory))!
            .MakeGenericMethod(entityType.FindPrimaryKey()!.GetKeyType())
            .Invoke(null, new object[] { entityType, _serviceProvider, _options })!;
    }
}