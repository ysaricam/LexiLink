CREATE TABLE IF NOT EXISTS "quests"."QuestDefinitions"
(
    "Id"                    uuid        NOT NULL,
    "QuestType"             varchar(64) NOT NULL,
    "Cadence"               varchar(32) NOT NULL,
    "Goal"                  integer     NOT NULL,
    "RewardAmount"          integer     NOT NULL,
    "PrerequisiteQuestType" varchar(64) NULL,
    "IsActive"              boolean     NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_QuestDefinitions"            PRIMARY KEY ("Id"),
    CONSTRAINT "UX_QuestDefinitions_QuestType"  UNIQUE      ("QuestType")
);

CREATE INDEX IF NOT EXISTS "IX_QuestDefinitions_IsActive_QuestType"
    ON "quests"."QuestDefinitions" ("IsActive", "QuestType");
