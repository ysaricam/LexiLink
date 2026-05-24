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
