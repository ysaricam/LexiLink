using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class IdempotencyKeyMustBeUniqueForPlayerRule : IBusinessRule
{
    private readonly bool _alreadyExists;

    internal IdempotencyKeyMustBeUniqueForPlayerRule(bool alreadyExists)
    {
        _alreadyExists = alreadyExists;
    }

    public bool IsBroken() => _alreadyExists;

    public string Message => "Idempotency key must be unique for the player.";
}
