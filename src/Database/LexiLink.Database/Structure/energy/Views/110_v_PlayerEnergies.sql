CREATE OR REPLACE VIEW "energy"."v_PlayerEnergies" AS
SELECT
    "PlayerId"                AS "PlayerId",
    "CurrentAmount"           AS "CurrentAmount",
    "MaximumAmount"           AS "MaximumAmount",
    "RechargeIntervalSeconds" AS "RechargeIntervalSeconds",
    "LastRefilledOn"          AS "LastRefilledOn"
FROM "energy"."PlayerEnergies";
