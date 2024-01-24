using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Serializers;

public class CsvRowDataSerializer : IRowDataSerializer
{
    private readonly string[] _columnNames;
    private readonly IEntityType _entityType;
    private readonly object _keyValueFactory;
    private readonly IProperty[] _properties;

    public CsvRowDataSerializer(IEntityType entityType, object keyValueFactory)
    {
        _entityType = entityType;
        _keyValueFactory = keyValueFactory;
        _columnNames = entityType.GetProperties()
                                 .Select(p => p.GetColumnName())
                                 .ToArray();

        _properties = entityType.GetProperties().ToArray();
    }

    public string FileExtension => ".csv";

    public void Serialize<TKey>(Stream stream, IReadOnlyDictionary<TKey, object[]> source)
    {
        using StreamWriter writer = new(stream);
        //write column names
        for (int i = 0; i < _columnNames.Length; i++)
        {
            if (i > 0) writer.Write(',');

            writer.Write(_columnNames[i]);
        }

        writer.WriteLine();

        //write data
        foreach (var row in source)
        {
            object[] columnValues = row.Value;
            for (int i = 0; i < columnValues.Length; i++)
            {
                if (i > 0) writer.Write(',');

                //var val = Convert.ToString(columnValues[i], CultureInfo.InvariantCulture);
                Type clrType = _properties[i].GetValueConverter()?.ProviderClrType ?? _properties[i].ClrType;
                object val = null;

                if (clrType == typeof(byte[]))
                {
                    if (columnValues[i] != null) val = Convert.ToBase64String(columnValues[i] as byte[]);
                    //val = BitConverter.ToString(columnValues[i] as byte[]);
                    //val = JsonSerializer.Serialize(columnValues[i] as byte[], clrType).Trim('"');
                    //serialize byte array to file
                    //val = Encoding.UTF8.GetString(columnValues[i] as byte[]);
                    //val = string.Join(";", (byte[])columnValues[i]);
                }
                else
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(clrType);
                    object columnValue = columnValues[i];
                    val = typeConverter.ConvertToInvariantString(columnValue);
                }


                writer.Write(val);
            }

            writer.WriteLine();
        }
    }

    public void Deserialize<TKey>(Stream stream, Dictionary<TKey, object[]> result)
    {
        using StreamReader reader = new(stream);
        int rowIndex = 0;

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            rowIndex++;
            if (rowIndex == 1) continue;

            string[] columnValues = line.Split(',');
            object[] cVal = new object[columnValues.Length];
            for (int i = 0; i < columnValues.Length; i++)
                try
                {
                    string columnValue = string.Empty;
                    if (i < columnValues.Length)
                        columnValue = columnValues[i];

                    Type clrType = _properties[i].GetValueConverter()?.ProviderClrType ?? _properties[i].ClrType;
                    if (clrType == typeof(byte[]))
                    {
                        if (string.IsNullOrEmpty(columnValue))
                            cVal[i] = null;
                        else
                            cVal[i] = Convert.FromBase64String(columnValue);
                        //cVal[i] = BitConverter.GetBytes(columnValue);
                        //cVal[i] = JsonSerializer.Deserialize(columnValue, clrType);
                        //var r = new byte[columnValue.Length];
                        //for (int index = 0; index < columnValue.Length; index++)
                        //    r[index] = checked((byte)columnValue[index]);
                        //cVal[i] = r;
                    }
                    else if (clrType.IsNullableType() && string.IsNullOrEmpty(columnValue))
                    {
                        cVal[i] = null;
                    }
                    else if (clrType == typeof(string))
                    {
                        cVal[i] = columnValue;
                    }
                    else
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(clrType);
                        cVal[i] = typeConverter.ConvertFromInvariantString(columnValue.Trim());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            var keyValues = new object[_columnNames.Length];
            for (int i = 0; i < keyValues.Length; i++) keyValues[i] = cVal[i];

            var keyValueFactory = (IPrincipalKeyValueFactory<TKey>)_keyValueFactory;
            TKey key = (TKey)keyValueFactory.CreateFromKeyValues(keyValues);

            result.Add(key, cVal);
        }
    }
}