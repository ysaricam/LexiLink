using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Dapper;
using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace LexiLink.API.Tests.Modules.Players;

[TestFixture]
[NonParallelizable]
public sealed class PlayerProfileEndpointsTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";
    private const string JwtIssuer = "LexiLink.Tests";
    private const string JwtAudience = "LexiLink.Api.Tests";
    private const string JwtSigningKey = "test-signing-key-with-at-least-32-chars";

    static PlayerProfileEndpointsTests()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    [SetUp]
    public async Task SetUp()
    {
        await ClearProfileTestPlayersAsync();
    }

    [Test]
    public async Task PatchProfile_WithAppleSession_UpdatesHandleAndProfile()
    {
        var playerId = Guid.NewGuid();
        await SeedPlayerAsync(playerId, "OriginalApple", 8101, isGuest: false);

        try
        {
            using var factory = CreateFactory();
            using var client = CreateAuthenticatedClient(factory, playerId, PlayerAuthSessionMode.Apple);

            var response = await client.PatchAsJsonAsync(
                $"/players/{playerId}/profile",
                new
                {
                    avatarUrl = "https://example.com/avatar.png",
                    locale = "tr-TR",
                    displayName = "UpdatedApple",
                    discriminator = 8102
                });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var player = await ReadPlayerAsync(playerId);
            player.DisplayName.Should().Be("UpdatedApple");
            player.DiscriminatorValue.Should().Be(8102);
            player.AvatarUrl.Should().Be("https://example.com/avatar.png");
            player.Locale.Should().Be("tr-TR");
        }
        finally
        {
            await DeletePlayersAsync(playerId);
        }
    }

    [Test]
    public async Task PatchProfile_WithGuestSession_WhenHandleChanges_ReturnsForbiddenAndDoesNotMutate()
    {
        var playerId = Guid.NewGuid();
        await SeedPlayerAsync(playerId, "GuestUser", 8201, isGuest: true);

        try
        {
            using var factory = CreateFactory();
            using var client = CreateAuthenticatedClient(factory, playerId, PlayerAuthSessionMode.Guest);

            var response = await client.PatchAsJsonAsync(
                $"/players/{playerId}/profile",
                new
                {
                    avatarUrl = (string?)null,
                    locale = "en-US",
                    displayName = "BlockedGuest",
                    discriminator = 8202
                });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var player = await ReadPlayerAsync(playerId);
            player.DisplayName.Should().Be("GuestUser");
            player.DiscriminatorValue.Should().Be(8201);
        }
        finally
        {
            await DeletePlayersAsync(playerId);
        }
    }

    [Test]
    public async Task PatchProfile_ForAnotherPlayer_ReturnsForbidden()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        await SeedPlayerAsync(ownerId, "OwnerUser", 8301, isGuest: false);
        await SeedPlayerAsync(attackerId, "AttackerUser", 8302, isGuest: false);

        try
        {
            using var factory = CreateFactory();
            using var client = CreateAuthenticatedClient(factory, attackerId, PlayerAuthSessionMode.Apple);

            var response = await client.PatchAsJsonAsync(
                $"/players/{ownerId}/profile",
                new
                {
                    avatarUrl = (string?)null,
                    locale = "en-US",
                    displayName = "Hijacked",
                    discriminator = 8303
                });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var owner = await ReadPlayerAsync(ownerId);
            owner.DisplayName.Should().Be("OwnerUser");
            owner.DiscriminatorValue.Should().Be(8301);
        }
        finally
        {
            await DeletePlayersAsync(ownerId, attackerId);
        }
    }

    [Test]
    public async Task PatchProfile_WithDuplicateHandle_ReturnsConflict()
    {
        var playerId = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        await SeedPlayerAsync(playerId, "AvailableUser", 8401, isGuest: false);
        await SeedPlayerAsync(existingId, "TakenUser", 8402, isGuest: false);

        try
        {
            using var factory = CreateFactory();
            using var client = CreateAuthenticatedClient(factory, playerId, PlayerAuthSessionMode.Apple);

            var response = await client.PatchAsJsonAsync(
                $"/players/{playerId}/profile",
                new
                {
                    avatarUrl = (string?)null,
                    locale = "en-US",
                    displayName = "TakenUser",
                    discriminator = 8402
                });

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var player = await ReadPlayerAsync(playerId);
            player.DisplayName.Should().Be("AvailableUser");
            player.DiscriminatorValue.Should().Be(8401);
        }
        finally
        {
            await DeletePlayersAsync(playerId, existingId);
        }
    }

    [Test]
    public async Task PatchProfile_WithInvalidHandle_ReturnsBadRequest()
    {
        var playerId = Guid.NewGuid();
        await SeedPlayerAsync(playerId, "ValidUser", 8501, isGuest: false);

        try
        {
            using var factory = CreateFactory();
            using var client = CreateAuthenticatedClient(factory, playerId, PlayerAuthSessionMode.Apple);

            var response = await client.PatchAsJsonAsync(
                $"/players/{playerId}/profile",
                new
                {
                    avatarUrl = (string?)null,
                    locale = "en-US",
                    displayName = "Bad#Name",
                    discriminator = 8502
                });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            await DeletePlayersAsync(playerId);
        }
    }

    [Test]
    public async Task PatchProfile_WithGuestSession_WhenOnlyProfileChanges_StillWorks()
    {
        var playerId = Guid.NewGuid();
        await SeedPlayerAsync(playerId, "GuestProfile", 8601, isGuest: true);

        try
        {
            using var factory = CreateFactory();
            using var client = CreateAuthenticatedClient(factory, playerId, PlayerAuthSessionMode.Guest);

            var response = await client.PatchAsJsonAsync(
                $"/players/{playerId}/profile",
                new
                {
                    avatarUrl = "https://example.com/guest.png",
                    locale = "fr-FR",
                    displayName = (string?)null,
                    discriminator = (int?)null
                });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var player = await ReadPlayerAsync(playerId);
            player.DisplayName.Should().Be("GuestProfile");
            player.DiscriminatorValue.Should().Be(8601);
            player.AvatarUrl.Should().Be("https://example.com/guest.png");
            player.Locale.Should().Be("fr-FR");
        }
        finally
        {
            await DeletePlayersAsync(playerId);
        }
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:LexiLinkDb", ConnectionString);
                builder.UseSetting("Authentication:Mode", "ProductionJwt");
                builder.UseSetting("Authentication:Jwt:Issuer", JwtIssuer);
                builder.UseSetting("Authentication:Jwt:Audience", JwtAudience);
                builder.UseSetting("Authentication:Jwt:SigningKey", JwtSigningKey);
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

    private static HttpClient CreateAuthenticatedClient(
        WebApplicationFactory<Program> factory,
        Guid playerId,
        PlayerAuthSessionMode sessionMode)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwt(playerId, sessionMode));
        return client;
    }

    private static string CreateJwt(Guid playerId, PlayerAuthSessionMode sessionMode)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSigningKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString()),
                new Claim(AuthConstants.PlayerAuthSessionModeClaimType, sessionMode.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static async Task SeedPlayerAsync(
        Guid playerId,
        string displayName,
        int discriminator,
        bool isGuest)
    {
        var now = DateTime.UtcNow;
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            INSERT INTO "players"."Players"
                ("Id", "DisplayName", "DiscriminatorValue", "AvatarUrl", "Locale", "CreatedAt", "IsGuest", "IsBanned", "BannedReason", "BannedAt")
            VALUES
                (@PlayerId, @DisplayName, @Discriminator, NULL, 'en-US', @Now, @IsGuest, FALSE, NULL, NULL);

            INSERT INTO "players"."PlayerAuthIdentities"
                ("PlayerId", "Provider", "ExternalId", "Email", "LinkedAt")
            VALUES
                (@PlayerId, 'Guest', @GuestDeviceId, NULL, @Now);
            """,
            new
            {
                PlayerId = playerId,
                DisplayName = displayName,
                Discriminator = discriminator,
                IsGuest = isGuest,
                GuestDeviceId = $"profile-test-{playerId:N}",
                Now = now
            });
    }

    private static async Task<PlayerRow> ReadPlayerAsync(Guid playerId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleAsync<PlayerRow>(
            """
            SELECT "DisplayName", "DiscriminatorValue", "AvatarUrl", "Locale"
            FROM "players"."Players"
            WHERE "Id" = @PlayerId;
            """,
            new { PlayerId = playerId });
    }

    private static async Task DeletePlayersAsync(params Guid[] playerIds)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            DELETE FROM "stats"."PlayerStats" WHERE "PlayerId" = ANY(@PlayerIds);
            DELETE FROM "players"."PlayerAuthIdentities" WHERE "PlayerId" = ANY(@PlayerIds);
            DELETE FROM "players"."Players" WHERE "Id" = ANY(@PlayerIds);
            """,
            new { PlayerIds = playerIds });
    }

    private static async Task ClearProfileTestPlayersAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            WITH profile_test_players AS (
                SELECT "PlayerId"
                FROM "players"."PlayerAuthIdentities"
                WHERE "ExternalId" LIKE 'profile-test-%'
            )
            DELETE FROM "stats"."PlayerStats"
            WHERE "PlayerId" IN (SELECT "PlayerId" FROM profile_test_players);

            WITH profile_test_players AS (
                SELECT "PlayerId"
                FROM "players"."PlayerAuthIdentities"
                WHERE "ExternalId" LIKE 'profile-test-%'
            )
            DELETE FROM "players"."PlayerAuthIdentities"
            WHERE "PlayerId" IN (SELECT "PlayerId" FROM profile_test_players);

            DELETE FROM "players"."Players"
            WHERE "DisplayName" IN (
                'OriginalApple',
                'UpdatedApple',
                'GuestUser',
                'BlockedGuest',
                'OwnerUser',
                'AttackerUser',
                'Hijacked',
                'AvailableUser',
                'TakenUser',
                'ValidUser',
                'GuestProfile'
            );
            """);
    }

    private sealed record PlayerRow(
        string DisplayName,
        int DiscriminatorValue,
        string? AvatarUrl,
        string Locale);
}
