-- UR5 expands QuestDefinition rewards from Energy + Hint to
-- Energy + Hint + Undo + Reset. Existing quests receive zero for
-- the new reward types, preserving current behavior until an admin
-- opts into the new rewards.
ALTER TABLE "quests"."QuestDefinitions"
    ADD COLUMN IF NOT EXISTS "UndoReward" integer NOT NULL DEFAULT 0;

ALTER TABLE "quests"."QuestDefinitions"
    ADD COLUMN IF NOT EXISTS "ResetReward" integer NOT NULL DEFAULT 0;
