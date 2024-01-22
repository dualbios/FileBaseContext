using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Diagnostics.CodeAnalysis;

namespace kDg.FileBaseContext.Infrastructure
{
    internal sealed class FileContextBaseValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        IFileBaseContextDatabase database)
        : ValueGeneratorSelector(dependencies)
    {
        private readonly IFileBaseContextStore _store = database.Store;

        public override ValueGenerator Select(IProperty property, ITypeBase typeBase)
        {
            if (property.GetValueGeneratorFactory() == null
                && property.ClrType.IsInteger()
                && property.ClrType.UnwrapNullableType() != typeof(char))
            {
                var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();
                if (FindGenerator(property, type, out var generator))
                {
                    return generator;
                }

                throw new ArgumentException(
                    $"Integer type {type} has no supported value generator.",
                    nameof(property));
            }

            return base.Select(property, typeBase);
        }

        protected override ValueGenerator FindForType(IProperty property, ITypeBase typeBase, Type clrType)
        {
            if (property.ValueGenerated != ValueGenerated.Never
                && FindGenerator(property, clrType, out var generator))
            {
                return generator;
            }

            return base.FindForType(property, typeBase, clrType);
        }

        private bool FindGenerator(
            IProperty property,
            Type type,
            [MaybeNullWhen(false)] out ValueGenerator valueGenerator)
        {
            if (type == typeof(long))
            {
                valueGenerator = _store.GetIntegerValueGenerator<long>(property);
                return true;
            }

            if (type == typeof(int))
            {
                valueGenerator = _store.GetIntegerValueGenerator<int>(property);
                return true;
            }

            if (type == typeof(short))
            {
                valueGenerator = _store.GetIntegerValueGenerator<short>(property);
                return true;
            }

            if (type == typeof(byte))
            {
                valueGenerator = _store.GetIntegerValueGenerator<byte>(property);
                return true;
            }

            if (type == typeof(ulong))
            {
                valueGenerator = _store.GetIntegerValueGenerator<ulong>(property);
                return true;
            }

            if (type == typeof(uint))
            {
                valueGenerator = _store.GetIntegerValueGenerator<uint>(property);
                return true;
            }

            if (type == typeof(ushort))
            {
                valueGenerator = _store.GetIntegerValueGenerator<ushort>(property);
                return true;
            }

            if (type == typeof(sbyte))
            {
                valueGenerator = _store.GetIntegerValueGenerator<sbyte>(property);
                return true;
            }

            valueGenerator = null;
            return false;
        }
    }
}
