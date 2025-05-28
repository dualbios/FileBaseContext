using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.Extensions.DependencyInjection;

namespace kDg.FileBaseContext.Serializers;

public class JsonRowDataSerializer : IRowDataSerializer
{
    private readonly IEntityType _entityType;
    private readonly object _keyValueFactory;
    private readonly int[] _keyColumns;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonRowDataSerializer(IEntityType entityType, object keyValueFactory, IServiceProvider serviceProvider)
    {
        _entityType = entityType;
        _keyValueFactory = keyValueFactory;
        _keyColumns = CreateKeyColumnsLookup(entityType);
        JsonSerializerOptions options = serviceProvider.GetService<JsonSerializerOptions>();
        _jsonOptions = CreateJsonOptions(entityType, options);

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
        foreach (var rowData in rowsData)
        {
            var keyValues = new object[_keyColumns.Length];
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

    internal static JsonSerializerOptions CreateJsonOptions(IEntityType entityType, JsonSerializerOptions jsonOptions) 
    {
        JsonSerializerOptions serializerOptions = CreateJsonOptions(JsonColumnInfo.FromEntityType(entityType));
        if(jsonOptions == null) {
            return serializerOptions;
        }
        
        // Merge the provided JsonSerializerOptions with the default options
        IList<JsonConverter> converters = serializerOptions.Converters.ToList();
        serializerOptions.Converters.Clear();

        foreach (JsonConverter converter in converters.Concat(jsonOptions.Converters).Distinct()) {
            serializerOptions.Converters.Add(converter);
        }

        serializerOptions.AllowTrailingCommas = jsonOptions.AllowTrailingCommas;
        serializerOptions.DefaultBufferSize = jsonOptions.DefaultBufferSize;
        serializerOptions.DefaultIgnoreCondition = jsonOptions.DefaultIgnoreCondition;
        serializerOptions.DictionaryKeyPolicy = jsonOptions.DictionaryKeyPolicy;
        serializerOptions.Encoder = jsonOptions.Encoder;
        serializerOptions.IgnoreReadOnlyFields = jsonOptions.IgnoreReadOnlyFields;
        serializerOptions.IgnoreReadOnlyProperties = jsonOptions.IgnoreReadOnlyProperties;
        serializerOptions.IncludeFields = jsonOptions.IncludeFields;
        serializerOptions.MaxDepth = jsonOptions.MaxDepth;
        serializerOptions.NumberHandling = jsonOptions.NumberHandling;
        serializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
        serializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
        serializerOptions.ReadCommentHandling = jsonOptions.ReadCommentHandling;
        serializerOptions.ReferenceHandler = jsonOptions.ReferenceHandler;
        serializerOptions.TypeInfoResolver = jsonOptions.TypeInfoResolver;
        serializerOptions.UnknownTypeHandling = jsonOptions.UnknownTypeHandling;
        serializerOptions.WriteIndented = jsonOptions.WriteIndented;

        return serializerOptions;
    }

    internal static JsonSerializerOptions CreateJsonOptions(IEnumerable<JsonColumnInfo> columns)
    {
        return new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
            Converters =
            {
                new JsonRowDataConverter(columns),
            },
        };
    }
}
