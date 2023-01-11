using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Storage;

public interface IFileBaseContextTableFactory
{
    IFileBaseContextTable Create(IEntityType entityType, IFileBaseContextTable baseTable);
}