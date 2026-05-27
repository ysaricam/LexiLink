using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Diamond.Application.Admin.GrantBonusDiamond;
using LexiLink.Modules.Diamond.Application.Admin.ResetPlayerDiamond;
using LexiLink.Modules.Diamond.Application.Admin.SetPlayerDiamond;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GetPlayerDiamond;
using LexiLink.Modules.Diamond.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

namespace LexiLink.Modules.Diamond.IntegrationTests.PlayerDiamondInventories;

[TestFixture]
public sealed class DiamondAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004");

    [Test]
    public async Task NonAdmin_Should_Be_RejectedWith_AdminAuthorizationException()
    {
        AdminContext.Logout();
        var playerId = await ProvisionPlayerWithInventoryAsync("device-diamond-admin-not-allowed");

        var act = async () => await ExecuteCommandAsync(new SetPlayerDiamondCommand(playerId, 1));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task GetPlayerDiamond_ReturnsSnapshot()
    {
        var playerId = await ProvisionPlayerWithInventoryAsync("device-diamond-admin-get");

        var snapshot = await DiamondModule.ExecuteQueryAsync(new GetPlayerDiamondQuery(playerId));

        snapshot.PlayerId.Should().Be(playerId);
        snapshot.Balance.Should().Be(0);
    }

    [Test]
    public async Task SetPlayerDiamond_AsAdmin_SnapsBalance_AndAuditsRow()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-diamond-admin-set");

        await ExecuteCommandAsync(new SetPlayerDiamondCommand(playerId, 7));

        var balance = await ReadBalanceAsync(playerId);
        balance.Should().Be(7);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(SetPlayerDiamondCommand),
            TargetId = playerId.ToString()
        });

        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Diamond.PlayerDiamondInventory");
    }

    [Test]
    public async Task GrantBonusDiamond_AsAdmin_AccumulatesBalance_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-diamond-admin-grant");

        await ExecuteCommandAsync(new GrantBonusDiamondCommand(playerId, 5));
        await ExecuteCommandAsync(new GrantBonusDiamondCommand(playerId, 3));

        var balance = await ReadBalanceAsync(playerId);
        balance.Should().Be(8, "Diamond has no max cap");

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(GrantBonusDiamondCommand),
            TargetId = playerId.ToString()
        });
        auditCount.Should().Be(2);
    }

    [Test]
    public async Task ResetPlayerDiamond_AsAdmin_SnapsBalanceToZero_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithInventoryAsync("device-diamond-admin-reset");

        await ExecuteCommandAsync(new SetPlayerDiamondCommand(playerId, 12));
        await ExecuteCommandAsync(new ResetPlayerDiamondCommand(playerId));

        var balance = await ReadBalanceAsync(playerId);
        balance.Should().Be(0);

        await ProcessOutboxAsync();

        var resetAuditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
        """, new
        {
            ActionType = nameof(ResetPlayerDiamondCommand),
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

    private async Task<int> ReadBalanceAsync(Guid playerId) =>
        await QuerySingleOrDefaultAsync<int>("""
            SELECT "Balance" FROM "diamond"."PlayerDiamondInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId });

    private sealed record AdminActionRow(Guid AdminUserId, string ActionType, string TargetType, string? TargetId);
}
