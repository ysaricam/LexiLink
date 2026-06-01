using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class AuthenticationTests
{
    private const string JwtIssuer = "LexiLink.Tests";
    private const string JwtAudience = "LexiLink.Api.Tests";
    private const string JwtSigningKey = "test-signing-key-with-at-least-32-chars";

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
    public async Task Root_AllowsAnonymousAccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task ProtectedEndpoint_WithoutBearerToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ProtectedEndpoint_WithInvalidBearerToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-player-id");

        var response = await client.GetAsync("/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ProtectedEndpoint_WithValidDevelopmentBearer_ReturnsCurrentUser()
    {
        var playerId = Guid.NewGuid();
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", playerId.ToString());

        var response = await client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("userId").GetGuid().Should().Be(playerId);
    }

    [Test]
    public void ProductionEnvironment_WithDevelopmentBearerConfigured_FailsStartup()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.UseSetting(
                    "ConnectionStrings:LexiLinkDb",
                    "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Development bearer authentication is not allowed in Production*");
    }

    [Test]
    public void ProductionEnvironment_WithDevelopmentExternalTokenExchangeConfigured_FailsStartup()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.UseSetting(
                    "ConnectionStrings:LexiLinkDb",
                    "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852");
                builder.UseSetting("Authentication:Mode", "ProductionJwt");
                builder.UseSetting("Authentication:Jwt:Issuer", JwtIssuer);
                builder.UseSetting("Authentication:Jwt:Audience", JwtAudience);
                builder.UseSetting("Authentication:Jwt:SigningKey", JwtSigningKey);
                builder.UseSetting("Authentication:TokenExchange:Mode", "DevelopmentExternalToken");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Development external identity validation is not allowed in Production*");
    }

    [Test]
    public async Task ProductionEnvironment_WithValidJwt_ReturnsCurrentUser()
    {
        var playerId = Guid.NewGuid();
        using var factory = CreateProductionJwtFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwt(playerId, JwtSigningKey));

        var response = await client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("userId").GetGuid().Should().Be(playerId);
    }

    [Test]
    public async Task ProductionEnvironment_WithJwtSignedByWrongKey_ReturnsUnauthorized()
    {
        using var factory = CreateProductionJwtFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwt(Guid.NewGuid(), "wrong-signing-key-with-at-least-32-chars"));

        var response = await client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static WebApplicationFactory<Program> CreateProductionJwtFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.UseSetting(
                    "ConnectionStrings:LexiLinkDb",
                    "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852");
                builder.UseSetting("Authentication:Mode", "ProductionJwt");
                builder.UseSetting("Authentication:Jwt:Issuer", JwtIssuer);
                builder.UseSetting("Authentication:Jwt:Audience", JwtAudience);
                builder.UseSetting("Authentication:Jwt:SigningKey", JwtSigningKey);

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

    private static string CreateJwt(Guid playerId, string signingKey)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString())]),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
