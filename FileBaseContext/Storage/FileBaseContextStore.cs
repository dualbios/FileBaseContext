using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace kDg.FileBaseContext.Storage;

internal class FileBaseContextStore : IFileBaseContextStore
{
    private readonly IFileBaseContextTableFactory _tableFactory;
    private readonly bool _useNameMatching;
    private object _lock = new();
    private Dictionary<object, IFileBaseContextTable> _tables;

    public FileBaseContextStore(IFileBaseContextTableFactory FileBaseContextTableFactory, bool useNameMatching)
    {
        _tableFactory = FileBaseContextTableFactory;
        _useNameMatching = useNameMatching;
    }

    public bool Clear()
    {
        lock (_lock)
        {
            if (_tables == null)
            {
                return false;
            }

            _tables = null;

            return true;
        }
    }

    public bool EnsureCreated(IUpdateAdapterFactory updateAdapterFactory, IModel designModel)
    {
        lock (_lock)
        {
            var valuesSeeded = _tables == null;
            if (valuesSeeded)
            {
                _tables = CreateTables();

                var updateAdapter = updateAdapterFactory.CreateStandalone();
                var entries = new List<IUpdateEntry>();
                foreach (var entityType in updateAdapter.Model.GetEntityTypes())
                {
                    foreach (var targetSeed in entityType.GetSeedData())
                    {
                        var entry = updateAdapter.CreateEntry(targetSeed, entityType);
                        entry.EntityState = EntityState.Added;
                        entries.Add(entry);
                    }
                }

                ExecuteTransaction(entries);
            }

            return valuesSeeded;
        }
    }

    public int ExecuteTransaction(IList<IUpdateEntry> entries)
    {
        var rowsAffected = 0;

        lock (_lock)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entityType = entry.EntityType;

                var table = EnsureTable(entityType);

                if (entry.SharedIdentityEntry != null)
                {
                    if (entry.EntityState == EntityState.Deleted)
                    {
                        continue;
                    }

                    table.Delete(entry.SharedIdentityEntry);
                }

                switch (entry.EntityState)
                {
                    case EntityState.Added:
                        table.Create(entry);
                        break;

                    case EntityState.Deleted:
                        table.Delete(entry);
                        break;

                    case EntityState.Modified:
                        table.Update(entry);
                        break;
                }

                rowsAffected++;
            }
        }

        foreach (IFileBaseContextTable table in entries
                     .Select(x => EnsureTable(x.EntityType))
                     .GroupBy(x => x)
                     .Select(x => x.Key))
        {
            table.Save();
        }

        return rowsAffected;
    }

    public FileBaseContextIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(IProperty property)
    {
        lock (_lock)
        {
            var entityType = property.DeclaringEntityType;

            return EnsureTable(entityType).GetIntegerValueGenerator<TProperty>(
                property,
                entityType.GetDerivedTypesInclusive().Select(type => EnsureTable(type)).ToArray());
        }
    }

    public IReadOnlyList<FileBaseContextTableSnapshot> GetTables(IEntityType entityType)
    {
        var data = new List<FileBaseContextTableSnapshot>();
        lock (_lock)
        {
            foreach (var et in entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract()))
            {
                var table = EnsureTable(et);
                data.Add(new FileBaseContextTableSnapshot(et, table.SnapshotRows()));
            }
        }

        return data;
    }

    private static Dictionary<object, IFileBaseContextTable> CreateTables()
    {
        return new Dictionary<object, IFileBaseContextTable>();
    }

    private IFileBaseContextTable EnsureTable(IEntityType entityType)
    {
        _tables ??= CreateTables();

        IFileBaseContextTable baseTable = null;

        var entityTypes = entityType.GetAllBaseTypesInclusive();
        foreach (var currentEntityType in entityTypes)
        {
            var key = _useNameMatching ? (object)currentEntityType.Name : currentEntityType;
            if (!_tables.TryGetValue(key, out var table))
            {
                _tables.Add(key, table = _tableFactory.Create(currentEntityType, baseTable));
            }

            baseTable = table;
        }

        return _tables[_useNameMatching ? entityType.Name : entityType];
    }
}