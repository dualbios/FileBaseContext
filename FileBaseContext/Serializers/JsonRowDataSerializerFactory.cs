﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Serializers;

internal class JsonRowDataSerializerFactory : IRowDataSerializerFactory
{
    public IRowDataSerializer Create<TKey>(IEntityType entityType, 
                                           IPrincipalKeyValueFactory<TKey> keyValueFactory,
                                           IServiceProvider serviceProvider)
    {
        return new JsonRowDataSerializer(entityType, keyValueFactory, serviceProvider);
    }
}