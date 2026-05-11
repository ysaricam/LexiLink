using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class DiscriminatorMustBeInRangeRule : IBusinessRule
{
    private readonly int _value;

    public DiscriminatorMustBeInRangeRule(int value)
    {
        _value = value;
    }

    public bool IsBroken() => _value < Discriminator.MinValue || _value > Discriminator.MaxValue;

    public string Message => $"Discriminator must be between {Discriminator.MinValue} and {Discriminator.MaxValue}.";
}
