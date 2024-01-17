using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace kDg.FileBaseContext.Serializers
{
    internal sealed class JsonRowDataConverter(
        IEnumerable<JsonColumnInfo> columns)
        : JsonConverter<JsonRowData>
    {
        private readonly string[] _columnNames = columns
            .Select(t => t.ColumnName)
            .ToArray();

        private readonly Type[] _storeTypes = columns
            .Select(t => t.StoreType)
            .ToArray();

        public override JsonRowData Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var columnValues = new object[_columnNames.Length];
            for (int i = 0; i < columnValues.Length; i++)
                columnValues[i] = _storeTypes[i].GetDefaultValue();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        var propertyName = reader.GetString();

                        if (!reader.Read())
                            throw new JsonException();

                        var propertyIndex = Array.IndexOf(_columnNames, propertyName);
                        if (propertyIndex < 0)
                        {
                            reader.Skip();
                            continue;
                        }

                        var propertyType = _storeTypes[propertyIndex];
                        object propertyValue;
                        try
                        {
                            propertyValue = JsonSerializer.Deserialize(ref reader, propertyType, options);
                        }
                        catch (JsonException)
                        when (reader.TokenType == JsonTokenType.String)
                        {
                            // Allow deserializing values serialized as strings by a previous release
                            propertyValue = BackwardsCompatibleGetValue(reader.GetString(), propertyType);
                        }
                        catch (JsonException)
                        when (propertyType == typeof(byte[]) && reader.TokenType == JsonTokenType.StartArray)
                        {
                            // We never emit these, but allow deserializing a numeric array as a byte array for completeness
                            propertyValue = ReadByteArrayFromArray(ref reader);
                        }

                        columnValues[propertyIndex] = propertyValue;
                        break;

                    case JsonTokenType.EndObject:
                        return new(columnValues);
                }
            }

            throw new JsonException();
        }

        private static object BackwardsCompatibleGetValue(string input, Type type)
        {
            if (string.IsNullOrEmpty(input))
            {
                // NOTE: Behavior change in handling of null/empty string-type values
                // to make values round trip correctly.
                return type == typeof(string) ? input : type.GetDefaultValue();
            }

            type = type.UnwrapNullableType();

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
                var elementType = type.GetElementType();
                if (elementType == typeof(byte))
                {
                    return input.Split(',').Select(byte.Parse).ToArray();
                }
                else
                {
                    throw new InvalidOperationException($"CSV array fallback is only implemented for byte[] type.");
                }
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, input);
            }

            return Convert.ChangeType(input, type, CultureInfo.InvariantCulture);
        }

        private static object ReadByteArrayFromArray(ref Utf8JsonReader reader)
        {
            var intValues = JsonSerializer.Deserialize<List<int>>(ref reader);

            var result = new byte[intValues.Count];
            for (int i = 0; i < intValues.Count; i++)
                result[i] = checked((byte)intValues[i]);

            return result;
        }

        public override void Write(
            Utf8JsonWriter writer,
            JsonRowData value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var columnValues = value.ColumnValues;
            for (int i = 0; i < _columnNames.Length; i++)
            {
                writer.WritePropertyName(_columnNames[i]);
                JsonSerializer.Serialize(writer, columnValues[i], options);
            }

            writer.WriteEndObject();
        }
    }
}
