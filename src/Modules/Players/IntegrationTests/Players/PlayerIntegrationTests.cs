using LexiLink.Modules.Players.Application.Players.GetPlayerByAuthProvider;
using LexiLink.Modules.Players.Application.Players.GetPlayerById;
using LexiLink.Modules.Players.Application.Players.LinkAuthProvider;
using LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;
using LexiLink.Modules.Players.Domain.Players;
using LexiLink.Modules.Players.IntegrationTests.SeedWork;
using Serilog.Events;

namespace LexiLink.Modules.Players.IntegrationTests.Players;

[TestFixture]
public class PlayerIntegrationTests : TestBase
{
    [Test]
    public async Task RegisterGuestPlayer_Test()
    {
        var playerId = await PlayerHelper.RegisterGuestPlayerAsync(Sender);

        playerId.Should().NotBe(Guid.Empty);

        var details = await ExecuteQueryAsync(new GetPlayerByIdQuery(playerId));
        details.Id.Should().Be(playerId);
        details.DisplayName.Should().Be(PlayerHelper.DisplayName);
        details.Handle.Should().StartWith($"{PlayerHelper.DisplayName}#");
        details.Locale.Should().Be(PlayerHelper.Locale);
        details.IsGuest.Should().BeTrue();
        details.AuthIdentities.Should().ContainSingle(a =>
            a.Provider == AuthProvider.Guest &&
            a.ExternalId == PlayerHelper.DeviceId &&
            a.Email == null);
    }

    [Test]
    public async Task RegisterGuestPlayer_WithSameDeviceId_ReturnsExistingGuest_Test()
    {
        var firstPlayerId = await PlayerHelper.RegisterGuestPlayerAsync(
            Sender,
            deviceId: "device-idempotent-guest",
            displayName: "Yasin",
            locale: "tr-TR");

        var secondPlayerId = await PlayerHelper.RegisterGuestPlayerAsync(
            Sender,
            deviceId: "device-idempotent-guest",
            displayName: "Different Name",
            locale: "en-US");

        secondPlayerId.Should().Be(firstPlayerId);

        var details = await ExecuteQueryAsync(new GetPlayerByIdQuery(firstPlayerId));
        details.DisplayName.Should().Be("Yasin");
        details.Locale.Should().Be("tr-TR");
    }

    [Test]
    public async Task LinkAuthProvider_AndGetByAuthProvider_Test()
    {
        var playerId = await PlayerHelper.RegisterGuestPlayerAsync(Sender);

        await ExecuteCommandAsync(new LinkAuthProviderCommand(
            playerId,
            AuthProvider.Apple,
            "apple-sub-integration",
            "yasin@example.com"));

        var details = await ExecuteQueryAsync(new GetPlayerByAuthProviderQuery(
            AuthProvider.Apple,
            "apple-sub-integration"));

        details.Should().NotBeNull();
        details!.Id.Should().Be(playerId);
        details.IsGuest.Should().BeFalse();
        details.AuthIdentities.Should().HaveCount(2);
        details.AuthIdentities.Should().ContainSingle(a =>
            a.Provider == AuthProvider.Apple &&
            a.ExternalId == "apple-sub-integration" &&
            a.Email == "yasin@example.com");
    }

    [Test]
    public async Task GuestPlayer_WhenSocialAuthIsLinked_CanBeResolvedAsAuthenticatedIdentity_Test()
    {
        var playerId = await PlayerHelper.RegisterGuestPlayerAsync(Sender);
        var guestDetails = await ExecuteQueryAsync(new GetPlayerByIdQuery(playerId));
        guestDetails.IsGuest.Should().BeTrue();

        await ExecuteCommandAsync(new LinkAuthProviderCommand(
            playerId,
            AuthProvider.Google,
            "google-sub-auth-transition",
            "yasin@example.com"));

        var authenticatedDetails = await ExecuteQueryAsync(new GetPlayerByAuthProviderQuery(
            AuthProvider.Google,
            "google-sub-auth-transition"));

        authenticatedDetails.Should().NotBeNull();
        authenticatedDetails!.Id.Should().Be(playerId);
        authenticatedDetails.IsGuest.Should().BeFalse();
        authenticatedDetails.AuthIdentities.Should().ContainSingle(a =>
            a.Provider == AuthProvider.Guest &&
            a.ExternalId == PlayerHelper.DeviceId);
        authenticatedDetails.AuthIdentities.Should().ContainSingle(a =>
            a.Provider == AuthProvider.Google &&
            a.ExternalId == "google-sub-auth-transition" &&
            a.Email == "yasin@example.com");
    }

    [Test]
    public async Task UpdatePlayerProfile_Test()
    {
        var playerId = await PlayerHelper.RegisterGuestPlayerAsync(Sender);

        await ExecuteCommandAsync(new UpdatePlayerProfileCommand(
            playerId,
            "https://example.com/avatar.png",
            "en-US"));

        var details = await ExecuteQueryAsync(new GetPlayerByIdQuery(playerId));
        details.AvatarUrl.Should().Be("https://example.com/avatar.png");
        details.Locale.Should().Be("en-US");
    }

    [Test]
    public async Task GetPlayerByAuthProvider_WhenUnknown_ReturnsNull_Test()
    {
        var details = await ExecuteQueryAsync(new GetPlayerByAuthProviderQuery(
            AuthProvider.Google,
            "unknown-google-sub"));

        details.Should().BeNull();
    }

    [Test]
    public async Task CommandExecution_Should_IncludeExecutionContextCorrelationId_Test()
    {
        await PlayerHelper.RegisterGuestPlayerAsync(Sender, deviceId: "device-command-context");

        var hasCorrelationId = CapturedLogs.Events.Any(logEvent =>
        {
            if (!logEvent.Properties.TryGetValue("CorrelationId", out var value))
            {
                return false;
            }

            return value is ScalarValue { Value: Guid correlationId } &&
                correlationId == ExecutionContext.CorrelationId;
        });

        hasCorrelationId.Should().BeTrue();
    }
}
