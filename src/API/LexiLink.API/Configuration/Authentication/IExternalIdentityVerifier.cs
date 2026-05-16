using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Configuration.Authentication;

public interface IExternalIdentityVerifier
{
    Task<bool> VerifyAsync(
        AuthProvider provider,
        string externalId,
        string externalToken,
        CancellationToken cancellationToken = default);
}
