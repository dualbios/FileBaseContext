using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FileBaseContext.Tests
{
    [TestClass]
    public class SerializationWithConvertersTests
        : DbContextTestClassBase<SerializationWithConvertersTests.TestDbContext>
    {
        [TestMethod]
        public void CanRoundTripValueConverterToString_Null()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityUsesStringValueConverter()
                {
                    Id = 1,
                    ConvertsToString = null,
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntityUsesStringValueConverter.Single();
                Assert.IsNull(entity.ConvertsToString);
            }
        }

        [TestMethod]
        public void CanRoundTripValueConverterToString_Empty()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityUsesStringValueConverter()
                {
                    Id = 1,
                    ConvertsToString = new(string.Empty),
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntityUsesStringValueConverter.Single();
                Assert.IsNotNull(entity.ConvertsToString);
                Assert.AreEqual(string.Empty, entity.ConvertsToString.Value);
            }
        }

        [TestMethod]
        public void CanRoundTripValueConverterToString_NonEmpty()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityUsesStringValueConverter()
                {
                    Id = 1,
                    ConvertsToString = new("String1"),
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntityUsesStringValueConverter.Single();
                Assert.AreEqual("String1", entity.ConvertsToString?.Value);
            }
        }

        [TestMethod]
        public void CanRoundTripValueConverterToBytes_Null()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityUsesBytesValueConverter()
                {
                    Id = 1,
                    ConvertsToBytes = null,
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesUseBytesValueConverters.Single();
                Assert.IsNull(entity.ConvertsToBytes);
            }
        }

        [TestMethod]
        public void CanRoundTripValueConverterToBytes_Empty()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityUsesBytesValueConverter()
                {
                    Id = 1,
                    ConvertsToBytes = new(string.Empty),
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesUseBytesValueConverters.Single();
                Assert.IsNotNull(entity.ConvertsToBytes);
                Assert.AreEqual(string.Empty, entity.ConvertsToBytes.Value);
            }
        }

        [TestMethod]
        public void CanRoundTripValueConverterToBytes_NonEmpty()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityUsesBytesValueConverter()
                {
                    Id = 1,
                    ConvertsToBytes = new("String1"),
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesUseBytesValueConverters.Single();
                Assert.AreEqual("String1", entity.ConvertsToBytes?.Value);
            }
        }

        protected override TestDbContext CreateDbContext(DbContextOptions<TestDbContext> options)
            => new(options);

        public sealed class TestDbContext(
            DbContextOptions<TestDbContext> options)
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
            [Key]
            public int Id { get; set; }

            public ValueWrapper? ConvertsToString { get; set; }
        }

        public class EntityUsesBytesValueConverter
        {
            [Key]
            public int Id { get; set; }

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
    }
}
