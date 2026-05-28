using LexiLink.Common.Domain;
using LexiLink.Modules.Payments.Domain.Rules;

namespace LexiLink.Modules.Payments.Domain;

public sealed class PurchaseToken : ValueObject
{
    private const int MaxLength = 2048;

    public string Value { get; }

    private PurchaseToken()
    {
        Value = null!;
    }

    private PurchaseToken(string value)
    {
        Value = value;
    }

    public static PurchaseToken Of(string value)
    {
        CheckRule(new TextMustNotBeEmptyRule(value, nameof(PurchaseToken)));
        CheckRule(new TextMustNotExceedMaxLengthRule(value.Trim(), MaxLength, nameof(PurchaseToken)));

        return new PurchaseToken(value.Trim());
    }

    public override string ToString() => Value;
}
