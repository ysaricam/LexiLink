using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace LexiLink.API.Tests.ApiContract;

[TestFixture]
public sealed class ValidationProblemDetailsTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:LexiLinkDb",
                    "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852");

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
    public async Task CommandValidationFailure_ReturnsValidationProblemDetails()
    {
        // /categories POST moved under /admin/content/categories in B10,
        // behind AuthenticatedAdmin. Seed an admin GUID so the bearer
        // handler accepts the token as an admin principal, then exercise
        // the validation pipeline on the admin endpoint.
        var adminId = Guid.NewGuid();
        await SeedAdminAsync(adminId, $"validation-{adminId}@lexilink.test");

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            adminId.ToString());

        var response = await client.PostAsJsonAsync("/admin/content/categories", new
        {
            name = "",
            description = "Valid description"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be("https://httpstatuses.com/400");
        body.RootElement.GetProperty("title").GetString().Should().Be("Validation failed");
        body.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        body.RootElement.GetProperty("detail").GetString().Should().Be("One or more validation errors occurred.");
        body.RootElement.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
        body.RootElement.GetProperty("errors").GetProperty("Name").EnumerateArray()
            .Select(error => error.GetString())
            .Should().ContainSingle(error => !string.IsNullOrWhiteSpace(error));
    }

    [Test]
    public async Task EndpointLevelNotFound_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            Guid.NewGuid().ToString());

        var playerId = Guid.NewGuid();
        var response = await client.GetAsync($"/stats/players/{playerId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be("https://httpstatuses.com/404");
        body.RootElement.GetProperty("title").GetString().Should().Be("Resource not found");
        body.RootElement.GetProperty("status").GetInt32().Should().Be(404);
        body.RootElement.GetProperty("detail").GetString().Should().Contain(playerId.ToString());
        body.RootElement.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task BusinessRuleFailure_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/players/guest", new
        {
            deviceId = "device-business-rule",
            displayName = "Yasin",
            locale = "invalid-locale"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be("https://httpstatuses.com/400");
        body.RootElement.GetProperty("title").GetString().Should().Be("Business rule violation");
        body.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        body.RootElement.GetProperty("detail").GetString().Should().Contain("Locale");
        body.RootElement.GetProperty("rule").GetString().Should().Be("LocaleMustBeValidFormatRule");
        body.RootElement.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    private static async Task SeedAdminAsync(Guid adminId, string email)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            """
            INSERT INTO "administration"."AdminUsers"
                ("Id", "Email", "Role", "Status", "RegisteredOn", "DisabledOn")
            VALUES (@Id, @Email, 'Admin', 'Active', @Now, NULL)
            ON CONFLICT ("Id") DO NOTHING;
            """,
            new { Id = adminId, Email = email.ToLowerInvariant(), Now = DateTime.UtcNow });
    }
}
