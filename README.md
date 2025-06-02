# FileBaseContext

**FileBaseContext** is an [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) provider to store database records in files.

It was built for rapid development purposes. All information that is stored in files that can be added, updated or deleted manually.

## Benefits

- you don't need a database
- rapid modeling
- version control supported
- supports all serializable .NET types
- unit tests

## Download

https://www.nuget.org/packages/FileBaseContext/

| Provider Version | EF Core Version |
| ---------------- | --------------- |
| 1.0.x  | 7  |
| 2.0.x thru 4.0.x  | 8  |
| 5.0.x thru 5.1.x  | 9  |

## Configure Database Context

add database context to services

```cs
services.AddDbContext<ApplicationDbContext>(options => options.UseFileBaseContextDatabase("dbUser"));
```

or configure the database context itself

```cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseFileBaseContextDatabase("my_local_db");
    base.OnConfiguring(optionsBuilder);
}
```

## Configure Provider

##### Named database 
```cs
optionsBuilder.UseFileBaseContextDatabase(databaseName: "my_local_db");
```

##### Custom location
```cs
optionsBuilder.UseFileBaseContextDatabase(location: "C:\Temp\userDb");
```

## Unit testing

Since version 2.1.0 FileBaseContext injects access to the file system through `System.IO.Abstractions` library. It allows the use of the provider in unit tests.

If you need to use the provider in unit tests, you can change `IFileSystem` to `MockFileSystem` in OnConfiguring method in datacontext class.

```cs
private readonly MockFileSystem _fileSystem;
public DbTestContext(MockFileSystem fileSystem)
{
    _fileSystem = fileSystem;
}

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseFileBaseContextDatabase(DatabaseName, null, services =>
    {
        services.AddMockFileSystem(_fileSystem);
    });
}
```
Please find an example in the SimplePositiveTests class in the test project

## Update for 5.1.0

You can now specify serialization options in `UseFileBaseContextDatabase`, for example adding a converter like [GeoJSON4STJ](https://www.nuget.org/packages/NetTopologySuite.IO.GeoJSON4STJ).

```cs
options.UseFileBaseContextDatabase(location: "userDb", applyServices: services =>
{
    services.ConfigureJsonSerializerOptions(options =>
    {
        options.Converters.Add(new GeoJsonConverterFactory());
    });
});
```

This will enable you to use [Spatial Data](https://learn.microsoft.com/en-us/ef/core/modeling/spatial) with the addition of the [NetTopologySuite](https://www.nuget.org/packages/NetTopologySuite) library.

## Update for 5.0.0

Upgraded to .NET 9 and EF Core 9

## New in 4.0.0

Since version 4.0.0 the provider supports persisting data in CSV files.
The CSV files are stored in the directory using the database name. 
The CSV files are named using the entity name. 
The first row in the CSV file is the header row with the column names.

## Changes in 3.0.0

In version 3.0.0 the provider was changed to support numeric values without quotation marks.

```json
{
    "IntProp": 42,
    "LongProperty": 420,
    "DateTime": "2023-12-26T19:28:08"
}
```

This led to **breaking changes** in the provider. If you have used the provider before you'll need to manually update the database files. 
The changes also affect `DateTime` and `DateTimeOffset` values as those values are stored as a string in the database file.
The first run of the application could be slow because multiple `System.Text.Json.JsonException` will be generated.
Performance will improve after the provider saves the database to files as the data will then be stored in the new format.
If you still have performance issues you will need to manually update the database files.
