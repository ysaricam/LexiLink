using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Reset.Application.Admin.GrantBonusReset;
using LexiLink.Modules.Reset.Application.Admin.ResetPlayerReset;
using LexiLink.Modules.Reset.Application.Admin.SetPlayerReset;
using LexiLink.Modules.Reset.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

namespace LexiLink.Modules.Reset.IntegrationTests.PlayerResetInventories;

[TestFixture]
public sealed class ResetAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    [Test]
    public async Task NonAdmin_Should_Be_RejectedWith_AdminAuthorizationException()
    {
        AdminContext.Logout();
        var playerId = await ProvisionPlayerWithInventoryAsync("device-reset-admin-not-allowed");

        var act = async () => await ExecuteCommandAsync(new SetPlayerResetCommand(playerId, 1));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task SetPlayerReset_AsAdmin_SnapsBalance_AndAuditsRow()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-reset-admin-set");

        await ExecuteCommandAsync(new SetPlayerResetCommand(playerId, 7));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "reset"."PlayerResetInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(7);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(SetPlayerResetCommand),
            TargetId = playerId.ToString()
        });

        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Reset.PlayerResetInventory");
    }

    [Test]
    public async Task GrantBonusReset_AsAdmin_AccumulatesBalance_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-reset-admin-grant");

        await ExecuteCommandAsync(new GrantBonusResetCommand(playerId, 5));
        await ExecuteCommandAsync(new GrantBonusResetCommand(playerId, 3));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "reset"."PlayerResetInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(8, "no max cap — resets accumulate freely");

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(GrantBonusResetCommand),
            TargetId = playerId.ToString()
        });
        auditCount.Should().Be(2);
    }

    [Test]
    public async Task ResetPlayerReset_AsAdmin_SnapsBalanceToZero_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-reset-admin-reset");

        await ExecuteCommandAsync(new SetPlayerResetCommand(playerId, 12));
        await ExecuteCommandAsync(new ResetPlayerResetCommand(playerId));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "reset"."PlayerResetInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(0);

        await ProcessOutboxAsync();

        var resetAuditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(ResetPlayerResetCommand),
            TargetId = playerId.ToString()
        });
        resetAuditCount.Should().Be(1);
    }

    private async Task<Guid> ProvisionPlayerWithInventoryAsync(string deviceId)
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand(deviceId, "Test", "en-US"));
        await ProcessOutboxAsync();
        return playerId;
    }

    private sealed record AdminActionRow(Guid AdminUserId, string ActionType, string TargetType, string? TargetId);
}
