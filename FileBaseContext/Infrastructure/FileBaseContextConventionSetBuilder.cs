using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextConventionSetBuilder : ProviderConventionSetBuilder
{
    public FileBaseContextConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies) : base(dependencies)
    {
    }
}