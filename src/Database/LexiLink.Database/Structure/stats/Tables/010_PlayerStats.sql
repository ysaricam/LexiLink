CREATE TABLE IF NOT EXISTS "stats"."PlayerStats"
(
    "PlayerId" uuid NOT NULL,
    "DisplayName" text NULL,
    "Discriminator" integer NULL,
    "IsGuest" boolean NOT NULL,
    "AuthProvidersLinked" integer NOT NULL,
    "GamesCompleted" integer NOT NULL,
    "BestScore" integer NULL,
    "TotalScore" integer NOT NULL,
    "LastGameCompletedOn" timestamp without time zone NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "UpdatedAt" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_stats_PlayerStats" PRIMARY KEY ("PlayerId")
);
