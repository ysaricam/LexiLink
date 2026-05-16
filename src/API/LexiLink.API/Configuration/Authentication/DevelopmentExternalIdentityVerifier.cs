using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Configuration.Authentication;

public sealed class DevelopmentExternalIdentityVerifier : IExternalIdentityVerifier
{
    public Task<bool> VerifyAsync(
        AuthProvider provider,
        string externalId,
        string externalToken,
        CancellationToken cancellationToken = default)
    {
        var expectedToken = $"dev:{provider}:{externalId}";

        return Task.FromResult(string.Equals(externalToken, expectedToken, StringComparison.Ordinal));
    }
}
