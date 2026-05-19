using LexiLink.Common.Domain;
using LexiLink.Modules.Administration.Domain.AdminUsers.Events;
using LexiLink.Modules.Administration.Domain.AdminUsers.Rules;

namespace LexiLink.Modules.Administration.Domain.AdminUsers;

public class AdminUser : Entity, IAggregateRoot
{
    public AdminUserId Id { get; private set; }

    private Email _email = null!;
    private AdminRole _role = null!;
    private AdminUserStatus _status;
    private DateTime _registeredOn;
    private DateTime? _disabledOn;

    public Email Email => _email;
    public AdminRole Role => _role;
    public AdminUserStatus Status => _status;
    public DateTime RegisteredOn => _registeredOn;
    public DateTime? DisabledOn => _disabledOn;

    private AdminUser()
    {
        Id = null!;
    }

    private AdminUser(AdminUserId id, Email email, AdminRole role, DateTime registeredOn)
    {
        Id = id;
        _email = email;
        _role = role;
        _status = AdminUserStatus.Active;
        _registeredOn = registeredOn;
        _disabledOn = null;

        AddDomainEvent(new AdminUserRegisteredDomainEvent(id.Value, email.Value, role.Value));
    }

    public static AdminUser Register(Email email, DateTime registeredOn)
    {
        return new AdminUser(new AdminUserId(Guid.NewGuid()), email, AdminRole.Admin, registeredOn);
    }

    public void Disable(DateTime now)
    {
        CheckRule(new AdminUserMustBeActiveToDisableRule(_status));

        _status = AdminUserStatus.Disabled;
        _disabledOn = now;

        AddDomainEvent(new AdminUserDisabledDomainEvent(Id.Value));
    }

    public bool IsActive => _status == AdminUserStatus.Active;
}
