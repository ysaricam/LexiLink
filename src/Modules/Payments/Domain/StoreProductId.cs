using LexiLink.Common.Domain;
using LexiLink.Modules.Payments.Domain.Rules;

namespace LexiLink.Modules.Payments.Domain;

public sealed class StoreProductId : ValueObject
{
    private const int MaxLength = 128;

    public string Value { get; }

    private StoreProductId()
    {
        Value = null!;
    }

    private StoreProductId(string value)
    {
        Value = value;
    }

    public static StoreProductId Of(string value)
    {
        CheckRule(new TextMustNotBeEmptyRule(value, nameof(StoreProductId)));
        CheckRule(new TextMustNotExceedMaxLengthRule(value.Trim(), MaxLength, nameof(StoreProductId)));

        return new StoreProductId(value.Trim());
    }

    public override string ToString() => Value;
}
