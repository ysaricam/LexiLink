ALTER TABLE "players"."Players"
    ADD COLUMN IF NOT EXISTS "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS "DeletedAt" timestamp without time zone NULL;

CREATE INDEX IF NOT EXISTS "IX_Players_IsDeleted"
    ON "players"."Players" ("IsDeleted");
