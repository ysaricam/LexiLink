CREATE OR REPLACE VIEW "games"."v_Links" AS
SELECT
    "Id"          AS "Id",
    "CategoryId"  AS "CategoryId",
    "Value"       AS "Value",
    "Description" AS "Description",
    "IsActive"    AS "IsActive"
FROM "games"."Links";
