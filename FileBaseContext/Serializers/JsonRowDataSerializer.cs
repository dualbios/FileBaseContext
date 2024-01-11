using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace kDg.FileBaseContext.Serializers;

public class JsonRowDataSerializer : IRowDataSerializer
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
    };

    private readonly IEntityType _entityType;
    private readonly object _keyValueFactory;
    private string[] _propertyKeys;
    private Type[] _typeList;

    public JsonRowDataSerializer(IEntityType entityType, object keyValueFactory)
    {
        _entityType = entityType;
        _keyValueFactory = keyValueFactory;
        _propertyKeys = _entityType.GetProperties().Select(p => p.GetColumnName()).ToArray();
        _typeList = _entityType.GetProperties().Select(p => p.GetValueConverter()?.ProviderClrType ?? p.ClrType).ToArray();
    }

    public string FileExtension => ".json";

    public void Deserialize<TKey>(Stream stream, Dictionary<TKey, object[]> result)
    {
        Debug.Assert(stream.Position == 0);
        if (stream.Length == 0)
        {
            return;
        }

        var array = JsonSerializer.Deserialize<JsonArray>(stream, _jsonOptions);
        if (array == null)
        {
            return;
        }

        foreach (JsonNode node in array)
        {
            List<object> value = new();

            for (int i = 0; i < _propertyKeys.Length; i++)
            {
                JsonNode singleNode = node[_propertyKeys[i]];
                object val = null;
                if (singleNode != null)
                {
                    val = singleNode.GetValue<string>().Deserialize(_typeList[i]);
                }

                value.Add(val);
            }

            TKey key = SerializerHelper.GetKey<TKey>(_keyValueFactory, _entityType, propertyName => node[propertyName].GetValue<string>());

            result.Add(key, value.ToArray());
        }
    }

    public void Serialize<TKey>(Stream stream, IReadOnlyDictionary<TKey, object[]> source)
    {
        JsonArray array = new JsonArray();

        foreach (KeyValuePair<TKey, object[]> val in source)
        {
            var jsonObject = new JsonObject();

            for (int i = 0; i < _propertyKeys.Length; i++)
            {
                var property = KeyValuePair.Create<string, JsonNode>(_propertyKeys[i], val.Value[i].Serialize());
                jsonObject.Add(property);
            }

            array.Add(jsonObject);
        }

        Debug.Assert(stream.Length == 0);
        JsonSerializer.Serialize(stream, array, _jsonOptions);
    }
}