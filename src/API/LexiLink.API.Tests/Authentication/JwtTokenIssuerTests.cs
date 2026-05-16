using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application.Time;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class JwtTokenIssuerTests
{
    private const string Issuer = "LexiLink.Tests";
    private const string Audience = "LexiLink.Api.Tests";
    private const string SigningKey = "test-signing-key-with-at-least-32-chars";
    private static readonly DateTime FixedNow = new(2026, 5, 12, 20, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task Issue_Should_Create_Signed_Token_With_Player_Subject()
    {
        var playerId = Guid.NewGuid();
        var issuer = new JwtTokenIssuer(
            new LexiLinkAuthOptions
            {
                Mode = LexiLinkAuthMode.ProductionJwt,
                Jwt = new JwtAuthOptions
                {
                    Issuer = Issuer,
                    Audience = Audience,
                    SigningKey = SigningKey,
                    AccessTokenLifetimeMinutes = 30
                }
            },
            new FixedClock(FixedNow));

        var token = issuer.Issue(playerId);

        token.ExpiresAt.Should().Be(FixedNow.AddMinutes(30));
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(
            token.AccessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                ValidateLifetime = false,
                RequireSignedTokens = true
            });
        result.IsValid.Should().BeTrue();
        result.ClaimsIdentity.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub)
            .Value.Should().Be(playerId.ToString());
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}
