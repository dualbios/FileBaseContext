using kDg.FileBaseContext.Infrastructure;
using kDg.FileBaseContext.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace kDg.FileBaseContext.Storage;

public class FileBaseContextTable<TKey> : IFileBaseContextTable
{
    private readonly IEntityType _entityType;
    private readonly IFileBaseContextFileManager _fileManager;
    private readonly IFileBaseContextScopedOptions _options;
    private readonly IKey _primaryKey;
    private Dictionary<TKey, object[]> _rows = new Dictionary<TKey, object[]>();
    private readonly ISerializer _serializer;
    private IPrincipalKeyValueFactory<TKey> _keyValueFactory = null;

    private Dictionary<int, IFileBaseContextIntegerValueGenerator> _integerGenerators;

    public FileBaseContextTable(IEntityType entityType, IServiceProvider serviceProvider, IFileBaseContextScopedOptions options)
    {
        _entityType = entityType;
        _fileManager = serviceProvider.GetService<IFileBaseContextFileManager>();
        _options = options;
        _primaryKey = entityType.FindPrimaryKey();
        _keyValueFactory = _primaryKey.GetPrincipalKeyValueFactory<TKey>();
        _serializer = new JsonDataSerializer(entityType, _keyValueFactory);

        _fileManager.Init(_options);

        Load();
    }

    public IEnumerable<object[]> Rows => _rows.Values;

    public void Create(IUpdateEntry entry)
    {
        var row = entry.EntityType.GetProperties()
            .Select(p => SnapshotValue(p, GetStructuralComparer(p), entry))
            .ToArray();

        _rows.Add(CreateKey(entry), row);
    }

    public void Load()
    {
        _rows = ConvertFromProvider(_fileManager.Load<TKey>(_entityType, _serializer));
    }

    public void Save()
    {
        _fileManager.Save(_entityType, ConvertToProvider(_rows), _serializer);
    }

    private static ValueComparer GetStructuralComparer(IProperty p)
    {
        return p.GetValueComparer();
        //return p.GetStructuralValueComparer() ?? p.FindTypeMapping()?.StructuralComparer;
    }

    private static object SnapshotValue(IProperty property, ValueComparer comparer, IUpdateEntry entry)
    {
        return SnapshotValue(comparer, entry.GetCurrentValue(property));
    }

    private static object SnapshotValue(ValueComparer comparer, object value)
    {
        return comparer == null ? value : comparer.Snapshot(value);
    }

    private Dictionary<TKey, object[]> ApplyValueConverter(Dictionary<TKey, object[]> list, Func<ValueConverter, Func<object, object>> conversionFunc)
    {
        var result = new Dictionary<TKey, object[]>(_keyValueFactory.EqualityComparer);
        var converters = _entityType.GetProperties().Select(p => p.GetValueConverter()).ToArray();
        foreach (var keyValuePair in list)
        {
            result[keyValuePair.Key] = keyValuePair.Value.Select((value, index) =>
            {
                var converter = converters[index];
                return converter == null ? value : conversionFunc(converter)(value);
            }).ToArray();
        }

        return result;
    }

    private Dictionary<TKey, object[]> ConvertToProvider(Dictionary<TKey, object[]> list)
    {
        return ApplyValueConverter(list, converter => converter.ConvertToProvider);
    }
    private Dictionary<TKey, object[]> ConvertFromProvider(Dictionary<TKey, object[]> list)
    {
        return ApplyValueConverter(list, converter => converter.ConvertFromProvider);
    }

    private TKey CreateKey(IUpdateEntry entry)
    {
        return _keyValueFactory.CreateFromCurrentValues(entry);
    }

    public virtual IReadOnlyList<object[]> SnapshotRows()
    {
        return new ReadOnlyCollection<object[]>(_rows.Values.ToList());
    }

    public void Delete(IUpdateEntry entry)
    {
        var key = CreateKey(entry);

        if (key.GetType().IsArray)
        {
            foreach (var item in _rows.Keys)
            {
                if (StructuralComparisons.StructuralEqualityComparer.Equals(item, key))
                {
                    key = item;
                    break;
                }
            }
        }

        if (_rows.TryGetValue(key, out object[] value))
        {
            var properties = entry.EntityType.GetProperties().ToList();
            var concurrencyConflicts = new Dictionary<IProperty, object>();

            for (var index = 0; index < properties.Count; index++)
            {
                IsConcurrencyConflict(entry, properties[index], value[index], concurrencyConflicts);
            }

            if (concurrencyConflicts.Count > 0)
            {
                ThrowUpdateConcurrencyException(entry, concurrencyConflicts);
            }

            _rows.Remove(key);
        }
        else
        {
            throw new DbUpdateConcurrencyException("UpdateConcurrencyException", new[] { entry });
        }
    }

    public void Update(IUpdateEntry entry)
    {
        var key = CreateKey(entry);

        if (_rows.ContainsKey(key))
        {
            var properties = entry.EntityType.GetProperties().ToList();
            var comparers = GetStructuralComparers(properties);
            var valueBuffer = new object[properties.Count];
            var concurrencyConflicts = new Dictionary<IProperty, object>();

            for (var index = 0; index < valueBuffer.Length; index++)
            {
                if (IsConcurrencyConflict(entry, properties[index], _rows[key][index], concurrencyConflicts))
                {
                    continue;
                }

                valueBuffer[index] = entry.IsModified(properties[index])
                    ? SnapshotValue(properties[index], comparers[index], entry)
                    : _rows[key][index];
            }

            if (concurrencyConflicts.Count > 0)
            {
                ThrowUpdateConcurrencyException(entry, concurrencyConflicts);
            }

            _rows[key] = valueBuffer;

            BumpValueGenerators(valueBuffer);
        }
        else
        {
            throw new DbUpdateConcurrencyException("UpdateConcurrencyException", new[] { entry });
        }
    }

    private void BumpValueGenerators(object[] row)
    {
        if (_integerGenerators != null)
        {
            foreach (var generator in _integerGenerators.Values)
            {
                generator.Bump(row);
            }
        }
    }

    private static List<ValueComparer> GetStructuralComparers(IEnumerable<IProperty> properties)
    {
        return properties.Select(GetStructuralComparer).ToList();
    }

    private static bool IsConcurrencyConflict(
        IUpdateEntry entry,
        IProperty property,
        object rowValue,
        Dictionary<IProperty, object> concurrencyConflicts)
    {
        if (property.IsConcurrencyToken
            && !StructuralComparisons.StructuralEqualityComparer.Equals(
                rowValue,
                entry.GetOriginalValue(property)))
        {
            concurrencyConflicts.Add(property, rowValue);

            return true;
        }

        return false;
    }

    protected virtual void ThrowUpdateConcurrencyException([NotNull] IUpdateEntry entry, [NotNull] Dictionary<IProperty, object> concurrencyConflicts)
    {
        throw new DbUpdateConcurrencyException(
            $"{entry.EntityType.DisplayName()},{concurrencyConflicts.Keys.Format()}",
            new[] { entry });
    }

    public virtual FileBaseContextIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(
        IProperty property,
        IReadOnlyList<IFileBaseContextTable> tables)
    {
        _integerGenerators ??= new Dictionary<int, IFileBaseContextIntegerValueGenerator>();

        var propertyIndex = property.GetIndex();
        if (!_integerGenerators.TryGetValue(propertyIndex, out var generator))
        {
            generator = new FileBaseContextIntegerValueGenerator<TProperty>(propertyIndex);
            _integerGenerators[propertyIndex] = generator;

            foreach (var table in tables)
            {
                foreach (var row in table.Rows)
                {
                    generator.Bump(row);
                }
            }
        }

        return (FileBaseContextIntegerValueGenerator<TProperty>)generator;
    }
}
