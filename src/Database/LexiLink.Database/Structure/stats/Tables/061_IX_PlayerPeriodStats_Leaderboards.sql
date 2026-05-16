CREATE INDEX IF NOT EXISTS "IX_stats_PlayerPeriodStats_Leaderboard_BestScore"
    ON "stats"."PlayerPeriodStats" (
        "PeriodType",
        "PeriodStartDate",
        "BestScore" DESC NULLS LAST,
        "LastGameCompletedOn" ASC NULLS LAST,
        "PlayerId" ASC)
    WHERE "GamesCompleted" > 0;

CREATE INDEX IF NOT EXISTS "IX_stats_PlayerPeriodStats_Leaderboard_TotalScore"
    ON "stats"."PlayerPeriodStats" (
        "PeriodType",
        "PeriodStartDate",
        "TotalScore" DESC,
        "LastGameCompletedOn" ASC NULLS LAST,
        "PlayerId" ASC)
    WHERE "GamesCompleted" > 0;

CREATE INDEX IF NOT EXISTS "IX_stats_PlayerPeriodStats_Leaderboard_GamesCompleted"
    ON "stats"."PlayerPeriodStats" (
        "PeriodType",
        "PeriodStartDate",
        "GamesCompleted" DESC,
        "LastGameCompletedOn" ASC NULLS LAST,
        "PlayerId" ASC)
    WHERE "GamesCompleted" > 0;
