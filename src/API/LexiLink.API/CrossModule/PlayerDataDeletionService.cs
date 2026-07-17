using Dapper;
using LexiLink.Common.Application.Time;
using Npgsql;

namespace LexiLink.API.CrossModule;

/// <summary>
/// Compliance boundary for account deletion. Operational player data is
/// physically removed; the Players row becomes a non-identifying tombstone.
/// Payment and administration records are retained only after unlinking and
/// redacting the player identifier. Everything runs in one database transaction.
/// </summary>
internal sealed class PlayerDataDeletionService : IPlayerDataDeletionService
{
    private readonly string _connectionString;
    private readonly IClock _clock;

    public PlayerDataDeletionService(string connectionString, IClock clock)
    {
        _connectionString = connectionString;
        _clock = clock;
    }

    public async Task<bool> DeleteAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string existsSql = """
            SELECT "IsDeleted"
            FROM "players"."Players"
            WHERE "Id" = @PlayerId
            FOR UPDATE;
            """;
        var isDeleted = await connection.QuerySingleOrDefaultAsync<bool?>(
            new CommandDefinition(
                existsSql,
                new { PlayerId = playerId },
                transaction,
                cancellationToken: cancellationToken));

        if (isDeleted is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        if (isDeleted.Value)
        {
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        var tombstoneId = Guid.NewGuid();
        var now = _clock.UtcNow;
        var playerIdText = playerId.ToString();

        const string sql = """
            DELETE FROM "players"."PlayerAuthIdentities" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "games"."Games" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "stats"."PlayerPeriodStats" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "stats"."PlayerStats" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "hint"."PlayerHintInventories" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "undo"."PlayerUndoInventories" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "reset"."PlayerResetInventories" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "diamond"."PlayerDiamondInventories" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "market"."PurchaseOrders" WHERE "PlayerId" = @PlayerId;
            DELETE FROM "ads"."RewardedAdGrants" WHERE "PlayerId" = @PlayerId;

            UPDATE "payments"."IapPurchases"
            SET "PlayerId" = @TombstoneId,
                "ClientRequestId" = NULL
            WHERE "PlayerId" = @PlayerId;

            UPDATE "administration"."AdminActionAudit"
            SET "TargetId" = CASE WHEN "TargetId" = @PlayerIdText THEN '[deleted-player]' ELSE "TargetId" END,
                "PayloadJson" = replace("PayloadJson", @PlayerIdText, '[deleted-player]')
            WHERE "TargetId" = @PlayerIdText OR "PayloadJson" LIKE '%' || @PlayerIdText || '%';

            DELETE FROM "players"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "games"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "stats"."InboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "stats"."InternalCommands" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "energy"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "quests"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "hint"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "undo"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "reset"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "diamond"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "market"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "ads"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "payments"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "payments"."InboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "administration"."OutboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';
            DELETE FROM "administration"."InboxMessages" WHERE "Data" LIKE '%' || @PlayerIdText || '%';

            UPDATE "players"."Players"
            SET "DisplayName" = 'Deleted-' || replace(@TombstoneId::text, '-', ''),
                "DiscriminatorValue" = 1,
                "AvatarUrl" = NULL,
                "Locale" = 'en-US',
                "IsGuest" = TRUE,
                "IsBanned" = TRUE,
                "BannedReason" = 'Deleted account',
                "BannedAt" = @Now,
                "IsDeleted" = TRUE,
                "DeletedAt" = @Now
            WHERE "Id" = @PlayerId;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    PlayerId = playerId,
                    TombstoneId = tombstoneId,
                    PlayerIdText = playerIdText,
                    Now = now
                },
                transaction,
                cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
