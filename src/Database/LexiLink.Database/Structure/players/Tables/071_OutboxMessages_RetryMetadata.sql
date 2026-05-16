ALTER TABLE "players"."OutboxMessages"
    ADD COLUMN IF NOT EXISTS "RetryCount" integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS "NextRetryDate" timestamp without time zone NULL,
    ADD COLUMN IF NOT EXISTS "Error" text NULL;

CREATE INDEX IF NOT EXISTS "IX_Players_OutboxMessages_Retry"
    ON "players"."OutboxMessages" ("ProcessedDate", "RetryCount", "NextRetryDate", "OccurredOn");
