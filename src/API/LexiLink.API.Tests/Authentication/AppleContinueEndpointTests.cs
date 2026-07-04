using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Players.Domain.Players;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
[NonParallelizable]
public sealed class AppleContinueEndpointTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";
    private static int _nextDiscriminator = 2000;

    [SetUp]
    public async Task SetUp()
    {
        await ClearPlayersAsync();
    }

    [Test]
    public async Task Continue_WithNewAppleIdentity_LinksCurrentGuestAndReturnsCurrentSession()
    {
        var currentGuestId = Guid.NewGuid();
        await SeedPlayerAsync(currentGuestId, "current-device", isGuest: true);
        var verifier = new FakeExternalIdentityVerifier();

        using var factory = CreateFactory(verifier);
        using var client = CreateAuthenticatedClient(factory, currentGuestId);

        var response = await client.PostAsJsonAsync(
            "/auth/apple/continue",
            new
            {
                externalId = "apple-new",
                externalToken = "apple-token",
                email = "new@example.com"
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.RootElement.GetProperty("playerId").GetGuid().Should().Be(currentGuestId);
        body.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        body.RootElement.GetProperty("mode").GetString().Should().Be("LinkedCurrentGuest");

        verifier.Calls.Should().ContainSingle()
            .Which.Should().Be((AuthProvider.Apple, "apple-new", "apple-token"));
        (await CountAuthIdentityAsync(currentGuestId, AuthProvider.Apple, "apple-new")).Should().Be(1);
        (await ReadIsGuestAsync(currentGuestId)).Should().BeFalse();
    }

    [Test]
    public async Task Continue_WithExistingAppleIdentity_SwitchesToApplePlayerAndKeepsCurrentGuest()
    {
        var currentGuestId = Guid.NewGuid();
        var applePlayerId = Guid.NewGuid();
        await SeedPlayerAsync(currentGuestId, "ipad-device", isGuest: true);
        await SeedPlayerAsync(applePlayerId, "phone-device", isGuest: false);
        await SeedAuthIdentityAsync(applePlayerId, AuthProvider.Apple, "apple-existing", "apple@example.com");

        using var factory = CreateFactory(new FakeExternalIdentityVerifier());
        using var client = CreateAuthenticatedClient(factory, currentGuestId);

        var response = await client.PostAsJsonAsync(
            "/auth/apple/continue",
            new
            {
                externalId = "apple-existing",
                externalToken = "apple-token",
                email = "apple@example.com"
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.RootElement.GetProperty("playerId").GetGuid().Should().Be(applePlayerId);
        body.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        body.RootElement.GetProperty("mode").GetString().Should().Be("SwitchedToExistingApplePlayer");

        (await CountAuthIdentityAsync(currentGuestId, AuthProvider.Guest, "ipad-device")).Should().Be(1);
        (await CountAuthIdentitiesForPlayerAsync(currentGuestId)).Should().Be(1);
        (await ReadIsGuestAsync(currentGuestId)).Should().BeTrue();
    }

    [Test]
    public async Task Continue_WithSameAppleIdentityOnCurrentPlayer_ReturnsCurrentSessionWithoutDuplicate()
    {
        var currentPlayerId = Guid.NewGuid();
        await SeedPlayerAsync(currentPlayerId, "device-1", isGuest: false);
        await SeedAuthIdentityAsync(currentPlayerId, AuthProvider.Apple, "apple-current", "apple@example.com");

        using var factory = CreateFactory(new FakeExternalIdentityVerifier());
        using var client = CreateAuthenticatedClient(factory, currentPlayerId);

        var response = await client.PostAsJsonAsync(
            "/auth/apple/continue",
            new
            {
                externalId = "apple-current",
                externalToken = "apple-token",
                email = "apple@example.com"
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.RootElement.GetProperty("playerId").GetGuid().Should().Be(currentPlayerId);
        body.RootElement.GetProperty("mode").GetString().Should().Be("LinkedCurrentGuest");
        (await CountAuthIdentityAsync(currentPlayerId, AuthProvider.Apple, "apple-current")).Should().Be(1);
    }

    [Test]
    public async Task Continue_WithInvalidAppleToken_ReturnsUnauthorizedAndDoesNotMutateCurrentGuest()
    {
        var currentGuestId = Guid.NewGuid();
        await SeedPlayerAsync(currentGuestId, "device-1", isGuest: true);

        using var factory = CreateFactory(new FakeExternalIdentityVerifier(result: false));
        using var client = CreateAuthenticatedClient(factory, currentGuestId);

        var response = await client.PostAsJsonAsync(
            "/auth/apple/continue",
            new
            {
                externalId = "apple-invalid",
                externalToken = "bad-token",
                email = "apple@example.com"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await CountAuthIdentitiesForPlayerAsync(currentGuestId)).Should().Be(1);
        (await ReadIsGuestAsync(currentGuestId)).Should().BeTrue();
    }

    [Test]
    public async Task Continue_WhenCurrentPlayerAlreadyHasDifferentApple_ReturnsBadRequest()
    {
        var currentPlayerId = Guid.NewGuid();
        await SeedPlayerAsync(currentPlayerId, "device-1", isGuest: false);
        await SeedAuthIdentityAsync(currentPlayerId, AuthProvider.Apple, "apple-current", "apple@example.com");

        using var factory = CreateFactory(new FakeExternalIdentityVerifier());
        using var client = CreateAuthenticatedClient(factory, currentPlayerId);

        var response = await client.PostAsJsonAsync(
            "/auth/apple/continue",
            new
            {
                externalId = "apple-other",
                externalToken = "apple-token",
                email = "other@example.com"
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CountAuthIdentityAsync(currentPlayerId, AuthProvider.Apple, "apple-current")).Should().Be(1);
        (await CountAuthIdentityAsync(currentPlayerId, AuthProvider.Apple, "apple-other")).Should().Be(0);
    }

    [Test]
    public async Task Continue_WhenExistingApplePlayerIsBanned_ReturnsForbidden()
    {
        var currentGuestId = Guid.NewGuid();
        var bannedApplePlayerId = Guid.NewGuid();
        await SeedPlayerAsync(currentGuestId, "device-1", isGuest: true);
        await SeedPlayerAsync(bannedApplePlayerId, "device-2", isGuest: false, isBanned: true);
        await SeedAuthIdentityAsync(bannedApplePlayerId, AuthProvider.Apple, "apple-banned", "apple@example.com");

        using var factory = CreateFactory(new FakeExternalIdentityVerifier());
        using var client = CreateAuthenticatedClient(factory, currentGuestId);

        var response = await client.PostAsJsonAsync(
            "/auth/apple/continue",
            new
            {
                externalId = "apple-banned",
                externalToken = "apple-token",
                email = "apple@example.com"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Continue_WithoutBearer_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(new FakeExternalIdentityVerifier());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/auth/apple/continue",
            new
            {
                externalId = "apple-new",
                externalToken = "apple-token",
                email = "new@example.com"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static WebApplicationFactory<Program> CreateFactory(FakeExternalIdentityVerifier verifier) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:LexiLinkDb", ConnectionString);
                builder.UseSetting("Authentication:Mode", "DevelopmentBearer");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                    services.RemoveAll<IExternalIdentityVerifier>();
                    services.AddSingleton<IExternalIdentityVerifier>(verifier);
                });
            });

    private static HttpClient CreateAuthenticatedClient(WebApplicationFactory<Program> factory, Guid playerId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());
        return client;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
        await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

    private static async Task SeedPlayerAsync(
        Guid playerId,
        string guestDeviceId,
        bool isGuest,
        bool isBanned = false)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        var discriminator = Interlocked.Increment(ref _nextDiscriminator);
        await conn.ExecuteAsync("""
            INSERT INTO "players"."Players"
                ("Id", "DisplayName", "DiscriminatorValue", "AvatarUrl", "Locale", "CreatedAt", "IsGuest", "IsBanned", "BannedReason", "BannedAt")
            VALUES
                (@Id, 'Player', @Discriminator, NULL, 'en-US', @Now, @IsGuest, @IsBanned, @BannedReason, @BannedAt);
            INSERT INTO "players"."PlayerAuthIdentities"
                ("PlayerId", "Provider", "ExternalId", "Email", "LinkedAt")
            VALUES
                (@Id, 'Guest', @GuestDeviceId, NULL, @Now);
            """,
            new
            {
                Id = playerId,
                Discriminator = discriminator,
                GuestDeviceId = guestDeviceId,
                IsGuest = isGuest,
                IsBanned = isBanned,
                BannedReason = isBanned ? "test ban" : null,
                BannedAt = isBanned ? DateTime.UtcNow : (DateTime?)null,
                Now = DateTime.UtcNow
            });
    }

    private static async Task SeedAuthIdentityAsync(
        Guid playerId,
        AuthProvider provider,
        string externalId,
        string? email)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            INSERT INTO "players"."PlayerAuthIdentities"
                ("PlayerId", "Provider", "ExternalId", "Email", "LinkedAt")
            VALUES
                (@PlayerId, @Provider, @ExternalId, @Email, @Now);
            """,
            new
            {
                PlayerId = playerId,
                Provider = provider.ToString(),
                ExternalId = externalId,
                Email = email,
                Now = DateTime.UtcNow
            });
    }

    private static async Task<int> CountAuthIdentityAsync(Guid playerId, AuthProvider provider, string externalId)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        return await conn.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM "players"."PlayerAuthIdentities"
            WHERE "PlayerId" = @PlayerId
              AND "Provider" = @Provider
              AND "ExternalId" = @ExternalId;
            """,
            new { PlayerId = playerId, Provider = provider.ToString(), ExternalId = externalId });
    }

    private static async Task<int> CountAuthIdentitiesForPlayerAsync(Guid playerId)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        return await conn.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM "players"."PlayerAuthIdentities"
            WHERE "PlayerId" = @PlayerId;
            """,
            new { PlayerId = playerId });
    }

    private static async Task<bool> ReadIsGuestAsync(Guid playerId)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        return await conn.ExecuteScalarAsync<bool>("""
            SELECT "IsGuest"
            FROM "players"."Players"
            WHERE "Id" = @PlayerId;
            """,
            new { PlayerId = playerId });
    }

    private static async Task ClearPlayersAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            DELETE FROM "players"."PlayerAuthIdentities";
            DELETE FROM "players"."Players";
            """);
    }

    private sealed class FakeExternalIdentityVerifier : IExternalIdentityVerifier
    {
        private readonly bool _result;

        public FakeExternalIdentityVerifier(bool result = true)
        {
            _result = result;
        }

        public List<(AuthProvider Provider, string ExternalId, string ExternalToken)> Calls { get; } = [];

        public Task<bool> VerifyAsync(
            AuthProvider provider,
            string externalId,
            string externalToken,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((provider, externalId, externalToken));
            return Task.FromResult(_result);
        }
    }
}
