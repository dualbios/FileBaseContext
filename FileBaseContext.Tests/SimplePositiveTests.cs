using System.IO.Abstractions.TestingHelpers;
using FileBaseContext.Tests.Data;
using FileBaseContext.Tests.Data.Entities;
using Microsoft.EntityFrameworkCore;
using static FileBaseContext.Tests.Data.Entities.User;

namespace FileBaseContext.Tests;

[TestClass]
public class SimplePositiveTests
{
    [TestMethod]
    public void InitializeDbTest()
    {
        MockFileSystem mockFileSystem = new();
        DbTestContext context = new(mockFileSystem);

        DbTestContext.InitDb(context);

        Assert.AreEqual(6, mockFileSystem.AllFiles.Count());
        Assert.IsTrue(mockFileSystem.AllFiles.Any(x => x.Contains("Content.json")));
        Assert.IsTrue(mockFileSystem.AllFiles.Any(x => x.Contains("ContentEntry.json")));
        Assert.IsTrue(mockFileSystem.AllFiles.Any(x => x.Contains("Messurement.json")));
        Assert.IsTrue(mockFileSystem.AllFiles.Any(x => x.Contains("Setting.json")));
        Assert.IsTrue(mockFileSystem.AllFiles.Any(x => x.Contains("SimpleEntity.json")));
        Assert.IsTrue(mockFileSystem.AllFiles.Any(x => x.Contains("User.json")));
    }

    [TestMethod]
    public void CheckChangeUserNameTest()
    {
        MockFileSystem mockFileSystem = new();
        DbTestContext context = new(mockFileSystem);
        DbTestContext.InitDb(context);

        context.Contents.Load();
        context.Users.Load();
        User user = context.Users.FirstOrDefault()!;
        user.Name = "New User Name";
        context.SaveChanges();

        string userFileName = mockFileSystem.Path.Combine(AppContext.BaseDirectory, DbTestContext.DatabaseName, "User.json");
        string userFileContent = mockFileSystem.File.ReadAllText(userFileName);

        StringAssert.Contains(userFileContent, "New User Name");
    }

    [TestMethod]
    public async Task ReadFromFile()
    {
        MockFileSystem mockFileSystem = new();
        mockFileSystem.AddFile(mockFileSystem.Path.Combine(AppContext.BaseDirectory, DbTestContext.DatabaseName, "User.json"), 
                               new MockFileData(@"[
{
    ""Id"": ""1"",
    ""CreatedOn"": ""01/01/2000 00:00:00"",
    ""Name"": ""john_doe"",
    ""Test"": """",
    ""Test2"": ""e4030155-ef22-4954-9b7c-c9ee398a8086"",
    ""Type"": ""User"",
    ""UpdatedOn"": ""01/01/0001 00:00:00"",
    ""Username"": ""john_doe_name""
  },
{
    ""Id"": ""2"",
    ""CreatedOn"": ""01/01/2000 00:00:00"",
    ""Name"": ""jane_smith"",
    ""Test"": ""42"",
    ""Test2"": ""e4030155-ef22-4954-9b7c-c9ee398a8082"",
    ""Type"": ""Manager"",
    ""UpdatedOn"": ""01/01/0001 00:00:00"",
    ""Username"": ""jane_smith_name""
  }
]"));
        DbTestContext context = new(mockFileSystem);

        await context.Users.LoadAsync();
        User user = context.Users.FirstOrDefault()!;

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
}