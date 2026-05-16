CREATE TABLE IF NOT EXISTS "stats"."InboxMessages"
(
    "Id" uuid NOT NULL,
    "OccurredOn" timestamp without time zone NOT NULL,
    "Type" text NOT NULL,
    "Data" text NOT NULL,
    "ProcessedDate" timestamp without time zone NULL,
    "RetryCount" integer NOT NULL DEFAULT 0,
    "NextRetryDate" timestamp without time zone NULL,
    "Error" text NULL,
    CONSTRAINT "PK_stats_InboxMessages" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_stats_InboxMessages_Processing"
    ON "stats"."InboxMessages" ("ProcessedDate", "RetryCount", "NextRetryDate", "OccurredOn");
