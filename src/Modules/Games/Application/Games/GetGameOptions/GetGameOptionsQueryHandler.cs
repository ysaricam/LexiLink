using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Queries;
using LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;

namespace LexiLink.Modules.Games.Application.Games.GetGameOptions;

internal class GetGameOptionsQueryHandler : IQueryHandler<GetGameOptionsQuery, List<OutgoingLinkDto>>
{
    private const int OptionLimit = 6;

    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetGameOptionsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<OutgoingLinkDto>> Handle(GetGameOptionsQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        // History only records steps the player has taken; the start link is
        // implicit via Game.StartLinkId. So "previous" resolves as:
        //   history count 0          -> no previous (player is at start)
        //   history count 1          -> StartLinkId (the player came from start)
        //   history count >= 2       -> the second-to-last history entry
        const string gameSql = """
            SELECT
                "Game"."CurrentLinkId" AS "CurrentLinkId",
                "Game"."StartLinkId"   AS "StartLinkId",
                "Game"."TargetLinkId"  AS "TargetLinkId",
                "Game"."CategoryId"    AS "CategoryId",
                "Counts"."HistoryCount" AS "HistoryCount",
                (
                    SELECT "LinkId"
                    FROM "games"."v_GameHistory"
                    WHERE "GameId" = @GameId
                    ORDER BY "StepNumber" DESC
                    OFFSET 1 LIMIT 1
                ) AS "HistoryPrev"
            FROM "games"."v_Games" AS "Game"
            CROSS JOIN LATERAL (
                SELECT COUNT(*)::int AS "HistoryCount"
                FROM "games"."v_GameHistory"
                WHERE "GameId" = @GameId
            ) AS "Counts"
            WHERE "Game"."Id" = @GameId;
        """;

        var gameRow = await connection.QuerySingleOrDefaultAsync<GameRow>(
            new CommandDefinition(
                gameSql,
                new { query.GameId },
                cancellationToken: cancellationToken
            )
        );

        if (gameRow is null)
        {
            throw new NotFoundException("Game", query.GameId);
        }

        var previousLinkId = gameRow.HistoryCount switch
        {
            0 => (Guid?)null,
            1 => gameRow.StartLinkId,
            _ => gameRow.HistoryPrev,
        };

        const string candidatesSql = """
            SELECT
                "Outgoing"."OutgoingLinkId" AS "Id",
                "Target"."Value"            AS "Value",
                "Target"."IsActive"         AS "IsActive",
                (
                    SELECT COUNT(*)::int
                    FROM "games"."LinkOutgoingLinks" AS "Sub"
                    WHERE "Sub"."LinkId" = "Outgoing"."OutgoingLinkId"
                )                           AS "Degree"
            FROM "games"."LinkOutgoingLinks" AS "Outgoing"
            INNER JOIN "games"."v_Links" AS "Target" ON "Target"."Id" = "Outgoing"."OutgoingLinkId"
            WHERE "Outgoing"."LinkId" = @CurrentLinkId;
        """;

        var candidates = (await connection.QueryAsync<CandidateRow>(
            new CommandDefinition(
                candidatesSql,
                new { gameRow.CurrentLinkId },
                cancellationToken: cancellationToken
            )
        )).ToList();

        if (candidates.Count == 0)
        {
            return new List<OutgoingLinkDto>();
        }

        var byId = candidates.ToDictionary(c => c.Id);
        var candidateIdSet = new HashSet<Guid>(byId.Keys);

        // Resolve a candidate that lies on a shortest path to the target so the
        // density heuristic cannot silently strip target reachability. BFS runs
        // over the game's category subgraph and returns the first hop from
        // currentLinkId toward targetLinkId.
        var pathToTargetLinkId = await ResolvePathToTargetAsync(
            connection,
            gameRow,
            candidateIdSet,
            cancellationToken);

        if (candidates.Count <= OptionLimit)
        {
            return Order(candidates.Select(c => c.Id).ToList(), previousLinkId, pathToTargetLinkId)
                .Select(id => ToDto(byId[id]))
                .ToList();
        }

        var candidateIds = candidates.Select(c => c.Id).ToArray();

        const string pairwiseSql = """
            SELECT
                LEAST("A"."LinkId", "B"."LinkId")    AS "LeftId",
                GREATEST("A"."LinkId", "B"."LinkId") AS "RightId",
                COUNT(*)::int                         AS "Common"
            FROM "games"."LinkOutgoingLinks" AS "A"
            INNER JOIN "games"."LinkOutgoingLinks" AS "B"
                ON "A"."OutgoingLinkId" = "B"."OutgoingLinkId"
               AND "A"."LinkId" < "B"."LinkId"
            WHERE "A"."LinkId" = ANY(@CandidateIds)
              AND "B"."LinkId" = ANY(@CandidateIds)
            GROUP BY "A"."LinkId", "B"."LinkId";
        """;

        var pairs = await connection.QueryAsync<PairwiseRow>(
            new CommandDefinition(
                pairwiseSql,
                new { CandidateIds = candidateIds },
                cancellationToken: cancellationToken
            )
        );

        var pairwise = pairs.ToDictionary(
            p => (p.LeftId, p.RightId),
            p => p.Common);
        var degrees = candidates.ToDictionary(c => c.Id, c => c.Degree);

        var selectedIds = OutgoingLinkSelector.Select(
            candidates: candidateIds,
            degrees: degrees,
            pairwiseCommon: pairwise,
            previousLinkId: previousLinkId,
            pathToTargetLinkId: pathToTargetLinkId,
            limit: OptionLimit);

        return selectedIds.Select(id => ToDto(byId[id])).ToList();
    }

    private static async Task<Guid?> ResolvePathToTargetAsync(
        System.Data.IDbConnection connection,
        GameRow gameRow,
        HashSet<Guid> candidateIdSet,
        CancellationToken cancellationToken)
    {
        if (gameRow.CurrentLinkId == gameRow.TargetLinkId)
        {
            return null;
        }

        if (candidateIdSet.Contains(gameRow.TargetLinkId))
        {
            return gameRow.TargetLinkId;
        }

        const string adjacencySql = """
            SELECT "Outgoing"."LinkId" AS "LinkId", "Outgoing"."OutgoingLinkId" AS "OutgoingLinkId"
            FROM "games"."LinkOutgoingLinks" AS "Outgoing"
            INNER JOIN "games"."Links" AS "Source" ON "Source"."Id" = "Outgoing"."LinkId"
            INNER JOIN "games"."Links" AS "Target" ON "Target"."Id" = "Outgoing"."OutgoingLinkId"
            WHERE "Source"."CategoryId" = @CategoryId
              AND "Target"."CategoryId" = @CategoryId;
        """;

        var edges = await connection.QueryAsync<AdjacencyRow>(
            new CommandDefinition(
                adjacencySql,
                new { gameRow.CategoryId },
                cancellationToken: cancellationToken
            )
        );

        var adjacency = new Dictionary<Guid, List<Guid>>();
        foreach (var edge in edges)
        {
            if (!adjacency.TryGetValue(edge.LinkId, out var list))
            {
                list = new List<Guid>();
                adjacency[edge.LinkId] = list;
            }
            list.Add(edge.OutgoingLinkId);
        }

        // Deterministic neighbor order so BFS tie-breaks the same way each run.
        foreach (var list in adjacency.Values)
        {
            list.Sort();
        }

        return FindFirstHop(adjacency, gameRow.CurrentLinkId, gameRow.TargetLinkId);
    }

    private static Guid? FindFirstHop(
        Dictionary<Guid, List<Guid>> adjacency,
        Guid source,
        Guid target)
    {
        // Parent map carries the first-hop a node was reached through, so we
        // recover "the candidate to lock" with O(1) at the end instead of
        // walking the parent chain back.
        var firstHopByNode = new Dictionary<Guid, Guid>();
        var queue = new Queue<Guid>();

        if (!adjacency.TryGetValue(source, out var sourceNeighbors))
        {
            return null;
        }

        foreach (var neighbor in sourceNeighbors)
        {
            if (firstHopByNode.ContainsKey(neighbor))
            {
                continue;
            }
            firstHopByNode[neighbor] = neighbor;
            if (neighbor == target)
            {
                return neighbor;
            }
            queue.Enqueue(neighbor);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!adjacency.TryGetValue(current, out var neighbors))
            {
                continue;
            }
            foreach (var neighbor in neighbors)
            {
                if (neighbor == source || firstHopByNode.ContainsKey(neighbor))
                {
                    continue;
                }
                firstHopByNode[neighbor] = firstHopByNode[current];
                if (neighbor == target)
                {
                    return firstHopByNode[neighbor];
                }
                queue.Enqueue(neighbor);
            }
        }

        return null;
    }

    private static List<Guid> Order(List<Guid> ids, Guid? previousLinkId, Guid? pathToTargetLinkId)
    {
        var sorted = ids.OrderBy(id => id).ToList();
        if (pathToTargetLinkId is { } target
            && target != previousLinkId
            && sorted.Contains(target))
        {
            sorted.Remove(target);
            sorted.Insert(0, target);
        }
        if (previousLinkId is { } prev && sorted.Contains(prev))
        {
            sorted.Remove(prev);
            sorted.Insert(0, prev);
        }
        return sorted;
    }

    private static OutgoingLinkDto ToDto(CandidateRow row) =>
        new(row.Id, row.Value, row.IsActive);

    private sealed record GameRow(
        Guid CurrentLinkId,
        Guid StartLinkId,
        Guid TargetLinkId,
        Guid CategoryId,
        int HistoryCount,
        Guid? HistoryPrev);

    private sealed record AdjacencyRow(Guid LinkId, Guid OutgoingLinkId);

    private sealed record CandidateRow(Guid Id, string Value, bool IsActive, int Degree);

    private sealed record PairwiseRow(Guid LeftId, Guid RightId, int Common);
}
