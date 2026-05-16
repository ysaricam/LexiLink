CREATE OR REPLACE VIEW "quests"."v_PlayerQuests" AS
SELECT
    "Id"           AS "Id",
    "PlayerId"     AS "PlayerId",
    "QuestType"    AS "QuestType",
    "Progress"     AS "Progress",
    "Goal"         AS "Goal",
    "RewardAmount" AS "RewardAmount",
    "State"        AS "State",
    "IssuedAt"     AS "IssuedAt",
    "CompletedAt"  AS "CompletedAt",
    "ClaimedAt"    AS "ClaimedAt",
    "ExpiresAt"    AS "ExpiresAt"
FROM "quests"."PlayerQuests";
