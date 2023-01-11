using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace kDg.FileBaseContext.Storage;

public interface IFileBaseContextTable
{
    IEnumerable<object[]> Rows { get; }

    void Create(IUpdateEntry entry);

    void Save();

    IReadOnlyList<object[]> SnapshotRows();

    void Delete(IUpdateEntry entry);

    void Update(IUpdateEntry entry);

    FileBaseContextIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(
        IProperty property,
        IReadOnlyList<IFileBaseContextTable> tables);
}