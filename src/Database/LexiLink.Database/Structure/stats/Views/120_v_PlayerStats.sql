DROP VIEW IF EXISTS "stats"."v_PlayerStats";

CREATE VIEW "stats"."v_PlayerStats" AS
SELECT
    "PlayerId",
    "DisplayName",
    "Discriminator",
    CASE
        WHEN "DisplayName" IS NULL OR "Discriminator" IS NULL THEN NULL
        ELSE "DisplayName" || '#' || lpad("Discriminator"::text, 4, '0')
    END AS "Handle",
    "AvatarUrl",
    "Locale",
    "IsGuest",
    "AuthProvidersLinked",
    "GamesCompleted",
    "BestScore",
    "TotalScore",
    "LastGameCompletedOn",
    "CreatedAt",
    "UpdatedAt"
FROM "stats"."PlayerStats";
