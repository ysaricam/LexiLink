using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Tests.Authentication;

[TestFixture]
public sealed class GuestExternalIdentityVerifierTests
{
    private readonly GuestExternalIdentityVerifier _verifier = new();

    [Test]
    public async Task Verify_Should_Accept_Guest_With_Matching_Handshake_Token()
    {
        var deviceId = Guid.NewGuid().ToString();

        var verified = await _verifier.VerifyAsync(
            AuthProvider.Guest, deviceId, $"dev:Guest:{deviceId}");

        Assert.That(verified, Is.True);
    }

    [Test]
    public async Task Verify_Should_Reject_Guest_With_Mismatched_Token()
    {
        var deviceId = Guid.NewGuid().ToString();

        var verified = await _verifier.VerifyAsync(
            AuthProvider.Guest, deviceId, $"dev:Guest:{Guid.NewGuid()}");

        Assert.That(verified, Is.False);
    }

    [Test]
    public async Task Verify_Should_Reject_Empty_ExternalId()
    {
        var verified = await _verifier.VerifyAsync(
            AuthProvider.Guest, string.Empty, "dev:Guest:");

        Assert.That(verified, Is.False);
    }

    [TestCase(AuthProvider.Apple)]
    [TestCase(AuthProvider.Google)]
    public async Task Verify_Should_Reject_Non_Guest_Providers(AuthProvider provider)
    {
        var externalId = Guid.NewGuid().ToString();

        // Even a well-formed token for a social provider is rejected: real
        // social sign-in verification is not wired yet.
        var verified = await _verifier.VerifyAsync(
            provider, externalId, $"dev:{provider}:{externalId}");

        Assert.That(verified, Is.False);
    }
}
