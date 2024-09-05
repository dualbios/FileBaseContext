# FileBaseContext

**FileBaseContext** is a provider of **Entity Framework Core 8** to store database information in files. 

It was built for development purposes. All information is stored in files that can be added, updated, or deleted manually.

## Benefits

- you don't need a database
- rapid modeling
- version control supported
- supports all serializable .NET types-
- unit tests

## Download

https://www.nuget.org/packages/FileBaseContext/

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

Since 2.1.0 the FileBaseContext injects access to the file system through System.IO.Abstractions library. It allows the use of the provider in unit tests.

If you need to use the provider in unit tests, you can change IFileSystem to MockFileSystem in OnConfiguring method in datacontext class.

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
Please find example in the SimplePositiveTests class in the test project

## New in 4.0.0

Since the 4.0.0 version the FileBaseContext supports persisting data in the CSV files.
The CSV files are stored in the directory with the database name. 
The CSV files are named by the entity name. 
The first row in the CSV file is the header with the column names.

## ! Braking changes in 3.0.0 !

In 3.0.0 version the provider was changed to support numeric values without quotation marks.

```
{
    "IntProp": 42,
    "LongProperty": 420,
    "DateTime": "2023-12-26T19:28:08"
}
```

The led to breaking changes in the provider. If you have used the provider before, you need to manualy update the database files. 
The changes also affect on DateTime and DateTimeOffset values. The values are stored as string in the database.
First run of the application could be slow becasuse a lot of System.Text.Json.JsonException will be provided.
Performance be fixed after provider saves a database to files. While that the data will be stored in new formats.
If you still have performance issues you need to manualy update the database files.
