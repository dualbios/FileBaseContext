using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics;
using System.Text.Json;

namespace kDg.FileBaseContext.Serializers;

public class JsonRowDataSerializer : IRowDataSerializer
{
    private readonly IEntityType _entityType;
    private readonly object _keyValueFactory;
    private readonly int[] _keyColumns;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonRowDataSerializer(IEntityType entityType, object keyValueFactory)
    {
        _entityType = entityType;
        _keyValueFactory = keyValueFactory;
        _keyColumns = CreateKeyColumnsLookup(entityType);
        _jsonOptions = CreateJsonOptions(entityType);

        static int[] CreateKeyColumnsLookup(IEntityType entityType)
        {
            var columnNames = entityType.GetProperties()
                .Select(p => p.GetColumnName())
                .ToArray();

            return entityType.FindPrimaryKey().Properties
                .Select(p => Array.IndexOf(columnNames, p.GetColumnName()))
                .ToArray();
        }
    }

    public string FileExtension => ".json";

    public void Deserialize<TKey>(Stream stream, Dictionary<TKey, object[]> result)
    {
        Debug.Assert(stream.Position == 0);
        if (stream.Length == 0)
        {
            return;
        }

        var rowsData = JsonSerializer.Deserialize<List<JsonRowData>>(stream, _jsonOptions);
        if (rowsData == null)
        {
            return;
        }

        var keyValueFactory = (IPrincipalKeyValueFactory<TKey>)_keyValueFactory;
        var keyValues = new object[_keyColumns.Length];
        foreach (var rowData in rowsData)
        {
            var columnValues = rowData.ColumnValues;

            for (int i = 0; i < keyValues.Length; i++)
                keyValues[i] = columnValues[_keyColumns[i]];

            var key = (TKey)keyValueFactory.CreateFromKeyValues(keyValues);
            result.Add(key, columnValues);
        }
    }

    public void Serialize<TKey>(Stream stream, IReadOnlyDictionary<TKey, object[]> source)
    {
        var rowsData = new List<JsonRowData>(source.Count);
        foreach (var columnValues in source.Values)
            rowsData.Add(new(columnValues));

        Debug.Assert(stream.Length == 0);
        JsonSerializer.Serialize(stream, rowsData, _jsonOptions);
    }

    internal static JsonSerializerOptions CreateJsonOptions(IEntityType entityType)
    {
        return CreateJsonOptions(JsonColumnInfo.FromEntityType(entityType));
    }

    internal static JsonSerializerOptions CreateJsonOptions(IEnumerable<JsonColumnInfo> columns)
    {
        return new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
            Converters =
            {
                new JsonRowDataConverter(columns),
            },
        };
    }
}
