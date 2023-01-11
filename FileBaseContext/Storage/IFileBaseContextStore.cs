using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace kDg.FileBaseContext.Storage;

public interface IFileBaseContextStore
{
    bool Clear();

    bool EnsureCreated(IUpdateAdapterFactory updateAdapterFactory, IModel designModel);

    int ExecuteTransaction(IList<IUpdateEntry> entries);

    FileBaseContextIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(IProperty property);

    IReadOnlyList<FileBaseContextTableSnapshot> GetTables(IEntityType entityType);
}