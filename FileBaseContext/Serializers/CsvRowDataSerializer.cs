using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Serializers;

public class CsvRowDataSerializer : IRowDataSerializer
{
    public CsvRowDataSerializer(IEntityType entityType, object keyValueFactory)
    {
        throw new NotImplementedException();
    }

    public void Deserialize<TKey>(Stream stream, Dictionary<TKey, object[]> result)
    {
        throw new NotImplementedException();
    }

    public string FileExtension => ".csv";

    public void Serialize<TKey>(Stream stream, IReadOnlyDictionary<TKey, object[]> source)
    {
        throw new NotImplementedException();
    }
}