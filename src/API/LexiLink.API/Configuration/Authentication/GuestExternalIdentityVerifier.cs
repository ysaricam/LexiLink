using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Configuration.Authentication;

/// <summary>
/// Production-safe identity verifier for the <see cref="AuthProvider.Guest"/> flow only.
/// A guest's identity is its client-generated device id (a high-entropy random value the
/// device keeps), which is the actual bearer credential — the external token is a
/// client-reproducible handshake, not independent proof. Apple/Google are rejected here
/// because real social sign-in (server-side ID-token verification) is not wired yet; they
/// return <c>false</c> until that lands. This keeps the guest-first game playable in
/// Production without trusting any unimplemented social provider.
/// </summary>
public sealed class GuestExternalIdentityVerifier : IExternalIdentityVerifier
{
    public Task<bool> VerifyAsync(
        AuthProvider provider,
        string externalId,
        string externalToken,
        CancellationToken cancellationToken = default)
    {
        if (provider != AuthProvider.Guest)
        {
            return Task.FromResult(false);
        }

        // Same handshake the client already produces, gated to the Guest provider.
        var expectedToken = $"dev:{AuthProvider.Guest}:{externalId}";

        return Task.FromResult(
            !string.IsNullOrWhiteSpace(externalId)
            && string.Equals(externalToken, expectedToken, StringComparison.Ordinal));
    }
}
