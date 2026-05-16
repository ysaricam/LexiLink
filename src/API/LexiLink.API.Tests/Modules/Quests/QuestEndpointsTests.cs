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
    public async Task GetQuestsMe_WhenPlayerHasQuests_ReturnsList()
    {
        var playerId = Guid.NewGuid();
        var questId = Guid.NewGuid();
        await UpsertQuestAsync(
            questId,
            playerId,
            questType: "FirstGameCompleted",
            state: "ReadyToClaim",
            progress: 1,
            goal: 1,
            rewardAmount: 3,
            issuedAt: DateTime.UtcNow.AddMinutes(-5));

        try
        {
            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());

            var response = await client.GetAsync("/quests/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            body.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
            body.RootElement.GetArrayLength().Should().Be(1);
            var first = body.RootElement[0];
            first.GetProperty("id").GetGuid().Should().Be(questId);
            first.GetProperty("playerId").GetGuid().Should().Be(playerId);
            first.GetProperty("questType").GetString().Should().Be("FirstGameCompleted");
            first.GetProperty("state").GetString().Should().Be("ReadyToClaim");
            first.GetProperty("rewardAmount").GetInt32().Should().Be(3);
        }
        finally
        {
            await DeletePlayerQuestsAsync(playerId);
        }
    }

    [Test]
    public async Task PostClaim_OnReadyToClaimQuest_Returns204AndMarksClaimed()
    {
        var playerId = Guid.NewGuid();
        var questId = Guid.NewGuid();
        await UpsertQuestAsync(
            questId,
            playerId,
            questType: "FirstGameCompleted",
            state: "ReadyToClaim",
            progress: 1,
            goal: 1,
            rewardAmount: 3,
            issuedAt: DateTime.UtcNow.AddMinutes(-5));

        try
        {
            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());

            var response = await client.PostAsync($"/quests/{questId}/claim", content: null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var state = await ReadQuestStateAsync(questId);
            state.Should().Be("Claimed");
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
        await UpsertQuestAsync(
            questId,
            ownerId,
            questType: "FirstGameCompleted",
            state: "ReadyToClaim",
            progress: 1,
            goal: 1,
            rewardAmount: 3,
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

    private static async Task UpsertQuestAsync(
        Guid id,
        Guid playerId,
        string questType,
        string state,
        int progress,
        int goal,
        int rewardAmount,
        DateTime issuedAt)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            INSERT INTO "quests"."PlayerQuests"
                ("Id", "PlayerId", "QuestType", "Progress", "Goal", "RewardAmount", "State", "IssuedAt")
            VALUES
                (@Id, @PlayerId, @QuestType, @Progress, @Goal, @RewardAmount, @State, @IssuedAt)
            ON CONFLICT ("Id") DO UPDATE SET
                "State" = EXCLUDED."State",
                "Progress" = EXCLUDED."Progress";
        """, connection);
        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("PlayerId", playerId);
        command.Parameters.AddWithValue("QuestType", questType);
        command.Parameters.AddWithValue("Progress", progress);
        command.Parameters.AddWithValue("Goal", goal);
        command.Parameters.AddWithValue("RewardAmount", rewardAmount);
        command.Parameters.AddWithValue("State", state);
        command.Parameters.AddWithValue("IssuedAt", issuedAt);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<string?> ReadQuestStateAsync(Guid id)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            SELECT "State" FROM "quests"."PlayerQuests" WHERE "Id" = @Id;
        """, connection);
        command.Parameters.AddWithValue("Id", id);
        var result = await command.ExecuteScalarAsync();
        return result as string;
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
