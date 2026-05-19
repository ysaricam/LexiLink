using LexiLink.Common.Application.Events;
using LexiLink.Modules.Administration.Domain.AdminUsers.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Administration.Infrastructure.Outbox.DomainEventNotifications;

public class AdminUserRegisteredDomainEventNotification : IDomainEventNotification<AdminUserRegisteredDomainEvent>
{
    [JsonIgnore]
    public AdminUserRegisteredDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid AdminUserId { get; private set; }
    public string Email { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    public AdminUserRegisteredDomainEventNotification(AdminUserRegisteredDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        AdminUserId = domainEvent.AdminUserId;
        Email = domainEvent.Email;
        Role = domainEvent.Role;
    }

    [JsonConstructor]
    private AdminUserRegisteredDomainEventNotification()
    {
    }
}
