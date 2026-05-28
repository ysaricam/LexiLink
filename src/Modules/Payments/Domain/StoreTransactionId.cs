using LexiLink.Common.Domain;
using LexiLink.Modules.Payments.Domain.Rules;

namespace LexiLink.Modules.Payments.Domain;

public sealed class StoreTransactionId : ValueObject
{
    private const int MaxLength = 256;

    public string Value { get; }

    private StoreTransactionId()
    {
        Value = null!;
    }

    private StoreTransactionId(string value)
    {
        Value = value;
    }

    public static StoreTransactionId Of(string value)
    {
        CheckRule(new TextMustNotBeEmptyRule(value, nameof(StoreTransactionId)));
        CheckRule(new TextMustNotExceedMaxLengthRule(value.Trim(), MaxLength, nameof(StoreTransactionId)));

        return new StoreTransactionId(value.Trim());
    }

    public override string ToString() => Value;
}
