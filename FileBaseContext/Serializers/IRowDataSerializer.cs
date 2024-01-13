namespace kDg.FileBaseContext.Serializers;

public interface IRowDataSerializer
{
    string Serialize<TKey>(Dictionary<TKey, object[]> list);

    Dictionary<TKey, object[]> Deserialize<TKey>(string list, Dictionary<TKey, object[]> newList);
}