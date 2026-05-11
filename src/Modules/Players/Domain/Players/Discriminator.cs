using LexiLink.Common.Domain;
using LexiLink.Modules.Players.Domain.Players.Rules;

namespace LexiLink.Modules.Players.Domain.Players;

public sealed class Discriminator : ValueObject
{
    public const int MinValue = 1;
    public const int MaxValue = 9999;

    public int Value { get; }

    private Discriminator() { }

    private Discriminator(int value)
    {
        Value = value;
    }

    public static Discriminator Of(int value)
    {
        CheckRule(new DiscriminatorMustBeInRangeRule(value));
        return new Discriminator(value);
    }

    public override string ToString() => Value.ToString("D4");
}
