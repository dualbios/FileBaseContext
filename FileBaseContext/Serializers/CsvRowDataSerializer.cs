using System.ComponentModel;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Serializers;

internal class CsvRowDataSerializer : IRowDataSerializer
{
    private readonly IEntityType _entityType;
    private readonly int[] _keyColumns;
    private readonly object _keyValueFactory;
    private readonly string[] _columnNames;
    private readonly IProperty[] _properties;

    public CsvRowDataSerializer(IEntityType entityType, object keyValueFactory)
    {
        _entityType = entityType;
        _keyValueFactory = keyValueFactory;
        _keyColumns = CreateKeyColumnsLookup(entityType);
        _columnNames = entityType.GetProperties()
                                 .Select(p => p.GetColumnName())
                                 .ToArray();

        _properties = entityType.GetProperties().ToArray();
    }

    public string FileExtension => ".csv";

    public void Deserialize<TKey>(Stream stream, Dictionary<TKey, object[]> result)
    {
        // deserialize from CSV
        if (stream == null)
        {
            return;
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        using StreamReader reader = new(stream);
        //read column names
        string header = reader.ReadLine();
        if (header == null)
        {
            return;
        }

        string[] columnNames = header.Split(',');
        if (columnNames.Length != _columnNames.Length)
        {
            throw new InvalidOperationException("Invalid CSV file");
        }

        //read data
        while (!reader.EndOfStream)
        {
            string[] columnValues = ReadColumnValues(reader, _properties.Length);
            object[] values = new object[columnValues.Length];
            for (int i = 0; i < columnValues.Length; i++)
            {
                string columnValue = columnValues[i];

                Type clrType = _properties[i].GetValueConverter()?.ProviderClrType ?? _properties[i].ClrType;


                if (clrType == typeof(string))
                {
                    if (string.IsNullOrEmpty(columnValue))
                    {
                        values[i] = null;
                        continue;
                    }
                }

                if (columnValue.StartsWith('"') && columnValue.EndsWith('"'))
                {
                    columnValue = columnValue[1..^1];
                }

                if (columnValue.Contains("\"\""))
                {
                    columnValue = columnValue.Replace("\"\"", "\"");
                }

                if (clrType == typeof(byte[]))
                {
                    values[i] = Convert.FromBase64String(columnValues[i]);
                }
                else
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(clrType);
                    object value = typeConverter.ConvertFromInvariantString(columnValue);
                    values[i] = value;
                }
            }

            var keyValues = new object[_keyColumns.Length];
            for (int i = 0; i < keyValues.Length; i++)
            {
                keyValues[i] = values[_keyColumns[i]];
            }

            var key = (TKey)((IPrincipalKeyValueFactory<TKey>)_keyValueFactory).CreateFromKeyValues(keyValues);
            result.Add(key, values);
        }
    }

    private static string[] ReadColumnValues(StreamReader inputStream, int propertiesCount)
    {
        int propertyIndex = 0;
        bool inQuotes = false;
        bool isEscaping = false;
        var columnValueBuilder = new StringBuilder();
        var columnValues = new List<string>();

        while (propertyIndex < propertiesCount && !inputStream.EndOfStream)
        {
            char ch = (char)inputStream.Read();

            if (isEscaping)
            {
                columnValueBuilder.Append(ch);
                isEscaping = false;
            }
            else if (ch == '"')
            {
                columnValueBuilder.Append(ch);
                inQuotes = !inQuotes;
            }
            else if (ch == '\\')
            {
                isEscaping = true;
            }
            else if (ch == ',' && !inQuotes)
            {
                columnValues.Add(columnValueBuilder.ToString());
                columnValueBuilder.Clear();
                propertyIndex++;
            }
            else if (ch == '\n' && !inQuotes)
            {
                if (columnValueBuilder[^1] == '\r')
                    columnValueBuilder.Remove(columnValueBuilder.Length - 1, 1);
                columnValues.Add(columnValueBuilder.ToString());

                return columnValues.ToArray();
            }
            else
            {
                columnValueBuilder.Append(ch);
            }
        }

        columnValues.Add(columnValueBuilder.ToString());
        return columnValues.ToArray();
    }


    public void Serialize<TKey>(Stream stream, IReadOnlyDictionary<TKey, object[]> source)
    {
        using StreamWriter writer = new(stream);
        //write column names

        writer.WriteLine(string.Join(",", _columnNames));

        //write data
        foreach (var row in source)
        {
            object[] columnValues = row.Value;
            for (int i = 0; i < columnValues.Length; i++)
            {
                if (i > 0)
                {
                    writer.Write(',');
                }

                Type clrType = _properties[i].GetValueConverter()?.ProviderClrType ?? _properties[i].ClrType;
                string value = null;

                if (clrType == typeof(string) && columnValues[i] as string == string.Empty)
                {
                    writer.Write("\"\"");
                    continue;
                }

                if (clrType == typeof(byte[]))
                {
                    if (columnValues[i] != null)
                    {
                        value = Convert.ToBase64String((columnValues[i] as byte[])!);
                    }
                }
                else
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(clrType);
                    object columnValue = columnValues[i];
                    value = typeConverter.ConvertToInvariantString(columnValue);
                }

                if (value != null && value.Contains('"'))
                {
                    value = $"\"{value.Replace("\"", "\"\"")}\""; // escape quotes
                }

                if (value != null && (value.Contains(',') || value.Contains('\n')) && !value.StartsWith('"') && !value.EndsWith('"'))
                {
                    value = $"\"{value}\"";
                }

                writer.Write(value);
            }

            writer.WriteLine();
        }
    }

    private static int[] CreateKeyColumnsLookup(IEntityType entityType)
    {
        var columnNames = entityType.GetProperties()
                                    .Select(p => p.GetColumnName())
                                    .ToArray();

        return entityType.FindPrimaryKey().Properties
                         .Select(p => Array.IndexOf(columnNames, p.GetColumnName()))
                         .ToArray();
    }
}