using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextConventionSetBuilder : ProviderConventionSetBuilder
{
    public FileBaseContextConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies) : base(dependencies)
    {
    }

    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        conventionSet.Add(new RelationalTableAttributeConvention(Dependencies, null));

        return conventionSet;
    }
}