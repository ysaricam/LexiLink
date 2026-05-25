-- Sprint H reshape: quest rewards expand from a single Reward int
-- to (EnergyReward, HintReward). The existing Reward column becomes
-- EnergyReward (semantically equivalent — all current rewards were
-- energy bonuses); HintReward is added with NOT NULL DEFAULT 0 so
-- existing rows convert losslessly.
--
-- Idempotent across fresh and existing databases:
--   * Fresh DB: 020 already created the new shape (EnergyReward +
--     HintReward columns). The ALTER blocks below short-circuit
--     when the old column is missing.
--   * Existing DB: 020 was journaled with the old Reward column;
--     this script renames it and adds the HintReward column.

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'quests'
          AND table_name = 'QuestDefinitions'
          AND column_name = 'Reward'
    ) THEN
        ALTER TABLE "quests"."QuestDefinitions" RENAME COLUMN "Reward" TO "EnergyReward";
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'quests'
          AND table_name = 'QuestDefinitions'
          AND column_name = 'HintReward'
    ) THEN
        ALTER TABLE "quests"."QuestDefinitions"
            ADD COLUMN "HintReward" integer NOT NULL DEFAULT 0;
    END IF;
END$$;
