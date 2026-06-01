ALTER TABLE "games"."Categories"
    ADD COLUMN IF NOT EXISTS "Language" text NOT NULL DEFAULT 'tr-TR';

CREATE INDEX IF NOT EXISTS "IX_Categories_Language_Name"
    ON "games"."Categories" ("Language", "Name");
