using System.Text.RegularExpressions;
using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Rules;

public class CategoryLanguageMustBeValidFormatRule : IBusinessRule
{
    private static readonly Regex Pattern = new("^[a-z]{2}-[A-Z]{2}$", RegexOptions.Compiled);

    private readonly string? _language;

    public CategoryLanguageMustBeValidFormatRule(string? language)
    {
        _language = language;
    }

    public bool IsBroken() => string.IsNullOrEmpty(_language) || !Pattern.IsMatch(_language);

    public string Message => "Category language must be in BCP 47 short form (e.g. 'tr-TR', 'en-US').";
}
