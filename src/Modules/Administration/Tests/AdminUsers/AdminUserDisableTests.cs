using LexiLink.Modules.Administration.Domain.AdminUsers;
using LexiLink.Modules.Administration.Domain.AdminUsers.Events;
using LexiLink.Modules.Administration.Domain.AdminUsers.Rules;
using LexiLink.Modules.Administration.Tests.SeedWork;

namespace LexiLink.Modules.Administration.Tests.AdminUsers;

[TestFixture]
public class AdminUserDisableTests : TestBase
{
    private static readonly DateTime FixedRegisteredOn =
        new(2026, 5, 19, 9, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime FixedDisabledOn =
        new(2026, 5, 19, 10, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Disable_Should_FlipStatusAndStampDisabledOn()
    {
        var adminUser = AdminUser.Register(Email.Of("ops@lexilink.test"), FixedRegisteredOn);

        adminUser.Disable(FixedDisabledOn);

        adminUser.Status.Should().Be(AdminUserStatus.Disabled);
        adminUser.DisabledOn.Should().Be(FixedDisabledOn);
        adminUser.IsActive.Should().BeFalse();
    }

    [Test]
    public void Disable_Should_PublishAdminUserDisabledDomainEvent()
    {
        var adminUser = AdminUser.Register(Email.Of("ops@lexilink.test"), FixedRegisteredOn);

        adminUser.Disable(FixedDisabledOn);

        var domainEvent = AssertPublishedDomainEvent<AdminUserDisabledDomainEvent>(adminUser);
        domainEvent.AdminUserId.Should().Be(adminUser.Id.Value);
    }

    [Test]
    public void Disable_Should_BeIdempotentlyRejected_WhenAlreadyDisabled()
    {
        var adminUser = AdminUser.Register(Email.Of("ops@lexilink.test"), FixedRegisteredOn);
        adminUser.Disable(FixedDisabledOn);

        AssertBrokenRule<AdminUserMustBeActiveToDisableRule>(
            () => adminUser.Disable(FixedDisabledOn.AddHours(1)));
    }
}
