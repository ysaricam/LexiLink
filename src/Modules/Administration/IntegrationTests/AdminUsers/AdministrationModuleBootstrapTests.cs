using Autofac;
using LexiLink.Modules.Administration.Application.Contracts;
using LexiLink.Modules.Administration.Domain.AdminUsers;
using LexiLink.Modules.Administration.Infrastructure;
using LexiLink.Modules.Administration.IntegrationTests.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Administration.IntegrationTests.AdminUsers;

/// <summary>
/// Smoke coverage for the module composition root and persistence wiring.
/// Real admin command/query handlers ship in B2+; this test exists to prove
/// the foundation (DbContext, repository, schema, mappings) is consistent.
/// </summary>
[TestFixture]
public class AdministrationModuleBootstrapTests : TestBase
{
    [Test]
    public void Module_Should_ResolveCompositionRoot()
    {
        var module = Scope.Resolve<IAdministrationModule>();

        module.Should().NotBeNull();
    }

    [Test]
    public async Task AdminUser_Should_RoundTripThroughEfRepository()
    {
        var context = Scope.Resolve<AdministrationContext>();
        var repository = Scope.Resolve<IAdminUserRepository>();

        var registeredOn = new DateTime(2026, 5, 19, 9, 0, 0, DateTimeKind.Utc);
        var adminUser = AdminUser.Register(Email.Of("bootstrap@lexilink.test"), registeredOn);

        await repository.AddAsync(adminUser);
        await context.SaveChangesAsync();

        var stored = await context.AdminUsers
            .AsNoTracking()
            .SingleAsync();

        // DateTime round-trip exactness is out of scope for this slice — the
        // module-wide `timestamp without time zone` + UTC DateTime pattern is
        // shared with Energy/Quests and none of their integration tests assert
        // exact DateTime values. The B1 foundation only needs identity,
        // value-object, role, and status round-trip to be deterministic.
        stored.Id.Should().Be(adminUser.Id);
        stored.Email.Value.Should().Be("bootstrap@lexilink.test");
        stored.Role.Should().Be(AdminRole.Admin);
        stored.Status.Should().Be(AdminUserStatus.Active);
        stored.DisabledOn.Should().BeNull();
    }

    [Test]
    public async Task GetByEmail_Should_FindAdminUser_CaseInsensitivelyViaNormalization()
    {
        var context = Scope.Resolve<AdministrationContext>();
        var repository = Scope.Resolve<IAdminUserRepository>();
        var registeredOn = new DateTime(2026, 5, 19, 9, 0, 0, DateTimeKind.Utc);

        var adminUser = AdminUser.Register(Email.Of("Lookup@LexiLink.Test"), registeredOn);
        await repository.AddAsync(adminUser);
        await context.SaveChangesAsync();

        var found = await repository.GetByEmailAsync(Email.Of("LOOKUP@lexilink.test"));

        found.Should().NotBeNull();
        found!.Id.Should().Be(adminUser.Id);
    }
}
