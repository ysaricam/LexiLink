using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class AdminAuditEndpointTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    [SetUp]
    public async Task SetUp()
    {
        await ClearAuditAsync();
        await ClearAdminUsersAsync();
    }

    [Test]
    public async Task AuditEndpoint_WithAnonymous_Returns401()
    {
        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/admin/audit");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task AuditEndpoint_WithPlayerBearer_Returns403()
    {
        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", Guid.NewGuid().ToString());

        var response = await client.GetAsync("/admin/audit");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task AuditEndpoint_WithDevAdminBearer_ReturnsSeededAuditRows()
    {
        var adminId = await SeedAdminAsync("audit-admin@lexilink.test");

        // Seed a couple of audit rows directly (B5 only ships the consumer
        // side; producer per-module decorators arrive in B7+).
        await InsertAuditRowAsync(
            adminId: adminId,
            actionType: "Quests.CreateQuestDefinitionCommand",
            targetType: "Quests.QuestDefinition",
            targetId: Guid.NewGuid().ToString(),
            payload: "{\"goal\":3}",
            occurredOn: DateTime.UtcNow.AddMinutes(-1));
        await InsertAuditRowAsync(
            adminId: adminId,
            actionType: "Energy.SetPlayerEnergyCommand",
            targetType: "Energy.PlayerEnergy",
            targetId: Guid.NewGuid().ToString(),
            payload: "{\"amount\":5}",
            occurredOn: DateTime.UtcNow);

        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", adminId.ToString());

        var response = await client.GetAsync("/admin/audit");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetArrayLength().Should().Be(2);

        var first = body.RootElement[0];
        // OccurredOn desc — Energy command is newer.
        first.GetProperty("actionType").GetString().Should().Be("Energy.SetPlayerEnergyCommand");
        first.GetProperty("adminUserId").GetGuid().Should().Be(adminId);
    }

    [Test]
    public async Task AuditEndpoint_Should_FilterByAdminUserId()
    {
        var actorA = await SeedAdminAsync("actor-a@lexilink.test");
        var actorB = await SeedAdminAsync("actor-b@lexilink.test");
        await InsertAuditRowAsync(actorA, "ActionA", "T", null, "{}", DateTime.UtcNow);
        await InsertAuditRowAsync(actorB, "ActionB", "T", null, "{}", DateTime.UtcNow);

        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", actorA.ToString());

        var response = await client.GetAsync($"/admin/audit?adminUserId={actorA}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetArrayLength().Should().Be(1);
        body.RootElement[0].GetProperty("actionType").GetString().Should().Be("ActionA");
    }

    private static WebApplicationFactory<Program> CreateDevFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:LexiLinkDb", ConnectionString);
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

    private static async Task<Guid> SeedAdminAsync(string email)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        var normalized = email.Trim().ToLowerInvariant();

        var existing = await conn.QuerySingleOrDefaultAsync<Guid?>("""
            SELECT "Id" FROM "administration"."AdminUsers" WHERE "Email" = @Email;
            """, new { Email = normalized });
        if (existing is not null && existing != Guid.Empty)
        {
            await conn.ExecuteAsync("""
                UPDATE "administration"."AdminUsers" SET "Status" = 'Active', "DisabledOn" = NULL WHERE "Id" = @Id;
                """, new { Id = existing });
            return existing.Value;
        }

        var id = Guid.NewGuid();
        await conn.ExecuteAsync("""
            INSERT INTO "administration"."AdminUsers"
                ("Id", "Email", "Role", "Status", "RegisteredOn", "DisabledOn")
            VALUES
                (@Id, @Email, 'Admin', 'Active', @Now, NULL);
            """, new { Id = id, Email = normalized, Now = DateTime.UtcNow });

        return id;
    }

    private static async Task InsertAuditRowAsync(
        Guid adminId,
        string actionType,
        string targetType,
        string? targetId,
        string payload,
        DateTime occurredOn)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            INSERT INTO "administration"."AdminActionAudit"
                ("Id", "OccurredOn", "AdminUserId", "ActionType", "TargetType", "TargetId", "PayloadJson")
            VALUES
                (@Id, @OccurredOn, @AdminUserId, @ActionType, @TargetType, @TargetId, @PayloadJson);
            """,
            new
            {
                Id = Guid.NewGuid(),
                OccurredOn = occurredOn,
                AdminUserId = adminId,
                ActionType = actionType,
                TargetType = targetType,
                TargetId = targetId,
                PayloadJson = payload
            });
    }

    private static async Task ClearAuditAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM \"administration\".\"AdminActionAudit\";");
    }

    private static async Task ClearAdminUsersAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            DELETE FROM "administration"."OutboxMessages";
            DELETE FROM "administration"."AdminUsers";
            """);
    }
}
