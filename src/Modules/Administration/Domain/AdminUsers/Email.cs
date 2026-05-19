using System.Text.RegularExpressions;
using LexiLink.Common.Domain;
using LexiLink.Modules.Administration.Domain.AdminUsers.Rules;

namespace LexiLink.Modules.Administration.Domain.AdminUsers;

public class Email : ValueObject
{
    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; } = string.Empty;

    private Email() { }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Of(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        CheckRule(new AdminUserEmailMustNotBeEmptyRule(normalized));
        CheckRule(new AdminUserEmailMustBeValidFormatRule(normalized, Pattern));

        return new Email(normalized);
    }

    public override string ToString() => Value;
}
