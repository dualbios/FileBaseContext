using System.IO.Abstractions.TestingHelpers;
using FileBaseContext.Tests.Data;
using FileBaseContext.Tests.Data.Entities;
using Microsoft.EntityFrameworkCore;

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
}