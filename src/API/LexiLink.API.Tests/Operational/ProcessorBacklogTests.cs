using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LexiLink.API.Tests.Operational;

[TestFixture]
public sealed class ProcessorBacklogTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
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
    public async Task ProcessorBacklog_WithoutBearerToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/operations/processors");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ProcessorBacklog_WithAuthenticatedPlayer_ReturnsQueueVisibility()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            Guid.NewGuid().ToString());

        var response = await client.GetAsync("/operations/processors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = body.RootElement;

        root.GetProperty("checkedAt").GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        var queues = root.GetProperty("queues").EnumerateArray().ToArray();

        queues.Select(queue => queue.GetProperty("name").GetString())
            .Should().BeEquivalentTo(
                "games-outbox",
                "players-outbox",
                "stats-inbox",
                "stats-internal-commands");

        foreach (var queue in queues)
        {
            queue.GetProperty("module").GetString().Should().NotBeNullOrWhiteSpace();
            queue.GetProperty("kind").GetString().Should().NotBeNullOrWhiteSpace();
            queue.GetProperty("maxRetryCount").GetInt32().Should().BeGreaterThan(0);
            queue.GetProperty("totalUnprocessed").GetInt64().Should().BeGreaterThanOrEqualTo(0);
            queue.GetProperty("readyToProcess").GetInt64().Should().BeGreaterThanOrEqualTo(0);
            queue.GetProperty("retryScheduled").GetInt64().Should().BeGreaterThanOrEqualTo(0);
            queue.GetProperty("poisoned").GetInt64().Should().BeGreaterThanOrEqualTo(0);
            queue.GetProperty("failed").GetInt64().Should().BeGreaterThanOrEqualTo(0);
            queue.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Array);
        }
    }
}
