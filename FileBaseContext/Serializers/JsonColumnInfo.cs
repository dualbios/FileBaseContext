using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Serializers
{
    internal sealed record JsonColumnInfo(
        string ColumnName,
        Type StoreType)
    {
        public static IEnumerable<JsonColumnInfo> FromEntityType(IEntityType entityType)
        {
            return entityType.GetProperties()
                .Select(p => new JsonColumnInfo(
                    ColumnName: p.GetColumnName(),
                    StoreType: p.GetValueConverter()?.ProviderClrType ?? p.ClrType));
        }
    }
}
