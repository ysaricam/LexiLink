ALTER TABLE "stats"."PlayerStats"
    ADD COLUMN IF NOT EXISTS "AvatarUrl" text NULL;

ALTER TABLE "stats"."PlayerStats"
    ADD COLUMN IF NOT EXISTS "Locale" text NULL;
