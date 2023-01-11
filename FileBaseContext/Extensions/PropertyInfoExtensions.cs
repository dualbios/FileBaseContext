using System.Diagnostics;

namespace System.Reflection;

[DebuggerStepThrough]
internal static class PropertyInfoExtensions
{
    public static bool IsStatic(this PropertyInfo property)
    {
        return (property.GetMethod ?? property.SetMethod).IsStatic;
    }

    public static bool IsCandidateProperty(this PropertyInfo propertyInfo, bool needsWrite = true, bool publicOnly = true)
    {
        return !propertyInfo.IsStatic()
               && propertyInfo.CanRead
               && (!needsWrite || propertyInfo.FindSetterProperty() != null)
               && propertyInfo.GetMethod != null && (!publicOnly || propertyInfo.GetMethod.IsPublic)
               && propertyInfo.GetIndexParameters().Length == 0;
    }

    public static bool IsEFIndexerProperty(this PropertyInfo propertyInfo)
    {
        if (propertyInfo.PropertyType == typeof(object))
        {
            var indexParams = propertyInfo.GetIndexParameters();
            if (indexParams.Length == 1
                && indexParams[0].ParameterType == typeof(string))
            {
                return true;
            }
        }

        return false;
    }

    public static PropertyInfo FindGetterProperty(this PropertyInfo propertyInfo)
    {
        return propertyInfo.DeclaringType
            .GetPropertiesInHierarchy(propertyInfo.GetSimpleMemberName())
            .FirstOrDefault(p => p.GetMethod != null);
    }

    public static PropertyInfo FindSetterProperty(this PropertyInfo propertyInfo)
    {
        return propertyInfo.DeclaringType
            .GetPropertiesInHierarchy(propertyInfo.GetSimpleMemberName())
            .FirstOrDefault(p => p.SetMethod != null);
    }
}