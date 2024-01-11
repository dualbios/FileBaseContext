namespace kDg.FileBaseContext.Serializers;

public interface IRowDataSerializer
{
    string FileExtension { get; }

    void Serialize<TKey>(Stream stream, IReadOnlyDictionary<TKey, object[]> source);

    void Deserialize<TKey>(Stream stream, Dictionary<TKey, object[]> result);
}