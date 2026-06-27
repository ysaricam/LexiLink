using LexiLink.Modules.Ads.Application.Configuration.Verification;
using LexiLink.Modules.Ads.Infrastructure.Configuration.Verification;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace LexiLink.Modules.Ads.Tests.Configuration.Verification;

[TestFixture]
public class AdMobSsvVerifierTests
{
    [Test]
    public async Task VerifyAsync_WithTrustedKeyAndValidSignature_ReturnsVerified()
    {
        using var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signedContent = "ad_network=5450213213286189855&ad_unit=3077352370&reward_amount=5" +
            "&reward_item=diamonds&timestamp=150777823&transaction_id=tx-1&user_id=user-1";
        var signature = signingKey.SignData(
            Encoding.UTF8.GetBytes(signedContent),
            HashAlgorithmName.SHA256,
            DSASignatureFormat.Rfc3279DerSequence);

        var verifier = CreateVerifier(signingKey, keyId: "123");

        var result = await verifier.VerifyAsync(
            new AdMobSsvVerificationRequest(
                signedContent,
                EncodeBase64Url(signature),
                "123",
                "tx-1",
                "user-1"));

        result.IsVerified.Should().BeTrue();
    }

    [Test]
    public async Task VerifyAsync_WithTamperedSignedContent_ReturnsFailed()
    {
        using var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var originalContent = "ad_network=5450213213286189855&transaction_id=tx-1&user_id=user-1";
        var signature = signingKey.SignData(
            Encoding.UTF8.GetBytes(originalContent),
            HashAlgorithmName.SHA256,
            DSASignatureFormat.Rfc3279DerSequence);

        var verifier = CreateVerifier(signingKey, keyId: "123");

        var result = await verifier.VerifyAsync(
            new AdMobSsvVerificationRequest(
                originalContent + "&reward_amount=999",
                EncodeBase64Url(signature),
                "123",
                "tx-1",
                "user-1"));

        result.IsVerified.Should().BeFalse();
    }

    private static AdMobSsvVerifier CreateVerifier(ECDsa signingKey, string keyId)
    {
        var publicKey = Convert.ToBase64String(signingKey.ExportSubjectPublicKeyInfo());
        var payload = $$"""
            {
              "keys": [
                {
                  "keyId": {{keyId}},
                  "base64": "{{publicKey}}"
                }
              ]
            }
            """;

        var httpClient = new HttpClient(new StubHttpMessageHandler(payload));
        return new AdMobSsvVerifier(
            new AdsSsvOptions { VerificationKeysUrl = "https://www.gstatic.com/admob/reward/verifier-keys.json" },
            httpClient);
    }

    private static string EncodeBase64Url(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _payload;

        public StubHttpMessageHandler(string payload)
        {
            _payload = payload;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_payload, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
