using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Infrastructure.Domain.Players;

internal class RandomDiscriminatorGenerator : IDiscriminatorGenerator
{
    private const int RandomAttempts = 10;

    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly Random _random;

    internal RandomDiscriminatorGenerator(ISqlConnectionFactory sqlConnectionFactory, Random random)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _random = random;
    }

    public async Task<Discriminator> GenerateForAsync(string displayName, CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT "DiscriminatorValue"
            FROM "players"."Players"
            WHERE "DisplayName" = @DisplayName
        """;

        var taken = (await connection.QueryAsync<int>(
            new CommandDefinition(sql, new { DisplayName = displayName }, cancellationToken: cancellationToken)
        )).ToHashSet();

        var capacity = Discriminator.MaxValue - Discriminator.MinValue + 1;
        if (taken.Count >= capacity)
            throw new InvalidOperationException(
                $"All {capacity} discriminators for display name '{displayName}' are taken.");

        for (var attempt = 0; attempt < RandomAttempts; attempt++)
        {
            var candidate = _random.Next(Discriminator.MinValue, Discriminator.MaxValue + 1);
            if (!taken.Contains(candidate))
                return Discriminator.Of(candidate);
        }

        for (var value = Discriminator.MinValue; value <= Discriminator.MaxValue; value++)
        {
            if (!taken.Contains(value))
                return Discriminator.Of(value);
        }

        throw new InvalidOperationException(
            $"All {capacity} discriminators for display name '{displayName}' are taken.");
    }
}
