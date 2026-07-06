using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace LexiLink.API.Tests.Operational;

[TestFixture]
public sealed class AdsSsvStartupTests
{
    private const string JwtIssuer = "LexiLink.Tests";
    private const string JwtAudience = "LexiLink.Api.Tests";
    private const string JwtSigningKey = "test-signing-key-with-at-least-32-chars";

    [TestCase("Production")]
    [TestCase("Staging")]
    public void NonDevelopmentEnvironment_WithFailOpenSsvConfigured_FailsStartup(string environment)
    {
        using var factory = CreateFactory(environment, "DevelopmentFailOpen");

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Ads:Ssv:Mode=DevelopmentFailOpen is not allowed outside Development*");
    }

    [Test]
    public void DevelopmentEnvironment_WithFailOpenSsvConfigured_Starts()
    {
        using var factory = CreateFactory("Development", "DevelopmentFailOpen");

        using var client = factory.CreateClient();

        client.Should().NotBeNull();
    }

    [Test]
    public void ProductionEnvironment_WithProductionSsvConfigured_Starts()
    {
        using var factory = CreateFactory("Production", "Production");

        using var client = factory.CreateClient();

        client.Should().NotBeNull();
    }

    private static WebApplicationFactory<Program> CreateFactory(
        string environment,
        string adsSsvMode) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(environment);
                builder.UseSetting(
                    "ConnectionStrings:LexiLinkDb",
                    "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852");
                builder.UseSetting("Authentication:Mode", "ProductionJwt");
                builder.UseSetting("Authentication:Jwt:Issuer", JwtIssuer);
                builder.UseSetting("Authentication:Jwt:Audience", JwtAudience);
                builder.UseSetting("Authentication:Jwt:SigningKey", JwtSigningKey);
                builder.UseSetting("Authentication:TokenExchange:Mode", "Disabled");
                builder.UseSetting("Authentication:AdminTokenExchange:Mode", "Disabled");
                builder.UseSetting("Ads:Ssv:Mode", adsSsvMode);

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });
}
