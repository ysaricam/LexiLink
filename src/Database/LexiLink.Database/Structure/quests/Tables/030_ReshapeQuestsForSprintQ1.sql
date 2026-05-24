-- Sprint Q1 destructive reshape. Drops the closed-enum-based shape
-- (QuestType / Cadence / Goal / Progress columns) and rebuilds the
-- data-driven shape that matches Domain v2: QuestDefinition.Trigger,
-- Threshold, Reward, ProgressBaseline, PrerequisiteQuestDefinitionId
-- and PlayerQuests.QuestDefinitionId / ProgressBaselineSnapshot. Drops
-- all existing PlayerQuests + QuestDefinitions rows because the old
-- shape's enum identity cannot be safely migrated to the new GUID FK
-- identity; no production data exists yet.
--
-- Idempotent across fresh and existing databases:
--   * Fresh DB: 010 / 020 / 021 / 110 already created the new shape +
--     daily seed. This script drops and re-creates them identically,
--     then re-inserts the seed via ON CONFLICT DO NOTHING.
--   * Existing DB: 010 / 020 / 021 / 110 are journaled (old shape).
--     This script drops the old tables and creates the new shape.

DROP VIEW  IF EXISTS "quests"."v_PlayerQuests";
DROP TABLE IF EXISTS "quests"."PlayerQuests"     CASCADE;
DROP TABLE IF EXISTS "quests"."QuestDefinitions" CASCADE;

CREATE TABLE "quests"."QuestDefinitions"
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

CREATE INDEX "IX_QuestDefinitions_IsActive"
    ON "quests"."QuestDefinitions" ("IsActive");

CREATE TABLE "quests"."PlayerQuests"
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
    CONSTRAINT "FK_PlayerQuests_QuestDefinition"
        FOREIGN KEY ("QuestDefinitionId")
        REFERENCES "quests"."QuestDefinitions" ("Id"),
    CONSTRAINT "UX_PlayerQuests_PlayerId_QuestDefinitionId"
        UNIQUE ("PlayerId", "QuestDefinitionId")
);

CREATE INDEX "IX_PlayerQuests_PlayerId_State"
    ON "quests"."PlayerQuests" ("PlayerId", "State");

CREATE INDEX "IX_PlayerQuests_QuestDefinitionId"
    ON "quests"."PlayerQuests" ("QuestDefinitionId");

CREATE OR REPLACE VIEW "quests"."v_PlayerQuests" AS
SELECT
    "Id",
    "PlayerId",
    "QuestDefinitionId",
    "ProgressBaselineSnapshot",
    "State",
    "IssuedAt",
    "ClaimedAt",
    "ExpiresAt"
FROM "quests"."PlayerQuests";

INSERT INTO "quests"."QuestDefinitions"
    ("Id",                                   "Name",          "Description",            "Trigger",            "Threshold", "Reward", "PrerequisiteQuestDefinitionId", "ProgressBaseline", "IsActive")
VALUES
    ('11111111-0000-0000-0000-000000000010', 'Günlük 3 Oyun', 'Bugün 3 oyun tamamla.',  'GameCompletedDaily', 3,           5,        NULL,                            'FromSnapshot',     TRUE)
ON CONFLICT ("Id") DO NOTHING;
