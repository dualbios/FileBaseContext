using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace kDg.FileBaseContext.Serializers;

public static class RowDataSerializerExtentions
{
    public static IServiceCollection AddJsonRowDataSerializer(this IServiceCollection services)
    {
        services.RemoveAll<IRowDataSerializerFactory>();
        services.AddSingleton<IRowDataSerializerFactory, JsonRowDataSerializerFactory>();
        return services;
    }

    public static IServiceCollection AddCsvRowDataSerializer(this IServiceCollection services)
    {
        services.RemoveAll<IRowDataSerializerFactory>();
        services.AddSingleton<IRowDataSerializerFactory, CsvRowDataSerializerFactory>();
        return services;
    }
}