using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextTableSnapshot
{
    public FileBaseContextTableSnapshot(IEntityType entityType, IReadOnlyList<object[]> rows)
    {
        EntityType = entityType;
        Rows = rows;
    }

    public virtual IEntityType EntityType { get; }

    public virtual IReadOnlyList<object[]> Rows { get; }
}