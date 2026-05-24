CREATE TABLE IF NOT EXISTS "quests"."PlayerQuests"
(
    "Id"                       uuid                        NOT NULL,
    "PlayerId"                 uuid                        NOT NULL,
    "QuestDefinitionId"        uuid                        NOT NULL,
    "ProgressBaselineSnapshot" integer                     NOT NULL,
    "State"                    varchar(32)                 NOT NULL,
    "IssuedAt"                 timestamp without time zone NOT NULL,
    "ClaimedAt"                timestamp without time zone NULL,
    "ExpiresAt"                timestamp without time zone NULL,
    CONSTRAINT "PK_PlayerQuests" PRIMARY KEY ("Id"),
    CONSTRAINT "UX_PlayerQuests_PlayerId_QuestDefinitionId"
        UNIQUE ("PlayerId", "QuestDefinitionId")
);

CREATE INDEX IF NOT EXISTS "IX_PlayerQuests_PlayerId_State"
    ON "quests"."PlayerQuests" ("PlayerId", "State");

CREATE INDEX IF NOT EXISTS "IX_PlayerQuests_QuestDefinitionId"
    ON "quests"."PlayerQuests" ("QuestDefinitionId");
