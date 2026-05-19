using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers.Rules;

public class AdminUserEmailMustNotBeEmptyRule : IBusinessRule
{
    private readonly string _email;

    public AdminUserEmailMustNotBeEmptyRule(string email)
    {
        _email = email;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_email);

    public string Message => "Admin user email must not be empty.";
}
