using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Serializers;

public class CsvRowDataSerializerFactory : IRowDataSerializerFactory
{
    public IRowDataSerializer Create<TKey>(IEntityType entityType, IPrincipalKeyValueFactory<TKey> keyValueFactory)
    {
        return new CsvRowDataSerializer(entityType, keyValueFactory);
    }
}