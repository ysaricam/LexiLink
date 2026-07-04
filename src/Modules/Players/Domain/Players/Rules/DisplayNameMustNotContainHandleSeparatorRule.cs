using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class DisplayNameMustNotContainHandleSeparatorRule : IBusinessRule
{
    private readonly string? _displayName;

    public DisplayNameMustNotContainHandleSeparatorRule(string? displayName)
    {
        _displayName = displayName;
    }

    public bool IsBroken() => _displayName?.Contains('#') == true;

    public string Message => "Player display name cannot contain '#'.";
}
