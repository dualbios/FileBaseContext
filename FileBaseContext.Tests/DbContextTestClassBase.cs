using kDg.FileBaseContext.Extensions;
using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions.TestingHelpers;

namespace FileBaseContext.Tests
{
    public abstract class DbContextTestClassBase<TContext>
        where TContext : DbContext
    {
        #region Lifecycle

        private MockFileSystem? _fileSystem;
        private DbContextOptions<TContext>? _dbContextOptions;

        protected MockFileSystem FileSystem
            => _fileSystem
            ?? throw new InvalidOperationException($"The {nameof(FileSystem)} has not been initialized.");

        protected DbContextOptions<TContext> DbContextOptions
            => _dbContextOptions
            ?? throw new InvalidOperationException($"The {nameof(DbContextOptions)} have not been initialized.");

        [TestInitialize]
        public virtual void SetUp()
        {
            _fileSystem = new();

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();

            // For unit tests, we are creating a new Options instance for each test case.
            // This is a no-no for production, and the efcore infrastructure detects it
            // and throws InvalidOperationException:
            //     An error was generated for warning 'Microsoft.EntityFrameworkCore.Infrastructure.ManyServiceProvidersCreatedWarning':
            //     More than twenty 'IServiceProvider' instances have been created for internal use by Entity Framework.
            //     This is commonly caused by injection of a new singleton service instance into every DbContext instance.
            // Explicitly disable provider caching for these unit tests.
            // For testing provider caching, this can be re-enabled and instead the warning can be suppressed:
            //optionsBuilder.ConfigureWarnings(warnings =>
            //{
            //    warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning);
            //});
            optionsBuilder.EnableServiceProviderCaching(false);

            ConfigureDbContextOptions(optionsBuilder);
            _dbContextOptions = optionsBuilder.Options;
        }

        [TestInitialize]
        public virtual void TearDown()
        {
            _dbContextOptions = null;
            _fileSystem = null;
        }

        protected virtual void ConfigureDbContextOptions(DbContextOptionsBuilder<TContext> options)
        {
            options.UseFileBaseContextDatabase(typeof(TContext).Name, null, services =>
            {
                services.AddMockFileSystem(FileSystem);
            });
        }

        protected TContext CreateDbContext()
        {
            return CreateDbContext(DbContextOptions);
        }

        protected abstract TContext CreateDbContext(DbContextOptions<TContext> options);

        #endregion

        #region File Helpers

        protected void AddDatabaseJsonFile(
            string fileName,
            [StringSyntax(StringSyntaxAttribute.Json)] string content)
        {
            FileSystem.AddFile(GetDatabaseFullFileName(fileName), content);
        }

        protected string? ReadDatabaseFileText(string fileName)
        {
            return FileSystem.File.ReadAllText(GetDatabaseFullFileName(fileName));
        }

        protected string GetDatabaseDirectoryFullName()
        {
            return FileSystem.Path.Combine(AppContext.BaseDirectory, GetDatabaseName());
        }

        protected string GetDatabaseFullFileName(string fileName)
        {
            return FileSystem.Path.Combine(GetDatabaseDirectoryFullName(), fileName);
        }

        protected virtual string GetDatabaseName()
        {
            var extension = DbContextOptions.GetExtension<FileBaseContextOptionsExtension>();
            return extension.Options.DatabaseName;
        }

        #endregion
    }
}
