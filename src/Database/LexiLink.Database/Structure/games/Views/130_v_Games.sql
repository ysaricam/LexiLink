CREATE OR REPLACE VIEW "games"."v_Games" AS
SELECT
    "Id"                                        AS "Id",
    "PlayerId"                                  AS "PlayerId",
    "CategoryId"                                AS "CategoryId",
    "Difficulty"                                AS "Difficulty",
    "StartLinkId"                               AS "StartLinkId",
    "TargetLinkId"                              AS "TargetLinkId",
    "CurrentLinkId"                             AS "CurrentLinkId",
    "State"                                     AS "State",
    "Score"                                     AS "Score",
    "MaxSteps"                                  AS "MaxSteps",
    "StepsTaken"                                AS "StepsTaken",
    ("HintsRemaining"  + "HintsUsed")           AS "HintsTotal",
    "HintsUsed"                                 AS "HintsUsed",
    ("UndosRemaining"  + "UndosUsed")           AS "UndosTotal",
    "UndosUsed"                                 AS "UndosUsed",
    ("ResetsRemaining" + "ResetsUsed")          AS "ResetsTotal",
    "ResetsUsed"                                AS "ResetsUsed"
FROM "games"."Games";
