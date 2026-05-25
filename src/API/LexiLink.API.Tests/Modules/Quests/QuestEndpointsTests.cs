using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace LexiLink.API.Tests.Modules.Quests;

[TestFixture]
public sealed class QuestEndpointsTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    // Daily seed from 021_SeedQuestDefinitions.sql.
    private static readonly Guid SeedDailyQuestDefinitionId =
        Guid.Parse("11111111-0000-0000-0000-000000000010");

    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:LexiLinkDb", ConnectionString);
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task GetQuestsMe_WithoutBearer_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/quests/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PostClaim_WithoutBearer_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync($"/quests/{Guid.NewGuid()}/claim", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetQuestsMe_FreshPlayer_LazilyReturnsSeededDaily()
    {
        var playerId = Guid.NewGuid();

        try
        {
            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());

            var response = await client.GetAsync("/quests/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            body.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
            body.RootElement.GetArrayLength().Should().Be(1, "lazy sync issues the seeded daily quest");
            var first = body.RootElement[0];
            first.GetProperty("playerId").GetGuid().Should().Be(playerId);
            first.GetProperty("questDefinitionId").GetGuid().Should().Be(SeedDailyQuestDefinitionId);
            first.GetProperty("trigger").GetString().Should().Be("GameCompletedDaily");
            first.GetProperty("threshold").GetInt32().Should().Be(3);
            first.GetProperty("energyReward").GetInt32().Should().Be(5);
            first.GetProperty("hintReward").GetInt32().Should().Be(0);
            first.GetProperty("displayState").GetString().Should().Be("Active");
        }
        finally
        {
            await DeletePlayerQuestsAsync(playerId);
        }
    }

    [Test]
    public async Task PostClaim_OnAnotherPlayersQuest_ReturnsNotFound()
    {
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var questId = Guid.NewGuid();
        await UpsertActiveQuestAsync(
            questId,
            ownerId,
            questDefinitionId: SeedDailyQuestDefinitionId,
            baselineSnapshot: 0,
            issuedAt: DateTime.UtcNow.AddMinutes(-5));

        try
        {
            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherId.ToString());

            var response = await client.PostAsync($"/quests/{questId}/claim", content: null);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            await DeletePlayerQuestsAsync(ownerId);
        }
    }

    private static async Task UpsertActiveQuestAsync(
        Guid id,
        Guid playerId,
        Guid questDefinitionId,
        int baselineSnapshot,
        DateTime issuedAt)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            INSERT INTO "quests"."PlayerQuests"
                ("Id", "PlayerId", "QuestDefinitionId", "ProgressBaselineSnapshot",
                 "State", "IssuedAt", "ClaimedAt", "ExpiresAt")
            VALUES
                (@Id, @PlayerId, @QuestDefinitionId, @ProgressBaselineSnapshot,
                 'Active', @IssuedAt, NULL, NULL)
            ON CONFLICT ("Id") DO UPDATE SET
                "State" = EXCLUDED."State",
                "ProgressBaselineSnapshot" = EXCLUDED."ProgressBaselineSnapshot";
        """, connection);
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("PlayerId", playerId);
        command.Parameters.AddWithValue("QuestDefinitionId", questDefinitionId);
        command.Parameters.AddWithValue("ProgressBaselineSnapshot", baselineSnapshot);
        command.Parameters.AddWithValue("IssuedAt", issuedAt);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DeletePlayerQuestsAsync(Guid playerId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            DELETE FROM "quests"."OutboxMessages";
            DELETE FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId;
        """, connection);
        command.Parameters.AddWithValue("PlayerId", playerId);
        await command.ExecuteNonQueryAsync();
    }
}
