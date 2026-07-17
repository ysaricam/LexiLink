CREATE OR REPLACE VIEW "players"."v_Players" AS
SELECT
    "Id"                                                                AS "Id",
    "DisplayName"                                                       AS "DisplayName",
    "DiscriminatorValue"                                                AS "DiscriminatorValue",
    "DisplayName" || '#' || lpad("DiscriminatorValue"::text, 4, '0')    AS "Handle",
    "AvatarUrl"                                                         AS "AvatarUrl",
    "Locale"                                                            AS "Locale",
    "IsGuest"                                                           AS "IsGuest"
FROM "players"."Players"
WHERE "IsDeleted" = FALSE;
