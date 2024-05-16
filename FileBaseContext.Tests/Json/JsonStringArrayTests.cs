using System.IO.Abstractions.TestingHelpers;
using kDg.FileBaseContext.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FileBaseContext.Tests.Json;

[TestClass]
public class JsonStringArrayTests
    : DbContextTestClassBase<JsonStringArrayTestContext>
{
    [TestMethod]
    public async Task ReadFromFile()
    {
        AddDatabaseJsonFile("DesignPattern.json", """
            [
                {
                    "Id": 2,
                    "Stage": "done",
                    "Title": "NullObject",
                    "Description": "Instead of returning null, use an object which implements the expected interface, but whose method body is empty.",
                    "LinkWikipedia": "https://en.wikipedia.org/wiki/Null_object_pattern",
                    "DemoFileCsproj": "NullObject.csproj",
                    "ClassNames": "EmptyFolder,NullLogger",
                    "Homework": [
                        "When retrieving data (e.g. a Person with ID =-1) from a database, return a NullObject instead of null.",
                        "How will you verify that the object is a NullObject?"
                    ],
                    "Tags": "behavioral,design pattern"
                }
            ]
            """);

        using var context = CreateDbContext();

        await context.DesignPatterns.LoadAsync();
        Assert.AreEqual(1, context.DesignPatterns.Count());
    }

    protected override JsonStringArrayTestContext CreateDbContext(DbContextOptions<JsonStringArrayTestContext> options)
    {
        return new JsonStringArrayTestContext(FileSystem);
    }

    protected override string GetDatabaseName()
    {
        // Since we're not using the options,
        // the database name is only accessable from a db instance,
        // rather than from the configured options.
        return JsonStringArrayTestContext.DatabaseName;
    }
}

public class DesignPattern
{
    public int Id { get; set; }
    public string Stage { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string LinkWikipedia { get; set; }
    public string DemoFileCsproj { get; set; }
    public string ClassNames { get; set; }
    public string[] Homework { get; set; }
    public string Tags { get; set; }
}

public class JsonStringArrayTestContext : DbContext
{
    public const string DatabaseName = "my_local_db";

    private readonly MockFileSystem _fileSystem;

    public JsonStringArrayTestContext(MockFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public DbSet<DesignPattern> DesignPatterns { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseFileBaseContextDatabase(DatabaseName, null, services => { services.AddMockFileSystem(_fileSystem); });
    }
}
