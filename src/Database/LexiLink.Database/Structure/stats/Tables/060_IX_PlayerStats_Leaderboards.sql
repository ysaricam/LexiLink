CREATE INDEX IF NOT EXISTS "IX_stats_PlayerStats_Leaderboard_BestScore"
    ON "stats"."PlayerStats" ("BestScore" DESC NULLS LAST, "LastGameCompletedOn" ASC NULLS LAST, "PlayerId" ASC)
    WHERE "GamesCompleted" > 0;

CREATE INDEX IF NOT EXISTS "IX_stats_PlayerStats_Leaderboard_TotalScore"
    ON "stats"."PlayerStats" ("TotalScore" DESC, "LastGameCompletedOn" ASC NULLS LAST, "PlayerId" ASC)
    WHERE "GamesCompleted" > 0;

CREATE INDEX IF NOT EXISTS "IX_stats_PlayerStats_Leaderboard_GamesCompleted"
    ON "stats"."PlayerStats" ("GamesCompleted" DESC, "LastGameCompletedOn" ASC NULLS LAST, "PlayerId" ASC)
    WHERE "GamesCompleted" > 0;
