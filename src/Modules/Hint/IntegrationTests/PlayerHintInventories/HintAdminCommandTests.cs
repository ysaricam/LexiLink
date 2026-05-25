using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Hint.Application.Admin.GrantBonusHint;
using LexiLink.Modules.Hint.Application.Admin.ResetPlayerHint;
using LexiLink.Modules.Hint.Application.Admin.SetPlayerHint;
using LexiLink.Modules.Hint.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

namespace LexiLink.Modules.Hint.IntegrationTests.PlayerHintInventories;

[TestFixture]
public sealed class HintAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    [Test]
    public async Task NonAdmin_Should_Be_RejectedWith_AdminAuthorizationException()
    {
        AdminContext.Logout();
        var playerId = await ProvisionPlayerWithInventoryAsync("device-hint-admin-not-allowed");

        var act = async () => await ExecuteCommandAsync(new SetPlayerHintCommand(playerId, 1));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task SetPlayerHint_AsAdmin_SnapsBalance_AndAuditsRow()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-hint-admin-set");

        await ExecuteCommandAsync(new SetPlayerHintCommand(playerId, 7));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "hint"."PlayerHintInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(7);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(SetPlayerHintCommand),
            TargetId = playerId.ToString()
        });

        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Hint.PlayerHintInventory");
    }

    [Test]
    public async Task GrantBonusHint_AsAdmin_AccumulatesBalance_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-hint-admin-grant");

        await ExecuteCommandAsync(new GrantBonusHintCommand(playerId, 5));
        await ExecuteCommandAsync(new GrantBonusHintCommand(playerId, 3));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "hint"."PlayerHintInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(8, "no max cap — hints accumulate freely");

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(GrantBonusHintCommand),
            TargetId = playerId.ToString()
        });
        auditCount.Should().Be(2);
    }

    [Test]
    public async Task ResetPlayerHint_AsAdmin_SnapsBalanceToZero_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-hint-admin-reset");

        await ExecuteCommandAsync(new SetPlayerHintCommand(playerId, 12));
        await ExecuteCommandAsync(new ResetPlayerHintCommand(playerId));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "hint"."PlayerHintInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(0);

        await ProcessOutboxAsync();

        var resetAuditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(ResetPlayerHintCommand),
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
