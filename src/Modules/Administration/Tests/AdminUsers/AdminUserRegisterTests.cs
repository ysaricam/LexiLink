using LexiLink.Modules.Administration.Domain.AdminUsers;
using LexiLink.Modules.Administration.Domain.AdminUsers.Events;
using LexiLink.Modules.Administration.Domain.AdminUsers.Rules;
using LexiLink.Modules.Administration.Tests.SeedWork;

namespace LexiLink.Modules.Administration.Tests.AdminUsers;

[TestFixture]
public class AdminUserRegisterTests : TestBase
{
    private static readonly DateTime FixedRegisteredOn =
        new(2026, 5, 19, 9, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Register_Should_CreateActiveAdminUser_WithAdminRole()
    {
        var adminUser = AdminUser.Register(Email.Of("ops@lexilink.test"), FixedRegisteredOn);

        adminUser.Status.Should().Be(AdminUserStatus.Active);
        adminUser.Role.Should().Be(AdminRole.Admin);
        adminUser.Email.Value.Should().Be("ops@lexilink.test");
        adminUser.RegisteredOn.Should().Be(FixedRegisteredOn);
        adminUser.DisabledOn.Should().BeNull();
        adminUser.IsActive.Should().BeTrue();
    }

    [Test]
    public void Register_Should_PublishAdminUserRegisteredDomainEvent()
    {
        var adminUser = AdminUser.Register(Email.Of("ops@lexilink.test"), FixedRegisteredOn);

        var domainEvent = AssertPublishedDomainEvent<AdminUserRegisteredDomainEvent>(adminUser);
        domainEvent.AdminUserId.Should().Be(adminUser.Id.Value);
        domainEvent.Email.Should().Be("ops@lexilink.test");
        domainEvent.Role.Should().Be(AdminRole.Admin.Value);
    }

    [Test]
    public void Register_Should_NormalizeEmailToLowercase()
    {
        var adminUser = AdminUser.Register(Email.Of("Ops@LexiLink.Test"), FixedRegisteredOn);

        adminUser.Email.Value.Should().Be("ops@lexilink.test");
    }

    [Test]
    public void Register_Should_RejectEmptyEmail()
    {
        AssertBrokenRule<AdminUserEmailMustNotBeEmptyRule>(
            () => AdminUser.Register(Email.Of("   "), FixedRegisteredOn));
    }

    [Test]
    public void Register_Should_RejectInvalidEmailFormat()
    {
        AssertBrokenRule<AdminUserEmailMustBeValidFormatRule>(
            () => AdminUser.Register(Email.Of("not-an-email"), FixedRegisteredOn));
    }
}
