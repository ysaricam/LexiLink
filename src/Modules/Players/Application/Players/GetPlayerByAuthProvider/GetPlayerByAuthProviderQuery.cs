using LexiLink.Modules.Players.Application.Contracts;
using LexiLink.Modules.Players.Application.Players.GetPlayerById;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Players.GetPlayerByAuthProvider;

public class GetPlayerByAuthProviderQuery : QueryBase<PlayerDetailsDto?>
{
    public AuthProvider Provider { get; }
    public string ExternalId { get; }

    public GetPlayerByAuthProviderQuery(AuthProvider provider, string externalId)
    {
        Provider = provider;
        ExternalId = externalId;
    }
}
