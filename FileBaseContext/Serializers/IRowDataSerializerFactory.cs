using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Serializers;

public interface IRowDataSerializerFactory
{
    public IRowDataSerializer Create<TKey>(IEntityType entityType, IPrincipalKeyValueFactory<TKey> keyValueFactory);
}