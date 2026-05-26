using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Undo.Application.Admin.GrantBonusUndo;
using LexiLink.Modules.Undo.Application.Admin.ResetPlayerUndo;
using LexiLink.Modules.Undo.Application.Admin.SetPlayerUndo;
using LexiLink.Modules.Undo.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

namespace LexiLink.Modules.Undo.IntegrationTests.PlayerUndoInventories;

[TestFixture]
public sealed class UndoAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    [Test]
    public async Task NonAdmin_Should_Be_RejectedWith_AdminAuthorizationException()
    {
        AdminContext.Logout();
        var playerId = await ProvisionPlayerWithInventoryAsync("device-undo-admin-not-allowed");

        var act = async () => await ExecuteCommandAsync(new SetPlayerUndoCommand(playerId, 1));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task SetPlayerUndo_AsAdmin_SnapsBalance_AndAuditsRow()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-undo-admin-set");

        await ExecuteCommandAsync(new SetPlayerUndoCommand(playerId, 7));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "undo"."PlayerUndoInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(7);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(SetPlayerUndoCommand),
            TargetId = playerId.ToString()
        });

        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Undo.PlayerUndoInventory");
    }

    [Test]
    public async Task GrantBonusUndo_AsAdmin_AccumulatesBalance_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-undo-admin-grant");

        await ExecuteCommandAsync(new GrantBonusUndoCommand(playerId, 5));
        await ExecuteCommandAsync(new GrantBonusUndoCommand(playerId, 3));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "undo"."PlayerUndoInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(8, "no max cap — undos accumulate freely");

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(GrantBonusUndoCommand),
            TargetId = playerId.ToString()
        });
        auditCount.Should().Be(2);
    }

    [Test]
    public async Task ResetPlayerUndo_AsAdmin_SnapsBalanceToZero_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-undo-admin-reset");

        await ExecuteCommandAsync(new SetPlayerUndoCommand(playerId, 12));
        await ExecuteCommandAsync(new ResetPlayerUndoCommand(playerId));

        var balance = await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "undo"."PlayerUndoInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });
        balance.Should().Be(0);

        await ProcessOutboxAsync();

        var resetAuditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(ResetPlayerUndoCommand),
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
