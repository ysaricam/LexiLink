using Dapper;
using LexiLink.Modules.Games.IntegrationEvents;
using LexiLink.Modules.Games.Application.Categories.CreateCategory;
using LexiLink.Modules.Games.Application.Games.CreateGame;
using LexiLink.Modules.Games.Application.Games.GetGameById;
using LexiLink.Modules.Games.Application.Games.MakeStep;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.Application.Links.AddOutgoingLink;
using LexiLink.Modules.Games.Application.Links.CreateLink;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Players.Application.Players.LinkAuthProvider;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;
using LexiLink.Modules.Players.Domain.Players;
using LexiLink.Modules.Stats.Application.PlayerStats.GetLeaderboard;
using LexiLink.Modules.Stats.Application.PlayerStats.GetPlayerStats;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;
using LexiLink.Modules.Stats.IntegrationTests.SeedWork;
using Npgsql;

namespace LexiLink.Modules.Stats.IntegrationTests.PlayerStats;

[TestFixture]
public class PlayerStatsProjectionTests : TestBase
{
    [Test]
    public async Task PlayerLifecycle_OutboxProcessor_ProjectsStats()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-1", "Yasin", "en-US"));

        await ProcessOutboxAsync();
        await ProcessStatsInboxAsync();

        var registeredStats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        registeredStats.Should().NotBeNull();
        registeredStats!.DisplayName.Should().Be("Yasin");
        registeredStats.Handle.Should().StartWith("Yasin#");
        registeredStats.Locale.Should().Be("en-US");
        registeredStats.IsGuest.Should().BeTrue();
        registeredStats.GamesCompleted.Should().Be(0);

        await ExecuteCommandAsync(
            new LinkAuthProviderCommand(
                playerId,
                AuthProvider.Apple,
                "apple-sub-1",
                "yasin@example.com"));

        await ProcessOutboxAsync();
        await ProcessStatsInboxAsync();

        var linkedStats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        linkedStats.Should().NotBeNull();
        linkedStats!.IsGuest.Should().BeFalse();
        linkedStats.AuthProvidersLinked.Should().Be(1);

        await ExecuteCommandAsync(
            new UpdatePlayerProfileCommand(
                playerId,
                "https://example.com/avatar.png",
                "tr-TR"));

        await ProcessOutboxAsync();
        await ProcessStatsInboxAsync();

        var updatedStats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        updatedStats.Should().NotBeNull();
        updatedStats!.AvatarUrl.Should().Be("https://example.com/avatar.png");
        updatedStats.Locale.Should().Be("tr-TR");
    }

    [Test]
    public async Task GameCompletedIntegrationEvent_WhenPublishedTwice_IsIdempotent()
    {
        var playerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var integrationEvent = new GameCompletedIntegrationEvent(
            eventId,
            DateTime.UtcNow,
            Guid.NewGuid(),
            playerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            300);

        await EventsBus.PublishAsync(integrationEvent);
        await EventsBus.PublishAsync(integrationEvent);
        await ProcessStatsInboxAsync();

        var stats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        stats.Should().NotBeNull();
        stats!.GamesCompleted.Should().Be(1);
        stats.BestScore.Should().Be(300);
        stats.TotalScore.Should().Be(300);

        var dailyLeaderboard = await StatsModule.ExecuteQueryAsync(
            new GetLeaderboardQuery(
                LeaderboardOrderBy.BestScore,
                10,
                LeaderboardPeriod.Daily,
                integrationEvent.OccurredOn.Date));

        dailyLeaderboard.Should().ContainSingle();
        dailyLeaderboard[0].PlayerId.Should().Be(playerId);
        dailyLeaderboard[0].GamesCompleted.Should().Be(1);
        dailyLeaderboard[0].BestScore.Should().Be(300);
    }

    [Test]
    public async Task GameCompleted_OutboxProcessor_ProjectsStats()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-2", "Ada", "en-US"));
        await ProcessOutboxAsync();
        await ProcessStatsInboxAsync();

        var setup = await SetupChainedGameAsync(playerId);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var started = await Sender.Send(new GetGameByIdQuery(setup.GameId));
        var startIndex = Array.IndexOf(setup.OrderedLinkIds, started.StartLinkId);
        var targetIndex = Array.IndexOf(setup.OrderedLinkIds, started.TargetLinkId);
        var direction = startIndex < targetIndex ? 1 : -1;

        for (var index = startIndex + direction; index != targetIndex + direction; index += direction)
        {
            await ExecuteCommandAsync(new MakeStepCommand(setup.GameId, setup.OrderedLinkIds[index]));
        }

        await ProcessOutboxAsync();
        await ProcessStatsInboxAsync();

        var stats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        stats.Should().NotBeNull();
        stats!.GamesCompleted.Should().Be(1);
        stats.BestScore.Should().BeGreaterThan(0);
        stats.TotalScore.Should().Be(stats.BestScore!.Value);
        stats.LastGameCompletedOn.Should().NotBeNull();
    }

    [Test]
    public async Task GetLeaderboard_ReturnsOrderedCompletedPlayers()
    {
        var firstPlayerId = Guid.NewGuid();
        var secondPlayerId = Guid.NewGuid();

        await EventsBus.PublishAsync(new GameCompletedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-2),
            Guid.NewGuid(),
            firstPlayerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            200));

        await EventsBus.PublishAsync(new GameCompletedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-1),
            Guid.NewGuid(),
            secondPlayerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            500));

        await ProcessStatsInboxAsync();

        var leaderboard = await StatsModule.ExecuteQueryAsync(
            new GetLeaderboardQuery(LeaderboardOrderBy.BestScore, 10));

        leaderboard.Should().HaveCount(2);
        leaderboard[0].PlayerId.Should().Be(secondPlayerId);
        leaderboard[0].BestScore.Should().Be(500);
        leaderboard[1].PlayerId.Should().Be(firstPlayerId);
        leaderboard[1].BestScore.Should().Be(200);
    }

    [Test]
    public async Task GetLeaderboard_WithDailyAndWeeklyPeriods_ReturnsPeriodAggregates()
    {
        var firstPlayerId = Guid.NewGuid();
        var secondPlayerId = Guid.NewGuid();
        var firstDay = new DateTime(2026, 05, 12, 10, 0, 0, DateTimeKind.Utc);
        var secondDay = firstDay.AddDays(1);

        await EventsBus.PublishAsync(new GameCompletedIntegrationEvent(
            Guid.NewGuid(),
            firstDay,
            Guid.NewGuid(),
            firstPlayerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            200));

        await EventsBus.PublishAsync(new GameCompletedIntegrationEvent(
            Guid.NewGuid(),
            firstDay.AddHours(1),
            Guid.NewGuid(),
            secondPlayerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            500));

        await EventsBus.PublishAsync(new GameCompletedIntegrationEvent(
            Guid.NewGuid(),
            secondDay,
            Guid.NewGuid(),
            firstPlayerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            400));

        await ProcessStatsInboxAsync();

        var dailyLeaderboard = await StatsModule.ExecuteQueryAsync(
            new GetLeaderboardQuery(
                LeaderboardOrderBy.BestScore,
                10,
                LeaderboardPeriod.Daily,
                firstDay.Date));

        dailyLeaderboard.Should().HaveCount(2);
        dailyLeaderboard[0].PlayerId.Should().Be(secondPlayerId);
        dailyLeaderboard[0].BestScore.Should().Be(500);
        dailyLeaderboard[1].PlayerId.Should().Be(firstPlayerId);
        dailyLeaderboard[1].BestScore.Should().Be(200);

        var weeklyLeaderboard = await StatsModule.ExecuteQueryAsync(
            new GetLeaderboardQuery(
                LeaderboardOrderBy.TotalScore,
                10,
                LeaderboardPeriod.Weekly,
                GetWeekStartDate(firstDay)));

        weeklyLeaderboard.Should().HaveCount(2);
        weeklyLeaderboard[0].PlayerId.Should().Be(firstPlayerId);
        weeklyLeaderboard[0].GamesCompleted.Should().Be(2);
        weeklyLeaderboard[0].TotalScore.Should().Be(600);
        weeklyLeaderboard[1].PlayerId.Should().Be(secondPlayerId);
        weeklyLeaderboard[1].GamesCompleted.Should().Be(1);
        weeklyLeaderboard[1].TotalScore.Should().Be(500);
    }

    [Test]
    public async Task OutboxProcessor_WhenMessageFails_PersistsRetryMetadataAndContinues()
    {
        var badMessageId = Guid.NewGuid();
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-3", "Retry", "en-US"));

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            INSERT INTO "games"."OutboxMessages"
                ("Id", "OccurredOn", "Type", "Data")
            VALUES
                (@Id, @OccurredOn, @Type, @Data)
            """,
            new
            {
                Id = badMessageId,
                OccurredOn = DateTime.UtcNow.AddMinutes(-1),
                Type = "unknown.domain.notification",
                Data = "{}"
            });

        await ProcessOutboxAsync();
        await ProcessStatsInboxAsync();

        var failedMessage = await connection.QuerySingleAsync<OutboxFailureRow>("""
            SELECT "RetryCount", "NextRetryDate", "Error"
            FROM "games"."OutboxMessages"
            WHERE "Id" = @Id
            """,
            new { Id = badMessageId });

        failedMessage.RetryCount.Should().Be(1);
        failedMessage.NextRetryDate.Should().NotBeNull();
        failedMessage.Error.Should().Contain("unknown.domain.notification");

        var stats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        stats.Should().NotBeNull();
        stats!.DisplayName.Should().Be("Retry");
    }

    [Test]
    public async Task StatsInboxProcessor_WhenMessageFails_PersistsRetryMetadataAndContinues()
    {
        var badMessageId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            INSERT INTO "stats"."InboxMessages"
                ("Id", "OccurredOn", "Type", "Data")
            VALUES
                (@BadMessageId, @BadOccurredOn, @BadType, @BadData),
                (@GoodMessageId, @GoodOccurredOn, @GoodType, @GoodData)
            """,
            new
            {
                BadMessageId = badMessageId,
                BadOccurredOn = DateTime.UtcNow.AddMinutes(-1),
                BadType = "unknown.integration.event",
                BadData = "{}",
                GoodMessageId = Guid.NewGuid(),
                GoodOccurredOn = DateTime.UtcNow,
                GoodType = typeof(GameCompletedIntegrationEvent).FullName!,
                GoodData = $$"""
                    {
                      "Id": "{{Guid.NewGuid()}}",
                      "OccurredOn": "{{DateTime.UtcNow:o}}",
                      "GameId": "{{Guid.NewGuid()}}",
                      "PlayerId": "{{playerId}}",
                      "StartLinkId": "{{Guid.NewGuid()}}",
                      "TargetLinkId": "{{Guid.NewGuid()}}",
                      "Score": 700
                    }
                    """
            });

        await ProcessStatsInboxAsync();

        var failedMessage = await connection.QuerySingleAsync<InboxFailureRow>("""
            SELECT "RetryCount", "NextRetryDate", "Error"
            FROM "stats"."InboxMessages"
            WHERE "Id" = @Id
            """,
            new { Id = badMessageId });

        failedMessage.RetryCount.Should().Be(1);
        failedMessage.NextRetryDate.Should().NotBeNull();
        failedMessage.Error.Should().Contain("unknown.integration.event");

        var stats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        stats.Should().NotBeNull();
        stats!.GamesCompleted.Should().Be(1);
        stats.BestScore.Should().Be(700);
    }

    [Test]
    public async Task StatsInternalCommandProcessor_WhenCommandFails_PersistsRetryMetadataAndContinues()
    {
        var badCommandId = Guid.NewGuid();
        var goodCommandId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            INSERT INTO "stats"."InternalCommands"
                ("Id", "EnqueueDate", "DueDate", "Type", "Data")
            VALUES
                (@BadCommandId, @Now, @Now, @BadType, @BadData),
                (@GoodCommandId, @Now, @Now, @GoodType, @GoodData);

            INSERT INTO "stats"."InboxMessages"
                ("Id", "OccurredOn", "Type", "Data")
            VALUES
                (@InboxMessageId, @Now, @InboxType, @InboxData);
            """,
            new
            {
                BadCommandId = badCommandId,
                GoodCommandId = goodCommandId,
                Now = DateTime.UtcNow,
                BadType = "unknown.internal.command",
                BadData = "{}",
                GoodType = typeof(ProcessStatsInboxCommand).FullName!,
                GoodData = "{}",
                InboxMessageId = Guid.NewGuid(),
                InboxType = typeof(GameCompletedIntegrationEvent).FullName!,
                InboxData = $$"""
                    {
                      "Id": "{{Guid.NewGuid()}}",
                      "OccurredOn": "{{DateTime.UtcNow:o}}",
                      "GameId": "{{Guid.NewGuid()}}",
                      "PlayerId": "{{playerId}}",
                      "StartLinkId": "{{Guid.NewGuid()}}",
                      "TargetLinkId": "{{Guid.NewGuid()}}",
                      "Score": 900
                    }
                    """
            });

        await ProcessStatsInternalCommandsAsync();

        var failedCommand = await connection.QuerySingleAsync<InternalCommandFailureRow>("""
            SELECT "RetryCount", "NextRetryDate", "Error"
            FROM "stats"."InternalCommands"
            WHERE "Id" = @Id
            """,
            new { Id = badCommandId });

        failedCommand.RetryCount.Should().Be(1);
        failedCommand.NextRetryDate.Should().NotBeNull();
        failedCommand.Error.Should().Contain("unknown.internal.command");

        var processedDate = await connection.QuerySingleAsync<DateTime?>("""
            SELECT "ProcessedDate"
            FROM "stats"."InternalCommands"
            WHERE "Id" = @Id
            """,
            new { Id = goodCommandId });
        processedDate.Should().NotBeNull();

        var stats = await StatsModule.ExecuteQueryAsync(new GetPlayerStatsQuery(playerId));
        stats.Should().NotBeNull();
        stats!.GamesCompleted.Should().Be(1);
        stats.BestScore.Should().Be(900);
    }

    private async Task<GameSetup> SetupChainedGameAsync(Guid playerId)
    {
        var categoryId = await ExecuteCommandAsync(
            new CreateCategoryCommand("Animals", "Animal words"));
        var words = new[] { "cat", "mat", "bat", "bag", "bug", "rug" };
        var linkIds = new List<Guid>();

        foreach (var word in words)
        {
            linkIds.Add(await ExecuteCommandAsync(
                new CreateLinkCommand(categoryId, word, $"{word} description", isActive: true)));
        }

        for (var i = 0; i < linkIds.Count - 1; i++)
        {
            await ExecuteCommandAsync(new AddOutgoingLinkCommand(linkIds[i], linkIds[i + 1]));
            await ExecuteCommandAsync(new AddOutgoingLinkCommand(linkIds[i + 1], linkIds[i]));
        }

        var gameId = await ExecuteCommandAsync(
            new CreateGameCommand(playerId, categoryId, Difficulty.Easy));

        return new GameSetup(gameId, linkIds.ToArray());
    }

    private sealed record GameSetup(Guid GameId, Guid[] OrderedLinkIds);

    private sealed record OutboxFailureRow(int RetryCount, DateTime? NextRetryDate, string Error);

    private sealed record InboxFailureRow(int RetryCount, DateTime? NextRetryDate, string Error);

    private sealed record InternalCommandFailureRow(int RetryCount, DateTime? NextRetryDate, string Error);

    private static DateTime GetWeekStartDate(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }
}
