CREATE TABLE IF NOT EXISTS "games"."GameHistory"
(
    "GameId"     uuid    NOT NULL,
    "StepNumber" integer NOT NULL,
    "LinkId"     uuid    NOT NULL,
    CONSTRAINT "PK_GameHistory" PRIMARY KEY ("GameId", "StepNumber"),
    CONSTRAINT "FK_GameHistory_Games" FOREIGN KEY ("GameId")
        REFERENCES "games"."Games" ("Id")
        ON DELETE CASCADE
);
