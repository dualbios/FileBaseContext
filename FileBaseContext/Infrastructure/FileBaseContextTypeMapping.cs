using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextTypeMapping : CoreTypeMapping
{
    public FileBaseContextTypeMapping(
        Type clrType,
        ValueComparer comparer = null,
        ValueComparer keyComparer = null,
        ValueComparer structuralComparer = null)
        : base(
            new CoreTypeMappingParameters(
                clrType,
                converter: null,
                comparer,
                keyComparer,
                structuralComparer,
                valueGeneratorFactory: null))
    {
    }

    private FileBaseContextTypeMapping(CoreTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    public override CoreTypeMapping Clone(ValueConverter converter)
    {
        return new FileBaseContextTypeMapping(Parameters.WithComposedConverter(converter));
    }
}