using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers.Rules;

public class AdminUserMustBeActiveToDisableRule : IBusinessRule
{
    private readonly AdminUserStatus _status;

    public AdminUserMustBeActiveToDisableRule(AdminUserStatus status)
    {
        _status = status;
    }

    public bool IsBroken() => _status != AdminUserStatus.Active;

    public string Message => "Admin user must be active to be disabled.";
}
