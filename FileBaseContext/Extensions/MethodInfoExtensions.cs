using System.Collections;
using System.Collections.Immutable;

namespace System.Reflection;

internal static class MethodInfoExtensions
{
    public static bool IsContainsMethod(this MethodInfo method)
        => method.Name == nameof(IList.Contains)
           && method.DeclaringType != null
           && method.DeclaringType.GetInterfaces().Append(method.DeclaringType).Any(
               t => t == typeof(IList)
                    || (t.IsGenericType
                        && t.GetGenericTypeDefinition() is Type genericType
                        && (genericType == typeof(ICollection<>)
                            || genericType == typeof(IReadOnlySet<>)
                            || genericType == typeof(IImmutableSet<>))));
}