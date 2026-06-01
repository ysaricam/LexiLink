using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Ads.Application.Configuration.Queries;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants;

namespace LexiLink.Modules.Ads.Application.RewardedAdGrants.GetRewardedAdStatus;

internal sealed class GetRewardedAdStatusQueryHandler
    : IQueryHandler<GetRewardedAdStatusQuery, RewardedAdStatusDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IAdsConfigurationService _configuration;
    private readonly IClock _clock;

    internal GetRewardedAdStatusQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IAdsConfigurationService configuration,
        IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _configuration = configuration;
        _clock = clock;
    }

    public async Task<RewardedAdStatusDto> Handle(
        GetRewardedAdStatusQuery query,
        CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT COUNT(*)
            FROM "ads"."RewardedAdGrants"
            WHERE "PlayerId" = @PlayerId
              AND "GrantedOn" >= @SinceUtc;
        """;

        var grantsToday = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { query.PlayerId, SinceUtc = _clock.UtcNow.Date },
                cancellationToken: cancellationToken));

        var dailyLimit = _configuration.RewardedDailyLimit;

        return new RewardedAdStatusDto(
            grantsToday,
            dailyLimit,
            Math.Max(0, dailyLimit - grantsToday),
            _configuration.RewardedDiamondAmount);
    }
}
