using LexiLink.Modules.Administration.Application.Contracts;

namespace LexiLink.Modules.Administration.Application.AdminUsers.RegisterAdminUser;

public class RegisterAdminUserCommand : CommandBase<Guid>
{
    public string Email { get; }

    public RegisterAdminUserCommand(string email)
    {
        Email = email;
    }
}
