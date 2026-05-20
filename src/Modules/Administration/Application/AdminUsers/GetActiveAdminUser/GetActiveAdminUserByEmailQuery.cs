using LexiLink.Modules.Administration.Application.Contracts;

namespace LexiLink.Modules.Administration.Application.AdminUsers.GetActiveAdminUser;

public sealed class GetActiveAdminUserByEmailQuery : QueryBase<AdminUserDto?>
{
    public string Email { get; }

    public GetActiveAdminUserByEmailQuery(string email)
    {
        Email = email;
    }
}
