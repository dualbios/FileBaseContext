using FileBaseContext.Tests.Data;
using FileBaseContext.Tests.Data.Entities;
using Microsoft.EntityFrameworkCore;
using static FileBaseContext.Tests.Data.Entities.User;

namespace FileBaseContext.Tests.Csv;

[TestClass]
public class SimplePositiveTestsForCsvDatabase
    : DbContextTestClassBase<CsvDbTestContext>
{
    [TestMethod]
    public void ReadFromFile()
    {
        //arrange
        AddDatabaseJsonFile("User.csv", """
            Id, CreatedOn, Name, Test, Test2, Type, UpdatedOn, Username
            1,01/01/2000 00:00:00,john_doe,,e4030155-ef22-4954-9b7c-c9ee398a8086,User,01/01/0001 00:00:00,john_doe_name
            2,01/01/2000 00:00:00,jane_smith,42,e4030155-ef22-4954-9b7c-c9ee398a8082,Manager,01/01/0001 00:00:00,jane_smith_name
            """);

        using CsvDbTestContext context = CreateDbContext();

        //act
        context.Users.Load();

        //assert
        string content = ReadDatabaseFileText(nameof(User) + ".csv");

        Assert.AreEqual(2, context.Users.Count());

        User user0 = context.Users.Local.ElementAt(0);
        Assert.AreEqual(1, user0.Id);
        Assert.AreEqual("john_doe", user0.Name);
        Assert.AreEqual(DateTime.Parse("01/01/2000 00:00:00"), user0.CreatedOn);
        Assert.IsNull(user0.Test);
        Assert.AreEqual(Guid.Parse("e4030155-ef22-4954-9b7c-c9ee398a8086"), user0.Test2);
        Assert.AreEqual(UserType.User, user0.Type);
        Assert.AreEqual(DateTime.Parse("01/01/0001 00:00:00"), user0.UpdatedOn);
        Assert.AreEqual("john_doe_name", user0.Username);

        User user1 = context.Users.Local.ElementAt(1);
        Assert.AreEqual(2, user1.Id);
        Assert.AreEqual("jane_smith", user1.Name);
        Assert.AreEqual(DateTime.Parse("01/01/2000 00:00:00"), user1.CreatedOn);
        Assert.AreEqual(42, user1.Test);
        Assert.AreEqual(Guid.Parse("e4030155-ef22-4954-9b7c-c9ee398a8082"), user1.Test2);
        Assert.AreEqual(UserType.Manager, user1.Type);
        Assert.AreEqual(DateTime.Parse("01/01/0001 00:00:00"), user1.UpdatedOn);
        Assert.AreEqual("jane_smith_name", user1.Username);
    }

    [TestMethod]
    public void WriteToFile()
    {
        //arrange
        using CsvDbTestContext context = CreateDbContext();
        User user = new()
        {
            Id = 1,
            Name = "john_doe",
            CreatedOn = DateTime.Parse("01/01/2000 11:12:13"),
            Test = null,
            Test2 = Guid.Parse("e4030155-ef22-4954-9b7c-c9ee398a8086"),
            Type = UserType.User,
            UpdatedOn = DateTime.Parse("01/01/0001 00:00:00"),
            Username = "john, \"doe\"; name"
        };
        context.Users.Add(user);

        //act
        context.SaveChanges();

        //assert
        string content = ReadDatabaseFileText(nameof(User) + ".csv");

        Assert.AreEqual(1, FileSystem.AllFiles.Count());
        Assert.IsTrue(FileSystem.AllFiles.Any(x => x.Contains("User.csv")));

        string fileContent = FileSystem.File.ReadAllText(FileSystem.AllFiles.First(x => x.Contains("User.csv")));
        TestHelpers.AssertString(@"Id,CreatedOn,Name,Test,Test2,Type,UpdatedOn,Username" + Environment.NewLine +
                                 @"1,01/01/2000 11:12:13,john_doe,,e4030155-ef22-4954-9b7c-c9ee398a8086,User,,""john, """"doe""""; name""" + Environment.NewLine,
                                 fileContent);
    }

    [TestMethod]
    public void ChangeTest()
    {
        // arrange
        using CsvDbTestContext context = CreateDbContext();
        User user = new()
        {
            Id = 1,
            Name = "john_doe",
            CreatedOn = DateTime.Parse("01/01/2000 00:00:00"),
            Test = null,
            Test2 = Guid.Parse("e4030155-ef22-4954-9b7c-c9ee398a8086"),
            Type = UserType.User,
            UpdatedOn = DateTime.Parse("01/01/0001 00:00:00"),
            Username = "john_doe_name"
        };
        context.Users.Add(user);
        context.SaveChanges();

        // act
        CsvDbTestContext actContext = CreateDbContext();
        actContext.Users.Load();
        actContext.Users.First().Name = "john_doe_changed";
        actContext.SaveChanges();

        // assert
        string content = ReadDatabaseFileText(nameof(User) + ".csv");

        CsvDbTestContext asserContext = CreateDbContext();
        asserContext.Users.Load();
        User assertUser = asserContext.Users.First();

        Assert.AreEqual(1, assertUser.Id);
        Assert.AreEqual("john_doe_changed", assertUser.Name);
        Assert.AreEqual(DateTime.Parse("01/01/2000 00:00:00"), assertUser.CreatedOn);
        Assert.IsNull(assertUser.Test);
        Assert.AreEqual(Guid.Parse("e4030155-ef22-4954-9b7c-c9ee398a8086"), assertUser.Test2);
        Assert.AreEqual(UserType.User, assertUser.Type);
        Assert.AreEqual(DateTime.Parse("01/01/0001 00:00:00"), assertUser.UpdatedOn);
        Assert.AreEqual("john_doe_name", assertUser.Username);
    }


    protected override CsvDbTestContext CreateDbContext(DbContextOptions<CsvDbTestContext> options)
    {
        return new CsvDbTestContext(FileSystem);
    }

    protected override string GetDatabaseName()
    {
        return CsvDbTestContext.DatabaseName;
    }

    public static string GetSubstring(string input, int index, int shift)
    {
        int startIndex = Math.Max(0, index - shift);
        int length = Math.Min(shift + shift, input.Length - startIndex);
        return input.Substring(startIndex, length);
    }
}