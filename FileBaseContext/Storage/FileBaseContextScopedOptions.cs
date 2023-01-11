namespace kDg.FileBaseContext.Storage;

public class FileBaseContextScopedOptions : IFileBaseContextScopedOptions
{
    public FileBaseContextScopedOptions(string databaseName = null, string location = null)
    {
        DatabaseName = databaseName;
        Location = location;
    }

    public string DatabaseName { get; }
    public string Location { get; }
}