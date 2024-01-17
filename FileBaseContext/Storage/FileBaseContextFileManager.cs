using kDg.FileBaseContext.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IO.Abstractions;
using System.Text;

namespace kDg.FileBaseContext.Storage;

public class FileBaseContextFileManager : IFileBaseContextFileManager
{
    private readonly IFileSystem _fileSystem;
    private string _databasename = "";
    private string _location;

    public FileBaseContextFileManager(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string GetFileName(IEntityType _entityType, IRowDataSerializer serializer)
    {
        string name = _entityType.GetTableName().GetValidFileName();

        string path = string.IsNullOrEmpty(_location)
            ? _fileSystem.Path.Combine(AppContext.BaseDirectory, _databasename)
            : _location;

        _fileSystem.Directory.CreateDirectory(path);

        return _fileSystem.Path.Combine(path, name + serializer.FileExtension);
    }

    public void Init(IFileBaseContextScopedOptions options)
    {
        _databasename = options.DatabaseName;
        _location = options.Location;
    }

    public Dictionary<TKey, object[]> Load<TKey>(IEntityType _entityType, IRowDataSerializer serializer)
    {
        var rows = new Dictionary<TKey, object[]>();
        try
        {
            string path = GetFileName(_entityType, serializer);
            using var stream = _fileSystem.File.OpenRead(path);
            serializer.Deserialize(stream, rows);
        }
        catch (Exception ex)
        when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
        }

        return rows;
    }

    public void Save<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> objectsMap, IRowDataSerializer serializer)
    {
        string path = GetFileName(_entityType, serializer);
        using var stream = _fileSystem.File.Create(path);
        serializer.Serialize(stream, objectsMap);
    }
}