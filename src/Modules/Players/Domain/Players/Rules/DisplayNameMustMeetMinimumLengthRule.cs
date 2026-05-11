using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class DisplayNameMustMeetMinimumLengthRule : IBusinessRule
{
    public const int MinLength = 2;

    private readonly string? _displayName;

    public DisplayNameMustMeetMinimumLengthRule(string? displayName)
    {
        _displayName = displayName;
    }

    public bool IsBroken() => _displayName is null || _displayName.Trim().Length < MinLength;

    public string Message => $"Player display name must be at least {MinLength} characters.";
}
