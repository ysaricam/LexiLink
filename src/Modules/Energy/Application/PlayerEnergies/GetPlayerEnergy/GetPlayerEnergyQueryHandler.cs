using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Energy.Application.Configuration.Queries;
using LexiLink.Modules.Energy.Domain.PlayerEnergies;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GetPlayerEnergy;

internal class GetPlayerEnergyQueryHandler : IQueryHandler<GetPlayerEnergyQuery, PlayerEnergySnapshotDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal GetPlayerEnergyQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public async Task<PlayerEnergySnapshotDto> Handle(GetPlayerEnergyQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Energy"."PlayerId"                AS "PlayerId",
                "Energy"."CurrentAmount"           AS "CurrentAmount",
                "Energy"."MaximumAmount"           AS "MaximumAmount",
                "Energy"."RechargeIntervalSeconds" AS "RechargeIntervalSeconds",
                "Energy"."LastRefilledOn"          AS "LastRefilledOn"
            FROM "energy"."v_PlayerEnergies" AS "Energy"
            WHERE "Energy"."PlayerId" = @PlayerId;
        """;

        var raw = await connection.QuerySingleOrDefaultAsync<RawPlayerEnergyRow>(
            new CommandDefinition(
                sql,
                new { query.PlayerId },
                cancellationToken: cancellationToken));

        if (raw is null)
        {
            throw new NotFoundException(nameof(PlayerEnergy), query.PlayerId);
        }

        var now = _clock.UtcNow;
        var projection = EnergyRefillCalculator.Project(
            raw.CurrentAmount,
            raw.MaximumAmount,
            DateTime.SpecifyKind(raw.LastRefilledOn, DateTimeKind.Utc),
            raw.RechargeIntervalSeconds,
            now);

        var isFull = projection.CurrentAmount >= raw.MaximumAmount;

        int? secondsUntilNextRefill = isFull
            ? null
            : ComputeSecondsUntilNextRefill(projection.LastRefilledOn, raw.RechargeIntervalSeconds, now);

        DateTime? fullyRefilledAt = isFull
            ? null
            : ComputeFullyRefilledAt(
                projection.CurrentAmount,
                raw.MaximumAmount,
                projection.LastRefilledOn,
                raw.RechargeIntervalSeconds);

        return new PlayerEnergySnapshotDto(
            PlayerId: raw.PlayerId,
            CurrentAmount: projection.CurrentAmount,
            MaximumAmount: raw.MaximumAmount,
            IsFull: isFull,
            RechargeIntervalSeconds: raw.RechargeIntervalSeconds,
            LastRefilledOn: projection.LastRefilledOn,
            SecondsUntilNextRefill: secondsUntilNextRefill,
            FullyRefilledAt: fullyRefilledAt);
    }

    private static int ComputeSecondsUntilNextRefill(DateTime lastRefilledOn, int rechargeIntervalSeconds, DateTime now)
    {
        var nextRefillAt = lastRefilledOn.AddSeconds(rechargeIntervalSeconds);
        var remaining = (nextRefillAt - now).TotalSeconds;
        if (remaining <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(remaining);
    }

    private static DateTime ComputeFullyRefilledAt(
        int currentAmount,
        int maximumAmount,
        DateTime lastRefilledOn,
        int rechargeIntervalSeconds)
    {
        var slotsRemaining = maximumAmount - currentAmount;
        return lastRefilledOn.AddSeconds((double)slotsRemaining * rechargeIntervalSeconds);
    }

    private sealed class RawPlayerEnergyRow
    {
        public Guid PlayerId { get; init; }
        public int CurrentAmount { get; init; }
        public int MaximumAmount { get; init; }
        public int RechargeIntervalSeconds { get; init; }
        public DateTime LastRefilledOn { get; init; }
    }
}
