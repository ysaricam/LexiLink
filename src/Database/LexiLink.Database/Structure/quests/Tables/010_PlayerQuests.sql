CREATE TABLE IF NOT EXISTS "quests"."PlayerQuests"
(
    "Id"           uuid                        NOT NULL,
    "PlayerId"     uuid                        NOT NULL,
    "QuestType"    varchar(64)                 NOT NULL,
    "Progress"     integer                     NOT NULL,
    "Goal"         integer                     NOT NULL,
    "RewardAmount" integer                     NOT NULL,
    "State"        varchar(32)                 NOT NULL,
    "IssuedAt"     timestamp without time zone NOT NULL,
    "CompletedAt"  timestamp without time zone NULL,
    "ClaimedAt"    timestamp without time zone NULL,
    "ExpiresAt"    timestamp without time zone NULL,
    CONSTRAINT "PK_PlayerQuests" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_PlayerQuests_PlayerId_State"
    ON "quests"."PlayerQuests" ("PlayerId", "State");

CREATE INDEX IF NOT EXISTS "IX_PlayerQuests_PlayerId_QuestType"
    ON "quests"."PlayerQuests" ("PlayerId", "QuestType");
