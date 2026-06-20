using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Players.Application.Admin.BanPlayer;
using LexiLink.Modules.Players.Application.Admin.GetPlayerAdminDetail;
using LexiLink.Modules.Players.Application.Admin.GetPlayerBanStatus;
using LexiLink.Modules.Players.Application.Admin.UnbanPlayer;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Players.IntegrationTests.SeedWork;
using MediatR;

namespace LexiLink.Modules.Players.IntegrationTests.Players;

[TestFixture]
public sealed class PlayerAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("cccccccc-0000-0000-0000-000000000001");

    [Test]
    public async Task NonAdmin_Should_Be_RejectedWith_AdminAuthorizationException()
    {
        AdminContext.Logout();
        var playerId = await Sender.Send(new RegisterGuestPlayerCommand("dev-ban-no-auth", "Test", "en-US"));

        var act = async () => await Sender.Send(new BanPlayerCommand(playerId, "spam"));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task BanPlayer_AsAdmin_FlipsBanFlag_AndWritesAuditRow()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await Sender.Send(new RegisterGuestPlayerCommand("dev-ban-1", "Banned", "en-US"));

        await Sender.Send(new BanPlayerCommand(playerId, "harassment"));

        var row = await QuerySingleOrDefaultAsync<BanRow>("""
            SELECT "IsBanned", "BannedReason"
            FROM "players"."Players" WHERE "Id" = @Id
            """, new { Id = playerId });
        row.Should().NotBeNull();
        row!.IsBanned.Should().BeTrue();
        row.BannedReason.Should().Be("harassment");

        var isBanned = await Sender.Send(new GetPlayerBanStatusQuery(playerId));
        isBanned.Should().BeTrue();

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
            """,
            new { ActionType = nameof(BanPlayerCommand), TargetId = playerId.ToString() });
        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Players.Player");
    }

    [Test]
    public async Task UnbanPlayer_AsAdmin_ClearsBanFlag_AndWritesAuditRow()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await Sender.Send(new RegisterGuestPlayerCommand("dev-unban-1", "User", "en-US"));
        await Sender.Send(new BanPlayerCommand(playerId, "mistake"));

        await Sender.Send(new UnbanPlayerCommand(playerId));

        var isBanned = await Sender.Send(new GetPlayerBanStatusQuery(playerId));
        isBanned.Should().BeFalse();

        await ProcessOutboxAsync();

        var unbanAuditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
            """,
            new { ActionType = nameof(UnbanPlayerCommand), TargetId = playerId.ToString() });
        unbanAuditCount.Should().Be(1);
    }

    [Test]
    public async Task BanPlayer_WithEmptyReason_FailsValidation()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await Sender.Send(new RegisterGuestPlayerCommand("dev-ban-empty", "Empty", "en-US"));

        var act = async () => await Sender.Send(new BanPlayerCommand(playerId, ""));

        await act.Should().ThrowAsync<Common.Application.Exceptions.InvalidCommandException>();
    }

    [Test]
    public async Task GetPlayerAdminDetail_ReturnsRichPayload_WhenPlayerExists()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await Sender.Send(new RegisterGuestPlayerCommand("dev-detail-1", "Yasin", "tr-TR"));

        var detail = await Sender.Send(new GetPlayerAdminDetailQuery(playerId));

        detail.Should().NotBeNull();
        detail!.Id.Should().Be(playerId);
        detail.DisplayName.Should().Be("Yasin");
        detail.Locale.Should().Be("tr-TR");
        detail.IsGuest.Should().BeTrue();
        detail.IsBanned.Should().BeFalse();
        detail.Handle.Should().StartWith("Yasin#");
        detail.AuthProvidersLinked.Should().Be(0);
    }

    [Test]
    public async Task GetPlayerAdminDetail_ReturnsNull_WhenPlayerMissing()
    {
        AdminContext.LoginAs(AdminId);

        var detail = await Sender.Send(new GetPlayerAdminDetailQuery(Guid.NewGuid()));

        detail.Should().BeNull();
    }

    [Test]
    public async Task GetPlayerAdminDetailByHandle_ReturnsRichPayload_WhenPlayerExists()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = await Sender.Send(new RegisterGuestPlayerCommand("dev-detail-handle-1", "Handle", "en-US"));
        var byId = await Sender.Send(new GetPlayerAdminDetailQuery(playerId));

        var byHandle = await Sender.Send(new GetPlayerAdminDetailByHandleQuery(
            byId!.DisplayName,
            byId.Discriminator));

        byHandle.Should().NotBeNull();
        byHandle!.Id.Should().Be(playerId);
        byHandle.Handle.Should().Be(byId.Handle);
        byHandle.DisplayName.Should().Be("Handle");
    }

    [Test]
    public async Task GetPlayerAdminDetailByHandle_ReturnsNull_WhenPlayerMissing()
    {
        AdminContext.LoginAs(AdminId);

        var detail = await Sender.Send(new GetPlayerAdminDetailByHandleQuery("Missing", 9999));

        detail.Should().BeNull();
    }

    [Test]
    public async Task GetPlayerBanStatus_ReturnsFalse_ForUnknownPlayer()
    {
        AdminContext.Logout();

        // Auth boundary uses this query — unknown ids must not be reported
        // as banned or first-time guest registration would fail.
        var isBanned = await Sender.Send(new GetPlayerBanStatusQuery(Guid.NewGuid()));

        isBanned.Should().BeFalse();
    }

    private sealed record BanRow(bool IsBanned, string? BannedReason);
    private sealed record AdminActionRow(Guid AdminUserId, string ActionType, string TargetType, string? TargetId);
}
