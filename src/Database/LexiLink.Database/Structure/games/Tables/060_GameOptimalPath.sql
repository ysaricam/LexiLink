CREATE TABLE IF NOT EXISTS "games"."GameOptimalPath"
(
    "GameId"   uuid    NOT NULL,
    "Position" integer NOT NULL,
    "LinkId"   uuid    NOT NULL,
    CONSTRAINT "PK_GameOptimalPath" PRIMARY KEY ("GameId", "Position"),
    CONSTRAINT "FK_GameOptimalPath_Games" FOREIGN KEY ("GameId")
        REFERENCES "games"."Games" ("Id")
        ON DELETE CASCADE
);
