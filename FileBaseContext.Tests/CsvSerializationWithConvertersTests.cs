using kDg.FileBaseContext.Extensions;
using kDg.FileBaseContext.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Linq.Expressions;
using System.Text;

namespace FileBaseContext.Tests;

[TestClass]
public class CsvSerializationWithConvertersTests
    : DbContextTestClassBase<CsvSerializationWithConvertersTests.CsvTestDbContext>
{
    [TestMethod]
    public void CanRoundTripValueConverterToString_Null()
    {
        using (CsvTestDbContext db = CreateDbContext())
        {
            db.Add(new EntityUsesStringValueConverter()
            {
                Id = 1,
                ConvertsToString = null,
            });
            db.SaveChanges();
        }

        using (CsvTestDbContext db = CreateDbContext())
        {
            EntityUsesStringValueConverter entity = db.EntityUsesStringValueConverter.Single();
            Assert.IsNull(entity.ConvertsToString);
        }
    }

    [TestMethod]
    public void CanRoundTripValueConverterToString_Empty()
    {
        using (CsvTestDbContext db = CreateDbContext())
        {
            db.Add(new EntityUsesStringValueConverter()
            {
                Id = 1,
                ConvertsToString = new(string.Empty),
            });
            db.SaveChanges();
        }

        using (CsvTestDbContext db = CreateDbContext())
        {
            EntityUsesStringValueConverter entity = db.EntityUsesStringValueConverter.Single();
            Assert.AreEqual(String.Empty, entity.ConvertsToString.Value);
        }
    }

    [TestMethod]
    public void CanRoundTripValueConverterToString_NonEmpty()
    {
        using (CsvTestDbContext db = CreateDbContext())
        {
            db.Add(new EntityUsesStringValueConverter()
            {
                Id = 1,
                ConvertsToString = new("String1"),
            });
            db.SaveChanges();
        }

        using (CsvTestDbContext db = CreateDbContext())
        {
            db.EntityUsesStringValueConverter.Load();
            EntityUsesStringValueConverter entity = db.EntityUsesStringValueConverter.Single();
            Assert.AreEqual("String1", entity.ConvertsToString?.Value);
        }
    }

    [TestMethod]
    public void CanRoundTripValueConverterToBytes_Null()
    {
        using (CsvTestDbContext db = CreateDbContext())
        {
            db.Add(new EntityUsesBytesValueConverter()
            {
                Id = 1,
                ConvertsToBytes = null,
            });
            db.SaveChanges();
        }

        using (CsvTestDbContext db = CreateDbContext())
        {
            EntityUsesBytesValueConverter entity = db.EntitiesUseBytesValueConverters.Single();
            Assert.AreEqual(String.Empty, entity.ConvertsToBytes.Value);
        }
    }

    [TestMethod]
    public void CanRoundTripValueConverterToBytes_Empty()
    {
        using (CsvTestDbContext db = CreateDbContext())
        {
            db.Add(new EntityUsesBytesValueConverter()
            {
                Id = 1,
                ConvertsToBytes = new(string.Empty),
            });
            db.SaveChanges();
        }
        string content = ReadDatabaseFileText(nameof(EntityUsesBytesValueConverter) + ".csv");

        using (CsvTestDbContext db = CreateDbContext())
        {
            EntityUsesBytesValueConverter entity = db.EntitiesUseBytesValueConverters.Single();
            Assert.AreEqual(String.Empty, entity.ConvertsToBytes.Value);
        }
    }

    [TestMethod]
    public void CanRoundTripValueConverterToBytes_NonEmpty()
    {
        using (CsvTestDbContext db = CreateDbContext())
        {
            db.Add(new EntityUsesBytesValueConverter()
            {
                Id = 1,
                ConvertsToBytes = new("String1"),
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityUsesBytesValueConverter) + ".csv");

        using (CsvTestDbContext db = CreateDbContext())
        {
            EntityUsesBytesValueConverter entity = db.EntitiesUseBytesValueConverters.Single();
            Assert.AreEqual("String1", entity.ConvertsToBytes?.Value);
        }
    }

    protected override void ConfigureDbContextOptions(DbContextOptionsBuilder<CsvTestDbContext> options)
    {
        options.UseFileBaseContextDatabase(nameof(CsvTestDbContext), null, services =>
        {
            services.AddMockFileSystem(FileSystem);
            services.AddCsvRowDataSerializer();
        });
    }

    public sealed class CsvTestDbContext(
            DbContextOptions<CsvTestDbContext> options)
        : DbContext(options)
    {
        public DbSet<EntityUsesStringValueConverter> EntityUsesStringValueConverter { get; set; } = null!;
        public DbSet<EntityUsesBytesValueConverter> EntitiesUseBytesValueConverters { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EntityUsesStringValueConverter>()
                        .Property(o => o.ConvertsToString)
                        .HasConversion<ConvertValueToString>();

            modelBuilder.Entity<EntityUsesBytesValueConverter>()
                        .Property(o => o.ConvertsToBytes)
                        .HasConversion<ConvertValueToByteArray>();
        }
    }

    public class EntityUsesStringValueConverter
    {
        [Key] public int Id { get; set; }

        public ValueWrapper? ConvertsToString { get; set; }
    }

    public class EntityUsesBytesValueConverter
    {
        [Key] public int Id { get; set; }

        public ValueWrapper? ConvertsToBytes { get; set; }
    }

    public record ValueWrapper(
        string Value);

    private sealed class ConvertValueToString()
        : ValueConverter<ValueWrapper, string>(
            modelValue => modelValue.Value,
            storeValue => new(storeValue));

    private sealed class ConvertValueToByteArray()
        : ValueConverter<ValueWrapper, byte[]>(
            modelValue => Encoding.UTF8.GetBytes(modelValue.Value),
            storeValue => new(Encoding.UTF8.GetString(storeValue)));

    protected override CsvTestDbContext CreateDbContext(DbContextOptions<CsvTestDbContext> options)
    {
        return new CsvTestDbContext(options);
    }
}