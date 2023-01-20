using kDg.FileBaseContext.Extensions;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextOptionsExtension : IDbContextOptionsExtension
{
    private readonly FileBaseContextScopedOptions _options = new();
    private bool _nullabilityCheckEnabled;
    private FileBaseContextDatabaseRoot _databaseRoot;

    public FileBaseContextOptionsExtension(string databaseName = null, string location = null)
    {
        _options = new(databaseName, location);
    }

    protected FileBaseContextOptionsExtension(FileBaseContextOptionsExtension copyFrom)
    {
        _databaseRoot = copyFrom._databaseRoot;
    }

    public FileBaseContextDatabaseRoot DatabaseRoot => _databaseRoot;

    public DbContextOptionsExtensionInfo Info => new FileBaseContextOptionsExtensionInfo(this);

    public virtual bool IsNullabilityCheckEnabled => _nullabilityCheckEnabled;

    public FileBaseContextScopedOptions Options => _options;

    protected virtual FileBaseContextOptionsExtension Clone() => new(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkFileBaseContextDatabase();
    }

    public void Validate(IDbContextOptions options)
    {
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
            return this.GetHashCode();
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["FileBaseContextOptionsExtensionInfo:DebugInfo"] = GetServiceProviderHashCode().ToString();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is FileBaseContextOptionsExtensionInfo otherInfo
               && Extension._databaseRoot == otherInfo.Extension._databaseRoot
               && Extension._nullabilityCheckEnabled == otherInfo.Extension._nullabilityCheckEnabled;
    }
}