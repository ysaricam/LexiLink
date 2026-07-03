ALTER TABLE "quests"."PlayerQuests"
    ADD COLUMN IF NOT EXISTS "RemainingEnergyReward" integer NOT NULL DEFAULT 0;

ALTER TABLE "quests"."PlayerQuests"
    ADD COLUMN IF NOT EXISTS "NonEnergyRewardsClaimed" boolean NOT NULL DEFAULT FALSE;

UPDATE "quests"."PlayerQuests" AS pq
SET "RemainingEnergyReward" = qd."EnergyReward"
FROM "quests"."QuestDefinitions" AS qd
WHERE pq."QuestDefinitionId" = qd."Id"
  AND pq."State" <> 'Claimed'
  AND pq."RemainingEnergyReward" = 0;

UPDATE "quests"."PlayerQuests"
SET "RemainingEnergyReward" = 0,
    "NonEnergyRewardsClaimed" = TRUE
WHERE "State" = 'Claimed';
