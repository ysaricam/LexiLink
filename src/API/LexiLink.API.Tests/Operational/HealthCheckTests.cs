using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LexiLink.API.Tests.Operational;

[TestFixture]
public sealed class HealthCheckTests
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
    public async Task LiveHealthCheck_IsAnonymousAndHealthy()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        body.RootElement.GetProperty("checks").TryGetProperty("self", out var selfCheck)
            .Should().BeTrue();
        selfCheck.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Test]
    public async Task ReadyHealthCheck_VerifiesPostgreSqlConnectivity()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");
        var responseBody = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        using var body = JsonDocument.Parse(responseBody);
        body.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        body.RootElement.GetProperty("checks").TryGetProperty("postgresql", out var postgresqlCheck)
            .Should().BeTrue();
        postgresqlCheck.GetProperty("status").GetString().Should().Be("Healthy");
        body.RootElement.GetProperty("checks").TryGetProperty("database-migrations", out var migrationsCheck)
            .Should().BeTrue();
        migrationsCheck.GetProperty("status").GetString().Should().Be("Healthy");
    }
}
