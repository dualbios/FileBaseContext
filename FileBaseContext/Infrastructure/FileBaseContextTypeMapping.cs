using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
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

    protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
    {
        return new FileBaseContextTypeMapping(parameters);
    }

    public override CoreTypeMapping WithComposedConverter(
        ValueConverter converter,
        ValueComparer comparer = null,
        ValueComparer keyComparer = null,
        CoreTypeMapping elementMapping = null,
        JsonValueReaderWriter jsonValueReaderWriter = null)
    {
        return new FileBaseContextTypeMapping(Parameters.WithComposedConverter(converter, comparer, keyComparer, elementMapping, jsonValueReaderWriter));
    }
}