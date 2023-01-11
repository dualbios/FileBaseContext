using FileBaseContextCore.Example.Data.Entities;
using kDg.FileBaseContext.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FileBaseContextCore.Example.Data
{
    public class Context : DbContext
    {
        public DbSet<ContentEntry> ContentEntries { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<GenericTest<int>> Generics { get; set; }
        public DbSet<Messurement> Messurements { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<SimpleEntity> SimpleEntities { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseFileBaseContextDatabase("my_local_db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}