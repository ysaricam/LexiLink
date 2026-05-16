CREATE TABLE IF NOT EXISTS "stats"."PlayerPeriodStats"
(
    "PeriodType" text NOT NULL,
    "PeriodStartDate" timestamp without time zone NOT NULL,
    "PlayerId" uuid NOT NULL,
    "GamesCompleted" integer NOT NULL,
    "BestScore" integer NULL,
    "TotalScore" integer NOT NULL,
    "LastGameCompletedOn" timestamp without time zone NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "UpdatedAt" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_stats_PlayerPeriodStats" PRIMARY KEY ("PeriodType", "PeriodStartDate", "PlayerId")
);

CREATE INDEX IF NOT EXISTS "IX_stats_PlayerPeriodStats_Leaderboard"
    ON "stats"."PlayerPeriodStats" ("PeriodType", "PeriodStartDate", "GamesCompleted", "BestScore", "TotalScore");
