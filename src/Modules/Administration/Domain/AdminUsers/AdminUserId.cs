using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers;

public class AdminUserId : TypedIdValueBase
{
    public AdminUserId(Guid value) : base(value)
    {
    }
}
