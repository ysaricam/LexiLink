using LexiLink.Modules.Administration.Application.Contracts;

namespace LexiLink.Modules.Administration.Application.AdminUsers.GetActiveAdminUser;

public sealed class GetActiveAdminUserByIdQuery : QueryBase<AdminUserDto?>
{
    public Guid AdminUserId { get; }

    public GetActiveAdminUserByIdQuery(Guid adminUserId)
    {
        AdminUserId = adminUserId;
    }
}
