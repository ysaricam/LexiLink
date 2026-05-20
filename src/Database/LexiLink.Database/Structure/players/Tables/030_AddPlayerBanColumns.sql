-- Player ban columns. IsBanned defaults to false for the existing row
-- population. IF NOT EXISTS keeps the script idempotent if re-run.
ALTER TABLE "players"."Players"
    ADD COLUMN IF NOT EXISTS "IsBanned"     boolean                     NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS "BannedReason" text                        NULL,
    ADD COLUMN IF NOT EXISTS "BannedAt"     timestamp without time zone NULL;

CREATE INDEX IF NOT EXISTS "IX_Players_IsBanned"
    ON "players"."Players" ("IsBanned")
    WHERE "IsBanned" = TRUE;
