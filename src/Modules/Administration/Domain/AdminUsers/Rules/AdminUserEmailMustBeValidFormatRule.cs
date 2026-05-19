using System.Text.RegularExpressions;
using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers.Rules;

public class AdminUserEmailMustBeValidFormatRule : IBusinessRule
{
    private readonly string _email;
    private readonly Regex _pattern;

    public AdminUserEmailMustBeValidFormatRule(string email, Regex pattern)
    {
        _email = email;
        _pattern = pattern;
    }

    public bool IsBroken() =>
        !string.IsNullOrWhiteSpace(_email) && !_pattern.IsMatch(_email);

    public string Message => "Admin user email must be a valid email address.";
}
