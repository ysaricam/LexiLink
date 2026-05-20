using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Dapper;
using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Modules.Admin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class AdminAuthenticationTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";
    private const string JwtIssuer = "LexiLink.Tests";
    private const string JwtAudience = "LexiLink.Api.Tests";
    private const string JwtSigningKey = "test-signing-key-with-at-least-32-chars";

    [SetUp]
    public async Task SetUp()
    {
        await ClearAdminUsersAsync();
    }

    [Test]
    public async Task AdminEndpoint_WithAnonymousRequest_Returns401()
    {
        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/admin/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task AdminEndpoint_WithPlayerBearer_Returns403()
    {
        var nonAdminGuid = Guid.NewGuid();
        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", nonAdminGuid.ToString());

        var response = await client.GetAsync("/admin/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task AdminEndpoint_WithDevAdminBearer_Returns200()
    {
        var adminId = await SeedAdminAsync("dev-admin@lexilink.test");
        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminId.ToString());

        var response = await client.GetAsync("/admin/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("adminUserId").GetGuid().Should().Be(adminId);
        body.RootElement.GetProperty("role").GetString().Should().Be("Admin");
    }

    [Test]
    public async Task AdminTokenExchange_WithValidExternalToken_IssuesAdminJwt()
    {
        const string adminEmail = "exchange-admin@lexilink.test";
        var adminId = await SeedAdminAsync(adminEmail);

        using var factory = CreateProductionJwtFactory();
        using var client = factory.CreateClient();

        var exchangeResponse = await client.PostAsJsonAsync("/auth/admin/token", new AdminTokenExchangeRequest(
            Email: adminEmail,
            ExternalToken: $"dev:admin:{adminEmail}"));

        exchangeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await exchangeResponse.Content.ReadFromJsonAsync<AdminTokenExchangeResponse>();
        token.Should().NotBeNull();
        token!.AdminUserId.Should().Be(adminId);
        token.Role.Should().Be("Admin");
        token.AccessToken.Should().NotBeNullOrEmpty();

        // Now use the issued JWT to call /admin/whoami.
        using var authedClient = factory.CreateClient();
        authedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var whoamiResponse = await authedClient.GetAsync("/admin/whoami");

        whoamiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await whoamiResponse.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("adminUserId").GetGuid().Should().Be(adminId);
    }

    [Test]
    public async Task AdminTokenExchange_WithInvalidExternalToken_Returns401()
    {
        const string adminEmail = "exchange-bad-admin@lexilink.test";
        await SeedAdminAsync(adminEmail);

        using var factory = CreateProductionJwtFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/admin/token", new AdminTokenExchangeRequest(
            Email: adminEmail,
            ExternalToken: "wrong"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task AdminTokenExchange_WithUnknownEmail_Returns404()
    {
        using var factory = CreateProductionJwtFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/admin/token", new AdminTokenExchangeRequest(
            Email: "ghost@lexilink.test",
            ExternalToken: "dev:admin:ghost@lexilink.test"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AdminJwt_WithRoleClaimButRevokedAdmin_Returns401()
    {
        const string adminEmail = "revoke-me@lexilink.test";
        var adminId = await SeedAdminAsync(adminEmail);

        // Issue a JWT that LOOKS like a valid admin token...
        var jwt = CreateAdminJwt(adminId, JwtSigningKey);

        // ...then revoke (delete) the admin in the database.
        await DisableAdminAsync(adminEmail);

        using var factory = CreateProductionJwtFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("/admin/whoami");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PlayerEndpoint_WithDevAdminBearer_StillAuthorizes()
    {
        // Admin GUID also satisfies AuthenticatedPlayer policy in dev mode —
        // by design: the bearer is both a player principal and an admin.
        var adminId = await SeedAdminAsync("both-roles@lexilink.test");
        using var factory = CreateDevFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminId.ToString());

        var response = await client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("userId").GetGuid().Should().Be(adminId);
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

    private static WebApplicationFactory<Program> CreateProductionJwtFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Staging");
                builder.UseSetting("ConnectionStrings:LexiLinkDb", ConnectionString);
                builder.UseSetting("Authentication:Mode", "ProductionJwt");
                builder.UseSetting("Authentication:Jwt:Issuer", JwtIssuer);
                builder.UseSetting("Authentication:Jwt:Audience", JwtAudience);
                builder.UseSetting("Authentication:Jwt:SigningKey", JwtSigningKey);
                builder.UseSetting("Authentication:AdminTokenExchange:Mode", "DevelopmentExternalToken");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

    private static string CreateAdminJwt(Guid adminId, string signingKey)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, adminId.ToString()),
                new Claim(AuthConstants.RoleClaimType, AuthConstants.AdminRoleValue),
                new Claim(AuthConstants.AdminUserIdClaimType, adminId.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static async Task<Guid> SeedAdminAsync(string email)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        var normalized = email.Trim().ToLowerInvariant();

        // Idempotent: if the admin already exists, return its id.
        var existing = await conn.QuerySingleOrDefaultAsync<Guid?>("""
            SELECT "Id" FROM "administration"."AdminUsers" WHERE "Email" = @Email;
            """, new { Email = normalized });
        if (existing is not null && existing != Guid.Empty)
        {
            // Ensure it's Active for the test.
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

    private static async Task DisableAdminAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("""
            UPDATE "administration"."AdminUsers"
            SET "Status" = 'Disabled', "DisabledOn" = @Now
            WHERE "Email" = @Email;
            """, new { Email = normalized, Now = DateTime.UtcNow });
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
