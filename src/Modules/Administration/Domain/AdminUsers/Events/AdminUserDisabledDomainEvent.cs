using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers.Events;

public class AdminUserDisabledDomainEvent : DomainEvent
{
    public Guid AdminUserId { get; }

    public AdminUserDisabledDomainEvent(Guid adminUserId)
    {
        AdminUserId = adminUserId;
    }
}
