using LexiLink.API.CrossModule;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Admin;
using NSubstitute;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class AdminAuthorizationContextTests
{
    [Test]
    public void IsAdmin_Should_BeFalse_WhenExecutionContextIsUnavailable()
    {
        var accessor = Substitute.For<IExecutionContextAccessor>();
        accessor.IsAvailable.Returns(false);
        var sut = new AdminAuthorizationContext(accessor);

        sut.IsAdmin.Should().BeFalse();
        sut.AdminUserId.Should().BeNull();
    }

    [Test]
    public void IsAdmin_Should_BeFalse_WhenPrincipalIsPlayer()
    {
        var accessor = Substitute.For<IExecutionContextAccessor>();
        accessor.IsAvailable.Returns(true);
        accessor.IsAdmin.Returns(false);
        accessor.AdminUserId.Returns((Guid?)null);
        var sut = new AdminAuthorizationContext(accessor);

        sut.IsAdmin.Should().BeFalse();
        sut.AdminUserId.Should().BeNull();
    }

    [Test]
    public void IsAdmin_Should_BeTrue_AndExposeId_WhenAdminClaimsPresent()
    {
        var adminId = Guid.NewGuid();
        var accessor = Substitute.For<IExecutionContextAccessor>();
        accessor.IsAvailable.Returns(true);
        accessor.IsAdmin.Returns(true);
        accessor.AdminUserId.Returns(adminId);
        var sut = new AdminAuthorizationContext(accessor);

        sut.IsAdmin.Should().BeTrue();
        sut.AdminUserId.Should().Be(adminId);
    }

    [Test]
    public void RequireAdminUserId_Should_ReturnId_WhenAuthorized()
    {
        var adminId = Guid.NewGuid();
        var accessor = Substitute.For<IExecutionContextAccessor>();
        accessor.IsAvailable.Returns(true);
        accessor.IsAdmin.Returns(true);
        accessor.AdminUserId.Returns(adminId);
        var sut = new AdminAuthorizationContext(accessor);

        sut.RequireAdminUserId().Should().Be(adminId);
    }

    [Test]
    public void RequireAdminUserId_Should_Throw_WhenPlayerPrincipal()
    {
        var accessor = Substitute.For<IExecutionContextAccessor>();
        accessor.IsAvailable.Returns(true);
        accessor.IsAdmin.Returns(false);
        var sut = new AdminAuthorizationContext(accessor);

        var act = () => sut.RequireAdminUserId();

        act.Should().Throw<AdminAuthorizationException>();
    }

    [Test]
    public void EnsureAuthorized_Should_Throw_WhenAdminIdMissing()
    {
        // Defensive: IsAdmin=true but AdminUserId=null shouldn't be possible
        // in practice (the auth handler stamps them together), but the
        // context must still refuse to claim authorization without an id.
        var accessor = Substitute.For<IExecutionContextAccessor>();
        accessor.IsAvailable.Returns(true);
        accessor.IsAdmin.Returns(true);
        accessor.AdminUserId.Returns((Guid?)null);
        var sut = new AdminAuthorizationContext(accessor);

        var act = () => sut.EnsureAuthorized();

        act.Should().Throw<AdminAuthorizationException>();
    }
}
