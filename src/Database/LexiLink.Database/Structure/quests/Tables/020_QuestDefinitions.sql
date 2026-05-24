CREATE TABLE IF NOT EXISTS "quests"."QuestDefinitions"
(
    "Id"                            uuid         NOT NULL,
    "Name"                          varchar(64)  NOT NULL,
    "Description"                   varchar(256) NOT NULL,
    "Trigger"                       varchar(32)  NOT NULL,
    "Threshold"                     integer      NOT NULL,
    "Reward"                        integer      NOT NULL,
    "PrerequisiteQuestDefinitionId" uuid         NULL,
    "ProgressBaseline"              varchar(32)  NOT NULL,
    "IsActive"                      boolean      NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_QuestDefinitions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_QuestDefinitions_Prerequisite"
        FOREIGN KEY ("PrerequisiteQuestDefinitionId")
        REFERENCES "quests"."QuestDefinitions" ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_QuestDefinitions_IsActive"
    ON "quests"."QuestDefinitions" ("IsActive");
