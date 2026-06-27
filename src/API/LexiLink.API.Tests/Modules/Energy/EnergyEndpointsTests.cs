using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace LexiLink.API.Tests.Modules.Energy;

[TestFixture]
public sealed class EnergyEndpointsTests
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
                builder.UseSetting("Authentication:Mode", "DevelopmentBearer");
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
    public async Task GetEnergyMe_WithoutBearer_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/energy/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetEnergyMe_WhenAggregateMissing_InitializesAndReturnsSnapshot()
    {
        var playerId = Guid.NewGuid();
        await DeletePlayerEnergyAsync(playerId);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());

        var response = await client.GetAsync("/energy/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("playerId").GetGuid().Should().Be(playerId);
        body.RootElement.GetProperty("currentAmount").GetInt32().Should()
            .Be(body.RootElement.GetProperty("maximumAmount").GetInt32());
        body.RootElement.GetProperty("isFull").GetBoolean().Should().BeTrue();

        await DeletePlayerEnergyAsync(playerId);
    }

    [Test]
    public async Task GetEnergyMe_WhenAggregateInitialized_ReturnsFullEnergySnapshot()
    {
        var playerId = Guid.NewGuid();
        await InsertPlayerEnergyAsync(
            playerId,
            currentAmount: 5,
            maximumAmount: 5,
            rechargeIntervalSeconds: 900,
            lastRefilledOn: DateTime.UtcNow);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());

        var response = await client.GetAsync("/energy/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("playerId").GetGuid().Should().Be(playerId);
        body.RootElement.GetProperty("currentAmount").GetInt32().Should().Be(5);
        body.RootElement.GetProperty("maximumAmount").GetInt32().Should().Be(5);
        body.RootElement.GetProperty("isFull").GetBoolean().Should().BeTrue();
        body.RootElement.GetProperty("secondsUntilNextRefill").ValueKind.Should().Be(JsonValueKind.Null);
        body.RootElement.GetProperty("fullyRefilledAt").ValueKind.Should().Be(JsonValueKind.Null);

        await DeletePlayerEnergyAsync(playerId);
    }

    private static async Task InsertPlayerEnergyAsync(
        Guid playerId,
        int currentAmount,
        int maximumAmount,
        int rechargeIntervalSeconds,
        DateTime lastRefilledOn)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            INSERT INTO "energy"."PlayerEnergies"
                ("PlayerId", "CurrentAmount", "MaximumAmount", "RechargeIntervalSeconds", "LastRefilledOn")
            VALUES
                (@PlayerId, @CurrentAmount, @MaximumAmount, @RechargeIntervalSeconds, @LastRefilledOn)
            ON CONFLICT ("PlayerId") DO UPDATE SET
                "CurrentAmount" = EXCLUDED."CurrentAmount",
                "MaximumAmount" = EXCLUDED."MaximumAmount",
                "RechargeIntervalSeconds" = EXCLUDED."RechargeIntervalSeconds",
                "LastRefilledOn" = EXCLUDED."LastRefilledOn";
        """, connection);
        command.Parameters.AddWithValue("PlayerId", playerId);
        command.Parameters.AddWithValue("CurrentAmount", currentAmount);
        command.Parameters.AddWithValue("MaximumAmount", maximumAmount);
        command.Parameters.AddWithValue("RechargeIntervalSeconds", rechargeIntervalSeconds);
        command.Parameters.AddWithValue("LastRefilledOn", lastRefilledOn);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DeletePlayerEnergyAsync(Guid playerId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            DELETE FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId;
        """, connection);
        command.Parameters.AddWithValue("PlayerId", playerId);
        await command.ExecuteNonQueryAsync();
    }
}
