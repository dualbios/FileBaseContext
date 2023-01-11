using kDg.FileBaseContext.Extensions;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextOptionsExtension : IDbContextOptionsExtension
{
    private readonly FileBaseContextScopedOptions _options = new();

    public FileBaseContextOptionsExtension(string databaseName = null, string location = null)
    {
        _options = new(databaseName, location);
    }

    public FileBaseContextDatabaseRoot DatabaseRoot { get; set; }

    public DbContextOptionsExtensionInfo Info => new FileBaseContextOptionsExtensionInfo(this);

    public virtual bool IsNullabilityCheckEnabled => false;

    public FileBaseContextScopedOptions Options => _options;

    public void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkFileBaseContextDatabase();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    public class FileBaseContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public FileBaseContextOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override bool IsDatabaseProvider { get; } = true;

        public override string LogFragment { get; } = nameof(FileBaseContextOptionsExtensionInfo);

        public override int GetServiceProviderHashCode()
        {
            return this.GetHashCode();
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["FileBaseContextOptionsExtensionInfo:DebugInfo"] = GetServiceProviderHashCode().ToString();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return false;
        }
    }
}