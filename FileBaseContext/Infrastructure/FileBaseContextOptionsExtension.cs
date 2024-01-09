using kDg.FileBaseContext.Extensions;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextOptionsExtension : IDbContextOptionsExtension
{
    private readonly Action<IServiceCollection> _applyServices;
    private readonly FileBaseContextScopedOptions _options = new();
    private FileBaseContextDatabaseRoot _databaseRoot;
    private bool _nullabilityCheckEnabled;

    public FileBaseContextOptionsExtension(string databaseName = null, string location = null, Action<IServiceCollection> applyServices = null)
    {
        _applyServices = applyServices;
        _options = new FileBaseContextScopedOptions(databaseName, location);
    }

    protected FileBaseContextOptionsExtension(FileBaseContextOptionsExtension copyFrom)
    {
        _databaseRoot = copyFrom._databaseRoot;
    }

    public FileBaseContextDatabaseRoot DatabaseRoot => _databaseRoot;

    public virtual bool IsNullabilityCheckEnabled => _nullabilityCheckEnabled;

    public FileBaseContextScopedOptions Options => _options;

    public void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkFileBaseContextDatabase();

        services.AddFileSystem();
        _applyServices?.Invoke(services);
    }

    public DbContextOptionsExtensionInfo Info => new FileBaseContextOptionsExtensionInfo(this);

    public void Validate(IDbContextOptions options)
    {
    }

    protected virtual FileBaseContextOptionsExtension Clone()
    {
        return new FileBaseContextOptionsExtension(this);
    }

    public virtual FileBaseContextOptionsExtension WithDatabaseRoot(FileBaseContextDatabaseRoot databaseRoot)
    {
        var clone = Clone();

        clone._databaseRoot = databaseRoot;

        return clone;
    }

    public virtual FileBaseContextOptionsExtension WithNullabilityCheckEnabled(bool nullabilityCheckEnabled)
    {
        var clone = Clone();

        clone._nullabilityCheckEnabled = nullabilityCheckEnabled;

        return clone;
    }

    public class FileBaseContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public FileBaseContextOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override bool IsDatabaseProvider { get; } = true;

        public override string LogFragment { get; } = nameof(FileBaseContextOptionsExtensionInfo);

        private new FileBaseContextOptionsExtension Extension => (FileBaseContextOptionsExtension)base.Extension;

        public override int GetServiceProviderHashCode()
        {
            return GetHashCode();
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["FileBaseContextOptionsExtensionInfo:DebugInfo"] = GetServiceProviderHashCode().ToString();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is FileBaseContextOptionsExtensionInfo otherInfo
                   && Extension._databaseRoot == otherInfo.Extension._databaseRoot
                   && Extension._nullabilityCheckEnabled == otherInfo.Extension._nullabilityCheckEnabled;
        }
    }
}