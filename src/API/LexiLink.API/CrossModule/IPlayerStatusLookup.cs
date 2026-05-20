namespace LexiLink.API.CrossModule;

/// <summary>
/// API-host facing query gateway over the Players module's per-id
/// ban state. The authentication handler calls this on every
/// request to refuse tokens that map to a banned player. Returns
/// false when the player doesn't exist yet (a freshly-issued dev
/// bearer GUID that hasn't called <c>POST /players/guest</c> is
/// allowed through so registration can complete).
/// </summary>
public interface IPlayerStatusLookup
{
    Task<bool> IsPlayerBannedAsync(Guid playerId, CancellationToken cancellationToken = default);
}
