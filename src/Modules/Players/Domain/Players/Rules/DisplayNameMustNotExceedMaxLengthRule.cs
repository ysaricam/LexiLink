using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class DisplayNameMustNotExceedMaxLengthRule : IBusinessRule
{
    public const int MaxLength = 32;

    private readonly string? _displayName;

    public DisplayNameMustNotExceedMaxLengthRule(string? displayName)
    {
        _displayName = displayName;
    }

    public bool IsBroken() => _displayName is not null && _displayName.Length > MaxLength;

    public string Message => $"Player display name cannot exceed {MaxLength} characters.";
}
