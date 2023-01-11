# FileBaseContext

**FileBaseContext** is a provider of **Entity Framework 7** to store database information in files. 

It was built for development purposes. All information is stored in files that can be added, updated, or deleted manually.

## Benefits

- you don't need a database
- rapid modeling
- version control supported
- supports all serializable .NET types

## Download

https://www.nuget.org/packages/FileBaseContext/

## Configure Database Context

- add database context to services
  ```cs
services.AddDbContext<ApplicationDbContext>(options => options.UseFileBaseContextDatabase("dbUser"));
  ```
- or configure the database context itself

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