using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Configuration.Authentication;

public sealed class DisabledExternalIdentityVerifier : IExternalIdentityVerifier
{
    public Task<bool> VerifyAsync(
        AuthProvider provider,
        string externalId,
        string externalToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
