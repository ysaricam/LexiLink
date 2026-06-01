CREATE TABLE IF NOT EXISTS "ads"."OutboxMessages"
(
    "Id"            uuid                        NOT NULL,
    "OccurredOn"    timestamp without time zone NOT NULL,
    "Type"          text                        NOT NULL,
    "Data"          text                        NOT NULL,
    "ProcessedDate" timestamp without time zone NULL,
    "RetryCount"    integer                     NOT NULL DEFAULT 0,
    "NextRetryDate" timestamp without time zone NULL,
    "Error"         text                        NULL,
    CONSTRAINT "PK_Ads_OutboxMessages" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Ads_OutboxMessages_ProcessedDate_OccurredOn"
    ON "ads"."OutboxMessages" ("ProcessedDate", "OccurredOn");

CREATE INDEX IF NOT EXISTS "IX_Ads_OutboxMessages_Retry"
    ON "ads"."OutboxMessages" ("ProcessedDate", "RetryCount", "NextRetryDate", "OccurredOn");
