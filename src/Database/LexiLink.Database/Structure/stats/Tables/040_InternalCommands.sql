CREATE TABLE IF NOT EXISTS "stats"."InternalCommands"
(
    "Id" uuid NOT NULL,
    "EnqueueDate" timestamp without time zone NOT NULL,
    "DueDate" timestamp without time zone NOT NULL,
    "Type" text NOT NULL,
    "Data" text NOT NULL,
    "ProcessedDate" timestamp without time zone NULL,
    "RetryCount" integer NOT NULL DEFAULT 0,
    "NextRetryDate" timestamp without time zone NULL,
    "Error" text NULL,
    CONSTRAINT "PK_stats_InternalCommands" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_stats_InternalCommands_Processing"
    ON "stats"."InternalCommands" ("ProcessedDate", "DueDate", "RetryCount", "NextRetryDate");
