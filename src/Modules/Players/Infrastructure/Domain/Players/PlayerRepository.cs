using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Players.Domain.Players;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Players.Infrastructure.Domain.Players;

internal class PlayerRepository : IPlayerRepository
{
    private readonly PlayersContext _playersContext;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal PlayerRepository(PlayersContext playersContext, ISqlConnectionFactory sqlConnectionFactory)
    {
        _playersContext = playersContext;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Player?> GetByIdAsync(PlayerId id, CancellationToken cancellationToken = default)
    {
        return await _playersContext.Players.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Player?> GetByHandleAsync(
        string displayName,
        int discriminator,
        CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT "Id"
            FROM "players"."Players"
            WHERE "DisplayName" = @DisplayName
              AND "DiscriminatorValue" = @Discriminator
        """;

        var playerIdValue = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(
                sql,
                new { DisplayName = displayName, Discriminator = discriminator },
                cancellationToken: cancellationToken));

        if (playerIdValue is null)
            return null;

        return await GetByIdAsync(new PlayerId(playerIdValue.Value), cancellationToken);
    }

    public async Task<Player?> GetByAuthProviderAsync(AuthProvider provider, string externalId, CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT "PlayerId"
            FROM "players"."PlayerAuthIdentities"
            WHERE "Provider" = @Provider AND "ExternalId" = @ExternalId
        """;

        var playerIdValue = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(
                sql,
                new { Provider = provider.ToString(), ExternalId = externalId },
                cancellationToken: cancellationToken));

        if (playerIdValue is null)
            return null;

        var playerId = new PlayerId(playerIdValue.Value);
        return await _playersContext.Players.FirstOrDefaultAsync(p => p.Id == playerId, cancellationToken);
    }

    public async Task AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        await _playersContext.Players.AddAsync(player, cancellationToken);
    }
}
