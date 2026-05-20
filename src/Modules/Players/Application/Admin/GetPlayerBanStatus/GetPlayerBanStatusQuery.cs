using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Admin.GetPlayerBanStatus;

/// <summary>
/// Cheap lookup used by the API host auth boundary to refuse banned
/// player tokens. Returns false when the player doesn't exist (e.g. a
/// freshly-issued dev bearer GUID that hasn't called /players/guest
/// yet) so the auth handler only rejects confirmed bans, never
/// pre-registration calls.
/// </summary>
public sealed class GetPlayerBanStatusQuery : QueryBase<bool>
{
    public Guid PlayerId { get; }

    public GetPlayerBanStatusQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
