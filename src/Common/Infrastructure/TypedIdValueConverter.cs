using LexiLink.Common.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LexiLink.Common.Infrastructure;

public class TypedIdValueConverter<TTypedIdValue> : ValueConverter<TTypedIdValue, Guid>
    where TTypedIdValue : TypedIdValueBase
{
    public TypedIdValueConverter() : this(null)
    {
    }

    public TypedIdValueConverter(ConverterMappingHints? mappingHints)
        : base(id => id.Value, value => Create(value), mappingHints)
    {
    }

    private static TTypedIdValue Create(Guid id) =>
        (TTypedIdValue)Activator.CreateInstance(typeof(TTypedIdValue), id)!;
}
