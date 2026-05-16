ALTER TABLE "stats"."InboxMessages"
    ADD COLUMN IF NOT EXISTS "Data" text NOT NULL DEFAULT '{}',
    ALTER COLUMN "ProcessedDate" DROP NOT NULL,
    ADD COLUMN IF NOT EXISTS "RetryCount" integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS "NextRetryDate" timestamp without time zone NULL,
    ADD COLUMN IF NOT EXISTS "Error" text NULL;

CREATE INDEX IF NOT EXISTS "IX_stats_InboxMessages_Processing"
    ON "stats"."InboxMessages" ("ProcessedDate", "RetryCount", "NextRetryDate", "OccurredOn");
