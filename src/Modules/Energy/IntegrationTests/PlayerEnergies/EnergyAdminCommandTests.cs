using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Energy.Application.Admin.GrantBonusEnergy;
using LexiLink.Modules.Energy.Application.Admin.ResetPlayerEnergy;
using LexiLink.Modules.Energy.Application.Admin.SetPlayerEnergy;
using LexiLink.Modules.Energy.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

namespace LexiLink.Modules.Energy.IntegrationTests.PlayerEnergies;

[TestFixture]
public sealed class EnergyAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    [Test]
    public async Task NonAdmin_Should_Be_RejectedWith_AdminAuthorizationException()
    {
        AdminContext.Logout();
        var playerId = await ProvisionPlayerWithEnergyAsync("device-energy-admin-not-allowed");

        var act = async () => await ExecuteCommandAsync(new SetPlayerEnergyCommand(playerId, 1));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task SetPlayerEnergy_AsAdmin_SnapsCurrent_AndAuditsRow()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithEnergyAsync("device-energy-admin-set");

        await ExecuteCommandAsync(new SetPlayerEnergyCommand(playerId, 1));

        var current = await QuerySingleOrDefaultAsync<int>("""
            SELECT "CurrentAmount" FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId
            """, new { PlayerId = playerId });
        current.Should().Be(1);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
            """,
            new
            {
                ActionType = nameof(SetPlayerEnergyCommand),
                TargetId = playerId.ToString()
            });

        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Energy.PlayerEnergy");
    }

    [Test]
    public async Task GrantBonusEnergy_AsAdmin_PushesAboveMax_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithEnergyAsync("device-energy-admin-grant");

        var maxAmount = await QuerySingleOrDefaultAsync<int>("""
            SELECT "MaximumAmount" FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId
            """, new { PlayerId = playerId });

        await ExecuteCommandAsync(new GrantBonusEnergyCommand(playerId, 3));

        var current = await QuerySingleOrDefaultAsync<int>("""
            SELECT "CurrentAmount" FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId
            """, new { PlayerId = playerId });
        current.Should().Be(maxAmount + 3, "GrantBonus intentionally permits over-max balance");

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType
            """, new { ActionType = nameof(GrantBonusEnergyCommand) });

        audit.Should().NotBeNull();
        audit!.TargetId.Should().Be(playerId.ToString());
    }

    [Test]
    public async Task ResetPlayerEnergy_AsAdmin_RestoresToMax_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await ProvisionPlayerWithEnergyAsync("device-energy-admin-reset");

        // First drop to 0 via a separate admin set.
        await ExecuteCommandAsync(new SetPlayerEnergyCommand(playerId, 0));

        // Then reset back to max.
        await ExecuteCommandAsync(new ResetPlayerEnergyCommand(playerId));

        var snapshot = await QuerySingleOrDefaultAsync<EnergySnapshot>("""
            SELECT "CurrentAmount", "MaximumAmount"
            FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId
            """, new { PlayerId = playerId });
        snapshot.Should().NotBeNull();
        snapshot!.CurrentAmount.Should().Be(snapshot.MaximumAmount);

        await ProcessOutboxAsync();

        var resetAuditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
            """,
            new { ActionType = nameof(ResetPlayerEnergyCommand), TargetId = playerId.ToString() });

        resetAuditCount.Should().Be(1);
    }

    private async Task<Guid> ProvisionPlayerWithEnergyAsync(string deviceId)
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand(deviceId, "Test", "en-US"));
        await ProcessOutboxAsync();
        return playerId;
    }

    private sealed record AdminActionRow(Guid AdminUserId, string ActionType, string TargetType, string? TargetId);
    private sealed record EnergySnapshot(int CurrentAmount, int MaximumAmount);
}
