using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class DisplayNameMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _displayName;

    public DisplayNameMustNotBeEmptyRule(string? displayName)
    {
        _displayName = displayName;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_displayName);

    public string Message => "Player display name cannot be empty.";
}
