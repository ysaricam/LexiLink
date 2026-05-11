CREATE OR REPLACE VIEW "games"."v_GameHistory" AS
SELECT
    "GameId"     AS "GameId",
    "StepNumber" AS "StepNumber",
    "LinkId"     AS "LinkId"
FROM "games"."GameHistory";
