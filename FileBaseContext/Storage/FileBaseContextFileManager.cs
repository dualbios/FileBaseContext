using System.IO.Abstractions;
using System.Text;
using kDg.FileBaseContext.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Storage;

public class FileBaseContextFileManager : IFileBaseContextFileManager
{
    private readonly IFileSystem _fileSystem;
    private readonly string _filetype = ".json";
    private string _databasename = "";
    private string _location;

    public FileBaseContextFileManager(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string GetFileName(IEntityType _entityType)
    {
        string name = _entityType.GetTableName().GetValidFileName();

        string path = string.IsNullOrEmpty(_location)
            ? _fileSystem.Path.Combine(AppContext.BaseDirectory, _databasename)
            : _location;

        _fileSystem.Directory.CreateDirectory(path);

        return _fileSystem.Path.Combine(path, name + _filetype);
    }

    public void Init(IFileBaseContextScopedOptions options)
    {
        _databasename = options.DatabaseName;
        _location = options.Location;
    }

    public Dictionary<TKey, object[]> Load<TKey>(IEntityType _entityType, ISerializer serializer)
    {
        string path = GetFileName(_entityType);

        string content = "";
        if (_fileSystem.File.Exists(path))
        {
            content = _fileSystem.File.ReadAllText(path);
        }

        Dictionary<TKey, object[]> rows = new Dictionary<TKey, object[]>();
        serializer.Deserialize(content, rows);

        return rows;
    }

    public void Save<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> objectsMap, ISerializer serializer)
    {
        string content = serializer.Serialize(objectsMap);
        string path = GetFileName(_entityType);
        _fileSystem.File.WriteAllText(path, content);
    }
}