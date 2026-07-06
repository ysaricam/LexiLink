using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Undo.Application.Configuration.Queries;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

namespace LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;

internal class GetPlayerUndoQueryHandler : IQueryHandler<GetPlayerUndoQuery, PlayerUndoSnapshotDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUndoConfigurationService _undoConfiguration;

    internal GetPlayerUndoQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUndoConfigurationService undoConfiguration)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _undoConfiguration = undoConfiguration;
    }

    public async Task<PlayerUndoSnapshotDto> Handle(GetPlayerUndoQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "PlayerId" AS "PlayerId",
                "Balance"  AS "Balance"
            FROM "undo"."PlayerUndoInventories"
            WHERE "PlayerId" = @PlayerId;
        """;

        var snapshot = await connection.QuerySingleOrDefaultAsync<PlayerUndoSnapshotDto>(
            new CommandDefinition(
                sql,
                new { query.PlayerId },
                cancellationToken: cancellationToken));

        var presentedSnapshot = PlayerUndoSnapshotPresenter.PresentOrCreateForGameplay(
            snapshot,
            query.PlayerId,
            _undoConfiguration,
            query.UseGameplayPresentation);

        if (presentedSnapshot is null)
        {
            throw new NotFoundException(nameof(PlayerUndoInventory), query.PlayerId);
        }

        return presentedSnapshot;
    }
}
