using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace kDg.FileBaseContext.Serializers;

public static class SerializerHelper
{
    private static readonly ConcurrentDictionary<Type, ArraySerializer> s_arraySerializers = [];

    public static object Deserialize(this string input, Type type)
    {
        if (string.IsNullOrEmpty(input))
        {
            return type.GetDefaultValue();
        }

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(input, CultureInfo.InvariantCulture);
        }

        if (type == typeof(TimeSpan))
        {
            return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
        }

        if (type == typeof(Guid))
        {
            return Guid.Parse(input);
        }

        if (type.IsArray)
        {
            Type elementType = type.GetElementType();
            var arraySerializer = GetOrCreateArraySerializer(elementType);
            return arraySerializer.Deserialize(input);
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, input);
        }

        return Convert.ChangeType(input, type, CultureInfo.InvariantCulture);
    }

    public static string Serialize(this object input)
    {
        if (input != null)
        {
            var inputType = input.GetType();

            if (inputType.IsArray)
            {
                Type elementType = inputType.GetElementType();
                var arraySerializer = GetOrCreateArraySerializer(elementType);
                return arraySerializer.Serialize(input);
            }

            return input is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : input.ToString();
        }

        return "";
    }

    public static TKey GetKey<TKey>(object keyValueFactoryObject, IEntityType entityType, Func<string, string> valueSelector)
    {
        IPrincipalKeyValueFactory<TKey> keyValueFactory = (IPrincipalKeyValueFactory<TKey>)keyValueFactoryObject;

        return (TKey)keyValueFactory.CreateFromKeyValues(
            entityType.FindPrimaryKey().Properties
                      .Select(p => valueSelector(p.GetColumnName())
                                  .Deserialize(p.GetValueConverter()?.ProviderClrType ?? p.ClrType)).ToArray());
    }

    private static ArraySerializer GetOrCreateArraySerializer(Type elementType)
    {
        return s_arraySerializers.GetOrAdd(elementType, CreateArraySerializer);

        static ArraySerializer CreateArraySerializer(Type elementType)
        {
            var closedType = typeof(ArraySerializer<>).MakeGenericType(elementType);
            return (ArraySerializer)Activator.CreateInstance(closedType);
        }
    }

    private abstract class ArraySerializer
    {
        public abstract object Deserialize(string value);

        public abstract string Serialize(object value);
    }

    /// <summary>
    /// Arrays of value types are not covariant
    /// so need to be handled with a generic handler per type.
    /// Arrays of reference types _are_ covariant and could be handled by an <c>object[]</c> handler,
    /// but the call to <see cref="SerializerHelper.Deserialize(string, Type)"/>
    /// still requires a <see cref="Type"/> variable,
    /// and so using this wrapper for reference types as well is a convenient factoring.
    /// </summary>
    /// <typeparam name="T">The element type of the array to serialize.</typeparam>
    private sealed class ArraySerializer<T>
        : ArraySerializer
    {
        public override object Deserialize(string value)
        {
            var result = new List<T>();

            foreach (var item in value.Split(','))
                result.Add((T)item.Deserialize(typeof(T)));

            return result.ToArray();
        }

        public override string Serialize(object value)
        {
            var array = (T[])value;
            if (array.Length == 0)
                return string.Empty;

            var resultBuilder = new StringBuilder();
            var separator = string.Empty;

            foreach (var item in array)
            {
                resultBuilder.Append(separator);
                separator = ",";

                resultBuilder.Append(item.Serialize());
            }

            return resultBuilder.ToString();
        }
    }
}