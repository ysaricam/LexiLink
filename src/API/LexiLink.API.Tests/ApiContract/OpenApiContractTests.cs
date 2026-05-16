using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LexiLink.API.Tests.ApiContract;

[TestFixture]
public sealed class OpenApiContractTests
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
    public async Task OpenApiDocument_DescribesBearerAuthAndProblemResponses()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = body.RootElement;

        var bearerScheme = root
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("LexiLinkBearer");
        bearerScheme.GetProperty("type").GetString().Should().Be("http");
        bearerScheme.GetProperty("scheme").GetString().Should().Be("bearer");

        var statsByPlayerGet = root
            .GetProperty("paths")
            .GetProperty("/stats/players/{playerId}")
            .GetProperty("get");

        statsByPlayerGet.GetProperty("security")[0]
            .TryGetProperty("LexiLinkBearer", out _)
            .Should().BeTrue();
        statsByPlayerGet.GetProperty("responses").TryGetProperty("401", out _)
            .Should().BeTrue();
        statsByPlayerGet.GetProperty("responses").TryGetProperty("404", out var notFound)
            .Should().BeTrue();
        notFound.GetProperty("content").TryGetProperty("application/problem+json", out _)
            .Should().BeTrue();

        var guestRegistrationPost = root
            .GetProperty("paths")
            .GetProperty("/players/guest")
            .GetProperty("post");

        guestRegistrationPost.TryGetProperty("security", out _).Should().BeFalse();
    }
}
