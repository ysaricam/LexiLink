using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players;

public sealed class AuthIdentity : ValueObject
{
    public AuthProvider Provider { get; }
    public string ExternalId { get; }
    public string? Email { get; }
    public DateTime LinkedAt { get; }

    private AuthIdentity()
    {
        ExternalId = null!;
    }

    internal AuthIdentity(AuthProvider provider, string externalId, string? email, DateTime linkedAt)
    {
        Provider = provider;
        ExternalId = externalId;
        Email = email;
        LinkedAt = linkedAt;
    }
}
