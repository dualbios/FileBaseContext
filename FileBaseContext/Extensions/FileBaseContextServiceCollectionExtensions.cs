using kDg.FileBaseContext.Infrastructure;
using kDg.FileBaseContext.Infrastructure.Query;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace kDg.FileBaseContext.Extensions;

public static class FileBaseContextServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkFileBaseContextDatabase(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, FileBaseContextLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<FileBaseContextOptionsExtension>>()
            .TryAdd<IDatabase>(p => p.GetService<IFileBaseContextDatabase>())
            .TryAdd<IDbContextTransactionManager, FileBaseContextTransactionManager>()
            .TryAdd<IDatabaseCreator, FileBaseContextDatabaseCreator>()
            .TryAdd<IQueryContextFactory, FileBaseContextQueryContextFactory>()
            .TryAdd<IProviderConventionSetBuilder, FileBaseContextConventionSetBuilder>()
            .TryAdd<ITypeMappingSource, FileBaseContextTypeMappingSource>()

            //// New Query pipeline
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, FileBaseContextShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, FileBaseContextQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, FileBaseContextQueryTranslationPostprocessorFactory>()

            .TryAddProviderSpecificServices(
                b => b
                    .TryAddSingleton<IFileBaseContextFileManager, FileBaseContextFileManager>()
                    .TryAddSingleton<IFileBaseContextSingletonOptions, FileBaseContextSingletonOptions>()
                    .TryAddSingleton<IFileBaseContextStoreCache, FileBaseContextStoreCache>()
                    .TryAddScoped<IFileBaseContextDatabase, FileBaseContextDatabase>()
            );

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}