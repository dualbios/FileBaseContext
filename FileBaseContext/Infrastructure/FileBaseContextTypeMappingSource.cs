using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

namespace kDg.FileBaseContext.Infrastructure;

public class FileBaseContextTypeMappingSource : TypeMappingSource
{
    public FileBaseContextTypeMappingSource(TypeMappingSourceDependencies dependencies) : base(dependencies)
    {
    }

    protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
    {
        Type clrType = mappingInfo.ClrType;
        Debug.Assert(clrType != null);

        if (clrType.IsValueType
            || clrType == typeof(string))
        {
            return new FileBaseContextTypeMapping(clrType);
        }

        if (clrType == typeof(byte[]))
        {
            return new FileBaseContextTypeMapping(clrType, structuralComparer: new ArrayStructuralComparer<byte>());
        }

        if (clrType.FullName == "NetTopologySuite.Geometries.Geometry"
            || clrType.GetBaseTypes().Any(t => t.FullName == "NetTopologySuite.Geometries.Geometry"))
        {
            ValueComparer comparer = (ValueComparer)Activator.CreateInstance(typeof(GeometryValueComparer<>).MakeGenericType(clrType));

            return new FileBaseContextTypeMapping(
                clrType,
                comparer,
                comparer,
                comparer);
        }

        return base.FindMapping(mappingInfo);
    }
}