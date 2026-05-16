using LexiLink.API.Configuration.Health;

namespace LexiLink.API.Tests.Operational;

[TestFixture]
public sealed class DatabaseMigrationScriptMatcherTests
{
    [Test]
    public void IsApplied_WhenJournalStoresRelativePath_MatchesExpectedScript()
    {
        DatabaseMigrationScriptMatcher
            .IsApplied("games/Tables/090_IX_Links_CategoryActive.sql", "games/Tables/090_IX_Links_CategoryActive.sql")
            .Should().BeTrue();
    }

    [Test]
    public void IsApplied_WhenJournalStoresFullPath_MatchesExpectedScript()
    {
        DatabaseMigrationScriptMatcher
            .IsApplied(
                "/app/Database/Structure/stats/Tables/061_IX_PlayerPeriodStats_Leaderboards.sql",
                "stats/Tables/061_IX_PlayerPeriodStats_Leaderboards.sql")
            .Should().BeTrue();
    }

    [Test]
    public void IsApplied_WhenJournalStoresDbUpDotName_MatchesExpectedScript()
    {
        DatabaseMigrationScriptMatcher
            .IsApplied(
                "stats.Tables.060_IX_PlayerStats_Leaderboards.sql",
                "stats/Tables/060_IX_PlayerStats_Leaderboards.sql")
            .Should().BeTrue();
    }

    [Test]
    public void IsApplied_WhenScriptIsDifferent_ReturnsFalse()
    {
        DatabaseMigrationScriptMatcher
            .IsApplied("games/Tables/090_IX_Links_CategoryActive.sql", "stats/Tables/060_IX_PlayerStats_Leaderboards.sql")
            .Should().BeFalse();
    }
}
