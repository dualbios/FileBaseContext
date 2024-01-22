using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileBaseContext.Tests
{
    [TestClass]
    public sealed class ValueGenerationTests
        : DbContextTestClassBase<ValueGenerationTests.TestDbContext>
    {
        [TestMethod]
        public void AddEntitiesWithIdentity_EmptyDatabase()
        {
            // Arrange

            // Act
            using (var testDb = CreateDbContext())
            {
                testDb.Add(new EntityHasIdentity() { ExpectedId = 1 });
                testDb.Add(new EntityHasIdentity() { ExpectedId = 2 });
                testDb.Add(new EntityHasIdentity() { ExpectedId = 3 });
                testDb.SaveChanges();
            }

            // Assert
            using (var assertDb = CreateDbContext())
            {
                var entities = assertDb.EntitiesHaveIdentity.ToList();
                Assert.AreEqual(3, entities.Count);
                Assert.AreEqual(0, entities.Count(o => o.ExpectedId == 0));
                foreach (var entity in entities)
                    Assert.AreEqual(entity.ExpectedId, entity.Id);
            }
        }

        [TestMethod]
        public void AddEntitiesWithIdentity_NonEmptyDatabase()
        {
            // Arrange
            {
                AddDatabaseJsonFile("EntityHasIdentity.json", """
                    [
                        { "Id": 1, "ExpectedId": 1 },
                        { "Id": 2, "ExpectedId": 2 },
                        { "Id": 3, "ExpectedId": 3 }
                    ]
                    """);
            }

            // Act
            using (var testDb = CreateDbContext())
            {
                testDb.Add(new EntityHasIdentity() { ExpectedId = 4 });
                testDb.Add(new EntityHasIdentity() { ExpectedId = 5 });
                testDb.Add(new EntityHasIdentity() { ExpectedId = 6 });
                testDb.SaveChanges();
            }

            // Assert
            using (var assertDb = CreateDbContext())
            {
                var entities = assertDb.EntitiesHaveIdentity.ToList();
                Assert.AreEqual(6, entities.Count);
                Assert.AreEqual(0, entities.Count(o => o.ExpectedId == 0));
                foreach (var entity in entities)
                    Assert.AreEqual(entity.ExpectedId, entity.Id);
            }
        }

        [TestMethod]
        public void AddEntitiesWithIdentity_TwoEntityTypes()
        {
            // Arrange

            // Act
            using (var testDb = CreateDbContext())
            {
                testDb.Add(new EntityHasIdentity() { ExpectedId = 1 });
                testDb.Add(new EntityHasIdentity() { ExpectedId = 2 });
                testDb.Add(new EntityHasIdentity2() { ExpectedId = 1 });
                testDb.Add(new EntityHasIdentity2() { ExpectedId = 2 });
                testDb.SaveChanges();
            }

            // Assert
            using (var assertDb = CreateDbContext())
            {
                var entity1s = assertDb.EntitiesHaveIdentity.ToList();
                Assert.AreEqual(2, entity1s.Count);
                Assert.AreEqual(0, entity1s.Count(o => o.ExpectedId == 0));
                foreach (var entity in entity1s)
                    Assert.AreEqual(entity.ExpectedId, entity.Id);

                var entity2s = assertDb.EntitiesHaveIdentity2.ToList();
                Assert.AreEqual(2, entity2s.Count);
                Assert.AreEqual(0, entity2s.Count(o => o.ExpectedId == 0));
                foreach (var entity in entity2s)
                    Assert.AreEqual(entity.ExpectedId, entity.Id);
            }
        }

        [TestMethod]
        public void AddEntitiesWithValueGeneratorOnAdd()
        {
            // Arrange

            // Act
            using (var testDb = CreateDbContext())
            {
                testDb.Add(new EntityHasValueGenerator() { IdSource = 1 });
                testDb.Add(new EntityHasValueGenerator() { IdSource = 2 });
                testDb.Add(new EntityHasValueGenerator() { IdSource = 3 });
                testDb.SaveChanges();
            }

            // Assert
            using (var assertDb = CreateDbContext())
            {
                var entities = assertDb.EntitiesHaveValueGenerator.ToList();
                Assert.AreEqual(3, entities.Count);
                CollectionAssert.AreEqual(
                    (string[])["[0001]", "[0002]", "[0003]"],
                    entities.Select(o => o.Id).ToList());
            }
        }

        protected override TestDbContext CreateDbContext(DbContextOptions<TestDbContext> options)
            => new(options);

        public sealed class TestDbContext(
            DbContextOptions<TestDbContext> options)
            : DbContext(options)
        {
            public DbSet<EntityHasIdentity> EntitiesHaveIdentity { get; set; } = null!;
            public DbSet<EntityHasIdentity2> EntitiesHaveIdentity2 { get; set; } = null!;
            public DbSet<EntityHasValueGenerator> EntitiesHaveValueGenerator { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<EntityHasValueGenerator>()
                    .Property(o => o.Id)
                    .HasValueGenerator<TestValueGenerator>()
                    .ValueGeneratedOnAdd();
            }
        }

        public class EntityHasIdentity
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public int ExpectedId { get; set; }
        }

        public class EntityHasIdentity2
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public int ExpectedId { get; set; }
        }

        public class EntityHasValueGenerator
        {
            [Key]
            public string Id { get; set; } = null!;

            public int IdSource { get; set; }
        }

        private sealed class TestValueGenerator
            : ValueGenerator<string>
        {
            public override bool GeneratesTemporaryValues => false;

            public override string Next(EntityEntry entry)
            {
                var entity = (EntityHasValueGenerator)entry.Entity;
                return $"[{entity.IdSource:0000}]";
            }
        }
    }
}
