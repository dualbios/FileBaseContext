using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace kDg.FileBaseContext.Extensions;

public static class FileBaseContextDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseFileBaseContextDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        string databaseName = "",
        string location = null)
    {
        var extension = optionsBuilder.Options.FindExtension<FileBaseContextOptionsExtension>() ?? new FileBaseContextOptionsExtension(databaseName, location);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}