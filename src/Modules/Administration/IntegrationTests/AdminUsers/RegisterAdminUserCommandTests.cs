using Autofac;
using LexiLink.Modules.Administration.Application.AdminUsers.RegisterAdminUser;
using LexiLink.Modules.Administration.Application.Contracts;
using LexiLink.Modules.Administration.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Administration.IntegrationTests.AdminUsers;

[TestFixture]
public class RegisterAdminUserCommandTests : TestBase
{
    [Test]
    public async Task RegisterAdminUser_Should_PersistAndReturnNewId()
    {
        var module = Scope.Resolve<IAdministrationModule>();

        var adminId = await module.ExecuteCommandAsync(
            new RegisterAdminUserCommand("first@lexilink.test"));

        adminId.Should().NotBeEmpty();

        var stored = await QuerySingleOrDefaultAsync<AdminUserRow>("""
            SELECT "Id" AS "Id", "Email" AS "Email", "Status" AS "Status"
            FROM "administration"."AdminUsers"
            WHERE "Id" = @AdminId;
            """,
            new { AdminId = adminId });

        stored.Should().NotBeNull();
        stored!.Email.Should().Be("first@lexilink.test");
        stored.Status.Should().Be("Active");
    }

    [Test]
    public async Task RegisterAdminUser_Should_BeIdempotentOnEmail()
    {
        var module = Scope.Resolve<IAdministrationModule>();

        var firstId = await module.ExecuteCommandAsync(
            new RegisterAdminUserCommand("dup@lexilink.test"));
        var secondId = await module.ExecuteCommandAsync(
            new RegisterAdminUserCommand("DUP@LexiLink.Test"));

        secondId.Should().Be(firstId);

        var count = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int
            FROM "administration"."AdminUsers"
            WHERE "Email" = 'dup@lexilink.test';
            """);

        count.Should().Be(1);
    }

    [Test]
    public async Task RegisterAdminUser_Should_WriteOutboxMessage_And_PublishIntegrationEventOnProcess()
    {
        var module = Scope.Resolve<IAdministrationModule>();

        var adminId = await module.ExecuteCommandAsync(
            new RegisterAdminUserCommand("outbox@lexilink.test"));

        var pendingCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int
            FROM "administration"."OutboxMessages"
            WHERE "ProcessedDate" IS NULL;
            """);
        pendingCount.Should().Be(1);

        await ProcessOutboxAsync();

        AdminUserRegisteredEvents.Captured.Should().HaveCount(1);
        var received = AdminUserRegisteredEvents.Captured[0];
        received.AdminUserId.Should().Be(adminId);
        received.Email.Should().Be("outbox@lexilink.test");
        received.Role.Should().Be("Admin");

        var processedCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int
            FROM "administration"."OutboxMessages"
            WHERE "ProcessedDate" IS NOT NULL;
            """);
        processedCount.Should().Be(1);
    }

    private sealed record AdminUserRow(Guid Id, string Email, string Status);
}
