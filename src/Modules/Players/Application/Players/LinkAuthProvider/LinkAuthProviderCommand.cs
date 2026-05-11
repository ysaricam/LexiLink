using LexiLink.Modules.Players.Application.Contracts;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Players.LinkAuthProvider;

public class LinkAuthProviderCommand : CommandBase
{
    public Guid PlayerId { get; }
    public AuthProvider Provider { get; }
    public string ExternalId { get; }
    public string? Email { get; }

    public LinkAuthProviderCommand(Guid playerId, AuthProvider provider, string externalId, string? email)
    {
        PlayerId = playerId;
        Provider = provider;
        ExternalId = externalId;
        Email = email;
    }
}
