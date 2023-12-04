using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.Json.Nodes;

namespace kDg.FileBaseContext.Serializers;

public class JsonDataSerializer : ISerializer
{
    private readonly IEntityType _entityType;
    private readonly object _keyValueFactory;
    private string[] _propertyKeys;
    private Type[] _typeList;

    public JsonDataSerializer(IEntityType entityType, object keyValueFactory)
    {
        _entityType = entityType;
        _keyValueFactory = keyValueFactory;
        _propertyKeys = _entityType.GetProperties().Select(p => p.GetColumnName()).ToArray();
        _typeList = _entityType.GetProperties().Select(p => p.GetValueConverter()?.ProviderClrType ?? p.ClrType).ToArray();
    }

    public Dictionary<TKey, object[]> Deserialize<TKey>(string list, Dictionary<TKey, object[]> newList)
    {
        if (list != string.Empty)
        {
            JsonArray array = JsonNode.Parse(list)?.AsArray();

            if (array != null)
            {
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

                    newList.Add(key, value.ToArray());
                }
            }
        }

        return newList;
    }

    public string Serialize<TKey>(Dictionary<TKey, object[]> list)
    {
        JsonArray array = new JsonArray();

        foreach (KeyValuePair<TKey, object[]> val in list)
        {
            var jsonObject = new JsonObject();

            for (int i = 0; i < _propertyKeys.Length; i++)
            {
                var property = KeyValuePair.Create<string, JsonNode>(_propertyKeys[i], val.Value[i].Serialize());
                jsonObject.Add(property);
            }

            array.Add(jsonObject);
        }

        return array.ToString();
    }
}