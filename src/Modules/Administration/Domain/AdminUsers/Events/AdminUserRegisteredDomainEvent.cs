using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers.Events;

public class AdminUserRegisteredDomainEvent : DomainEvent
{
    public Guid AdminUserId { get; }
    public string Email { get; }
    public string Role { get; }

    public AdminUserRegisteredDomainEvent(Guid adminUserId, string email, string role)
    {
        AdminUserId = adminUserId;
        Email = email;
        Role = role;
    }
}
