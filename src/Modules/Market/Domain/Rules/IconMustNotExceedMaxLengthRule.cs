using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class IconMustNotExceedMaxLengthRule : IBusinessRule
{
    private readonly string? _icon;
    private readonly int _maxLength;

    internal IconMustNotExceedMaxLengthRule(string? icon, int maxLength)
    {
        _icon = icon;
        _maxLength = maxLength;
    }

    public bool IsBroken() => _icon is not null && _icon.Length > _maxLength;

    public string Message => $"Icon must not exceed {_maxLength} characters.";
}
