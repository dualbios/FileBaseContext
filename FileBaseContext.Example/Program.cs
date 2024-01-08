using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using FileBaseContext.Tests.Data;
using FileBaseContext.Tests.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileBaseContext.Example;

internal class Program
{
    private static void Main()
    {
        MockFileSystem fileSystem = new();

        DbTestContext db = new(fileSystem);
        _ = db.ContentEntries.FirstOrDefault();

        DbTestContext.InitDb(db);

        db.Contents.Load();
        db.Users.Load();
        User? user = db.Users.FirstOrDefault() ?? throw new NullReferenceException();

        user.Name = "changed name - " + DateTime.Now.ToString(CultureInfo.InvariantCulture);
        db.SaveChanges();
    }
}