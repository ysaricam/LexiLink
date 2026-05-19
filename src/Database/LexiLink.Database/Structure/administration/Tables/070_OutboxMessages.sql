CREATE TABLE IF NOT EXISTS "administration"."OutboxMessages"
(
    "Id"            uuid                        NOT NULL,
    "OccurredOn"    timestamp without time zone NOT NULL,
    "Type"          text                        NOT NULL,
    "Data"          text                        NOT NULL,
    "ProcessedDate" timestamp without time zone NULL,
    "RetryCount"    integer                     NOT NULL DEFAULT 0,
    "NextRetryDate" timestamp without time zone NULL,
    "Error"         text                        NULL,
    CONSTRAINT "PK_Administration_OutboxMessages" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Administration_OutboxMessages_ProcessedDate_OccurredOn"
    ON "administration"."OutboxMessages" ("ProcessedDate", "OccurredOn");

CREATE INDEX IF NOT EXISTS "IX_Administration_OutboxMessages_Retry"
    ON "administration"."OutboxMessages" ("ProcessedDate", "RetryCount", "NextRetryDate", "OccurredOn");
