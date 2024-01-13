using FileBaseContext.Tests.Data;
using FileBaseContext.Tests.Data.Entities;
using Microsoft.EntityFrameworkCore;
using static FileBaseContext.Tests.Data.Entities.User;

namespace FileBaseContext.Tests;

[TestClass]
public class SimplePositiveTests
    : DbContextTestClassBase<DbTestContext>
{
    [TestMethod]
    public void InitializeDbTest()
    {
        using var context = CreateDbContext();
        DbTestContext.InitDb(context);

        Assert.AreEqual(6, FileSystem.AllFiles.Count());
        Assert.IsTrue(FileSystem.AllFiles.Any(x => x.Contains("Content.json")));
        Assert.IsTrue(FileSystem.AllFiles.Any(x => x.Contains("ContentEntry.json")));
        Assert.IsTrue(FileSystem.AllFiles.Any(x => x.Contains("Messurement.json")));
        Assert.IsTrue(FileSystem.AllFiles.Any(x => x.Contains("Setting.json")));
        Assert.IsTrue(FileSystem.AllFiles.Any(x => x.Contains("SimpleEntity.json")));
        Assert.IsTrue(FileSystem.AllFiles.Any(x => x.Contains("User.json")));
    }

    [TestMethod]
    public void CheckChangeUserNameTest()
    {
        using (var setupDb = CreateDbContext())
        {
            DbTestContext.InitDb(setupDb);
        }

        using (var testDb = CreateDbContext())
        {
            User user = testDb.Users.FirstOrDefault()!;
            user.Name = "New User Name";
            testDb.SaveChanges();
        }

        using (var assertDb = CreateDbContext())
        {
            User savedUser = assertDb.Users.FirstOrDefault()!;
            Assert.AreEqual("New User Name", savedUser.Name);

            var userFileContent = ReadDatabaseFileText("User.json");
            StringAssert.Contains(userFileContent, "New User Name");
        }
    }

    [TestMethod]
    public async Task ReadFromFile()
    {
        AddDatabaseJsonFile("User.json", """
            [
                {
                    "Id": "1",
                    "CreatedOn": "01/01/2000 00:00:00",
                    "Name": "john_doe",
                    "Test": "",
                    "Test2": "e4030155-ef22-4954-9b7c-c9ee398a8086",
                    "Type": "User",
                    "UpdatedOn": "01/01/0001 00:00:00",
                    "Username": "john_doe_name"
                },
                {
                    "Id": "2",
                    "CreatedOn": "01/01/2000 00:00:00",
                    "Name": "jane_smith",
                    "Test": "42",
                    "Test2": "e4030155-ef22-4954-9b7c-c9ee398a8082",
                    "Type": "Manager",
                    "UpdatedOn": "01/01/0001 00:00:00",
                    "Username": "jane_smith_name"
                }
            ]
            """);

        using var context = CreateDbContext();

        await context.Users.LoadAsync();
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

    protected override DbTestContext CreateDbContext(DbContextOptions<DbTestContext> options)
    {
        // NOTE: Ignoring `options` for this test class
        // to support `DbTestContext` doing its own OnConfiguring wireup.
        return new DbTestContext(FileSystem);
    }

    protected override string GetDatabaseName()
    {
        // Since we're not using the options,
        // the database name is only accessable from a db instance,
        // rather than from the configured options.
        return DbTestContext.DatabaseName;
    }
}