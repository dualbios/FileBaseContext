using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FileBaseContext.Tests
{
    [TestClass]
    public class SerializationTests
        : DbContextTestClassBase<SerializationTests.TestDbContext>
    {
        [TestMethod]
        public void CanDifferentiateNullFromDefault_Primitives()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityHasNullables()
                {
                    Id = 1,
                    StringValue = null,
                    IntValue = null,
                });

                db.Add(new EntityHasNullables()
                {
                    Id = 2,
                    StringValue = string.Empty,
                    IntValue = 0,
                });

                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entityWithNulls = db.EntitiesHaveNullables.Single(o => o.Id == 1);
                var entityWithDefaults = db.EntitiesHaveNullables.Single(o => o.Id == 2);

                Assert.IsNull(entityWithNulls.StringValue);
                Assert.IsNull(entityWithNulls.IntValue);

                Assert.AreEqual(string.Empty, entityWithDefaults.StringValue);
                Assert.AreEqual(0, entityWithDefaults.IntValue);
            }
        }

        [TestMethod]
        public void CanSaveNullByteArrays()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityHasByteArray()
                {
                    Id = 1,
                    ByteArray = null,
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesHaveByteArrays.Single();
                Assert.IsNull(entity.ByteArray);
            }
        }

        [TestMethod]
        public void CanSaveEmptyByteArrays()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityHasByteArray()
                {
                    Id = 1,
                    ByteArray = [],
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesHaveByteArrays.Single();
                Assert.IsNotNull(entity.ByteArray);
                Assert.AreEqual(0, entity.ByteArray.Length);
            }
        }

        [TestMethod]
        public void CanSaveByteArrays()
        {
            using (var db = CreateDbContext())
            {
                db.Add(new EntityHasByteArray()
                {
                    Id = 1,
                    ByteArray = [0x00, 0x01, 0x02, 0x03],
                });
                db.SaveChanges();
            }

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesHaveByteArrays.Single();
                Assert.IsNotNull(entity.ByteArray);
                CollectionAssert.AreEqual(new byte[] { 0x00, 0x01, 0x02, 0x03 }, entity.ByteArray);
            }
        }

        [TestMethod]
        public void CanDeserializeByteArraysFromCsv()
        {
            AddDatabaseJsonFile("EntityHasByteArray.json", """
                [
                    {
                        "Id": "1",
                        "ByteArray": "0,1,2,3"
                    }
                ]
                """);

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesHaveByteArrays.Single();
                Assert.IsNotNull(entity.ByteArray);
                CollectionAssert.AreEqual(new byte[] { 0x00, 0x01, 0x02, 0x03 }, entity.ByteArray);
            }
        }

        [TestMethod]
        public void CanDeserializeByteArraysFromBase64()
        {
            AddDatabaseJsonFile("EntityHasByteArray.json", """
                [
                    {
                        "Id": "1",
                        "ByteArray": "AAECAw=="
                    }
                ]
                """);

            using (var db = CreateDbContext())
            {
                var entity = db.EntitiesHaveByteArrays.Single();
                Assert.IsNotNull(entity.ByteArray);
                CollectionAssert.AreEqual(new byte[] { 0x00, 0x01, 0x02, 0x03 }, entity.ByteArray);
            }
        }

        protected override TestDbContext CreateDbContext(DbContextOptions<TestDbContext> options)
            => new(options);

        public sealed class TestDbContext(
            DbContextOptions<TestDbContext> options)
            : DbContext(options)
        {
            public DbSet<EntityHasByteArray> EntitiesHaveByteArrays { get; set; } = null!;
            public DbSet<EntityHasNullables> EntitiesHaveNullables { get; set; } = null!;
        }

        public class EntityHasByteArray
        {
            [Key]
            public int Id { get; set; }

            public byte[]? ByteArray { get; set; }
        }

        public class EntityHasNullables
        {
            [Key]
            public int Id { get; set; }

            public string? StringValue { get; set; }

            public int? IntValue { get; set; }
        }
    }
}
