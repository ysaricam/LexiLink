using System.Net;
using System.Net.Http.Headers;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class BannedPlayerAuthTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    [SetUp]
    public async Task SetUp()
    {
        await ClearPlayersAsync();
    }

    [Test]
    public async Task DevBearer_ForUnknownGuid_StillReachesProtectedEndpoint()
    {
        // Unknown GUID is treated as a fresh device — auth boundary lets it
        // through so /players/guest can register a new player on first call.
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

        var response = await client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task DevBearer_ForBannedPlayer_Returns401()
    {
        var playerId = Guid.NewGuid();
        await SeedBannedPlayerAsync(playerId);

        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());

        var response = await client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:LexiLinkDb", ConnectionString);
                builder.UseSetting("Authentication:Mode", "DevelopmentBearer");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

    private static async Task SeedBannedPlayerAsync(Guid playerId)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            INSERT INTO "players"."Players"
                ("Id", "DisplayName", "DiscriminatorValue", "AvatarUrl", "Locale", "CreatedAt", "IsGuest", "IsBanned", "BannedReason", "BannedAt")
            VALUES
                (@Id, 'Banned', 1234, NULL, 'en-US', @Now, TRUE, TRUE, 'test ban', @Now);
            INSERT INTO "players"."PlayerAuthIdentities"
                ("PlayerId", "Provider", "ExternalId", "Email", "LinkedAt")
            VALUES
                (@Id, 'Guest', 'device-' || @Id::text, NULL, @Now);
            """,
            new { Id = playerId, Now = DateTime.UtcNow });
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
}
