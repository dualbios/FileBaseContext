using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace kDg.FileBaseContext.Extensions;

public static class FileBaseContextDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseFileBaseContextDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        string databaseName = "",
        string location = null,
        Action<IServiceCollection> applyServices = null)
    {
        var extension = optionsBuilder.Options.FindExtension<FileBaseContextOptionsExtension>() ?? new FileBaseContextOptionsExtension(databaseName, location, applyServices);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}