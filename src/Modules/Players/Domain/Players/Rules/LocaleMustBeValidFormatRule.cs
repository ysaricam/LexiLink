using System.Text.RegularExpressions;
using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class LocaleMustBeValidFormatRule : IBusinessRule
{
    private static readonly Regex Pattern = new("^[a-z]{2}-[A-Z]{2}$", RegexOptions.Compiled);

    private readonly string? _locale;

    public LocaleMustBeValidFormatRule(string? locale)
    {
        _locale = locale;
    }

    public bool IsBroken() => string.IsNullOrEmpty(_locale) || !Pattern.IsMatch(_locale);

    public string Message => "Locale must be in BCP 47 short form (e.g. 'tr-TR', 'en-US').";
}
