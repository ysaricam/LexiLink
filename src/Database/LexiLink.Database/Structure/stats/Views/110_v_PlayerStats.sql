CREATE OR REPLACE VIEW "stats"."v_PlayerStats" AS
SELECT
    "PlayerId",
    "DisplayName",
    "Discriminator",
    CASE
        WHEN "DisplayName" IS NULL OR "Discriminator" IS NULL THEN NULL
        ELSE "DisplayName" || '#' || lpad("Discriminator"::text, 4, '0')
    END AS "Handle",
    "IsGuest",
    "AuthProvidersLinked",
    "GamesCompleted",
    "BestScore",
    "TotalScore",
    "LastGameCompletedOn",
    "CreatedAt",
    "UpdatedAt"
FROM "stats"."PlayerStats";
