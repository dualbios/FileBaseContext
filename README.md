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
	optionsBuilder.UseFileStoreDatabase("my_local_db");
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