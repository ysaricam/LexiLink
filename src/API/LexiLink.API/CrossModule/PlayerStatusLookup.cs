using LexiLink.Modules.Players.Application.Admin.GetPlayerBanStatus;
using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.API.CrossModule;

internal sealed class PlayerStatusLookup : IPlayerStatusLookup
{
    private readonly IPlayersModule _players;

    public PlayerStatusLookup(IPlayersModule players)
    {
        _players = players;
    }

    public Task<bool> IsPlayerBannedAsync(Guid playerId, CancellationToken cancellationToken = default) =>
        _players.ExecuteQueryAsync(new GetPlayerBanStatusQuery(playerId), cancellationToken);
}
