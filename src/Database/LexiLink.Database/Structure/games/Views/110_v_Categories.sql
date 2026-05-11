CREATE OR REPLACE VIEW "games"."v_Categories" AS
SELECT
    "Id"          AS "Id",
    "Name"        AS "Name",
    "Description" AS "Description"
FROM "games"."Categories";
