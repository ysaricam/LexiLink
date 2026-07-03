CREATE OR REPLACE VIEW "quests"."v_PlayerQuests" AS
SELECT
    "Id",
    "PlayerId",
    "QuestDefinitionId",
    "ProgressBaselineSnapshot",
    "RemainingEnergyReward",
    "NonEnergyRewardsClaimed",
    "State",
    "IssuedAt",
    "ClaimedAt",
    "ExpiresAt"
FROM "quests"."PlayerQuests";
