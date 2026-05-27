-- D3 expands QuestDefinition rewards from Energy + Hint + Undo + Reset
-- to Energy + Hint + Undo + Reset + Diamond. Existing quests receive
-- zero Diamond reward, preserving current behavior until an admin opts in.
ALTER TABLE "quests"."QuestDefinitions"
    ADD COLUMN IF NOT EXISTS "DiamondReward" integer NOT NULL DEFAULT 0;
