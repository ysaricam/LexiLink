CREATE OR REPLACE VIEW "games"."v_Categories" AS
SELECT
    "Id"          AS "Id",
    "Name"        AS "Name",
    "Description" AS "Description",
    "Language"    AS "Language"
FROM "games"."Categories";
