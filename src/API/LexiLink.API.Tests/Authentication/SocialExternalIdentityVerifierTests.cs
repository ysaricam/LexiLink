using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Players.Domain.Players;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class SocialExternalIdentityVerifierTests
{
    private const string AppleAudience = "com.wordlope.app";
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string KeyId = "apple-test-key";

    [Test]
    public async Task VerifyAsync_WithValidAppleJwt_ReturnsTrue()
    {
        using var tokenFactory = new AppleTokenFactory();
        var verifier = CreateVerifier(tokenFactory.Jwks, [AppleAudience]);
        var externalId = "apple-user-1";
        var token = tokenFactory.CreateToken(externalId, AppleAudience);

        var verified = await verifier.VerifyAsync(AuthProvider.Apple, externalId, token);

        verified.Should().BeTrue();
    }

    [Test]
    public async Task VerifyAsync_WithWrongAppleAudience_ReturnsFalse()
    {
        using var tokenFactory = new AppleTokenFactory();
        var verifier = CreateVerifier(tokenFactory.Jwks, [AppleAudience]);
        var token = tokenFactory.CreateToken("apple-user-1", "wrong.bundle.id");

        var verified = await verifier.VerifyAsync(AuthProvider.Apple, "apple-user-1", token);

        verified.Should().BeFalse();
    }

    [Test]
    public async Task VerifyAsync_WithMismatchedAppleSubject_ReturnsFalse()
    {
        using var tokenFactory = new AppleTokenFactory();
        var verifier = CreateVerifier(tokenFactory.Jwks, [AppleAudience]);
        var token = tokenFactory.CreateToken("apple-user-1", AppleAudience);

        var verified = await verifier.VerifyAsync(AuthProvider.Apple, "different-apple-user", token);

        verified.Should().BeFalse();
    }

    [Test]
    public async Task VerifyAsync_WithExpiredAppleJwt_ReturnsFalse()
    {
        using var tokenFactory = new AppleTokenFactory();
        var verifier = CreateVerifier(tokenFactory.Jwks, [AppleAudience]);
        var token = tokenFactory.CreateToken(
            "apple-user-1",
            AppleAudience,
            expiresAt: DateTime.UtcNow.AddMinutes(-10));

        var verified = await verifier.VerifyAsync(AuthProvider.Apple, "apple-user-1", token);

        verified.Should().BeFalse();
    }

    [Test]
    public async Task VerifyAsync_WithNoConfiguredAppleAudience_ReturnsFalse()
    {
        using var tokenFactory = new AppleTokenFactory();
        var verifier = CreateVerifier(tokenFactory.Jwks, []);
        var token = tokenFactory.CreateToken("apple-user-1", AppleAudience);

        var verified = await verifier.VerifyAsync(AuthProvider.Apple, "apple-user-1", token);

        verified.Should().BeFalse();
    }

    [Test]
    public async Task VerifyAsync_WithGuestProvider_StillAcceptsGuestHandshake()
    {
        using var tokenFactory = new AppleTokenFactory();
        var verifier = CreateVerifier(tokenFactory.Jwks, [AppleAudience]);
        var deviceId = Guid.NewGuid().ToString();

        var verified = await verifier.VerifyAsync(
            AuthProvider.Guest,
            deviceId,
            $"dev:Guest:{deviceId}");

        verified.Should().BeTrue();
    }

    private static SocialExternalIdentityVerifier CreateVerifier(string jwks, string[] appleClientIds)
    {
        var options = new LexiLinkAuthOptions
        {
            SocialIdentity = new SocialIdentityOptions
            {
                AppleClientIds = appleClientIds
            }
        };
        var httpClient = new HttpClient(new StubHttpMessageHandler(jwks))
        {
            BaseAddress = new Uri("https://appleid.apple.com")
        };

        return new SocialExternalIdentityVerifier(options, httpClient);
    }

    private sealed class AppleTokenFactory : IDisposable
    {
        private readonly RSA _rsa = RSA.Create(2048);

        public string Jwks
        {
            get
            {
                var parameters = _rsa.ExportParameters(false);
                return JsonSerializer.Serialize(new
                {
                    keys = new[]
                    {
                        new
                        {
                            kty = "RSA",
                            use = "sig",
                            kid = KeyId,
                            alg = SecurityAlgorithms.RsaSha256,
                            n = Base64UrlEncoder.Encode(parameters.Modulus),
                            e = Base64UrlEncoder.Encode(parameters.Exponent)
                        }
                    }
                });
            }
        }

        public string CreateToken(
            string subject,
            string audience,
            DateTime? expiresAt = null)
        {
            var key = new RsaSecurityKey(_rsa)
            {
                KeyId = KeyId
            };
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = AppleIssuer,
                Audience = audience,
                Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, subject)]),
                NotBefore = DateTime.UtcNow.AddMinutes(-5),
                Expires = expiresAt ?? DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }

        public void Dispose()
        {
            _rsa.Dispose();
        }
    }

    private sealed class StubHttpMessageHandler(string jwks) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.RequestUri!.ToString().Should().Be("https://appleid.apple.com/auth/keys");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jwks)
            });
        }
    }
}
