CREATE TABLE IF NOT EXISTS "players"."OutboxMessages"
(
    "Id"            uuid                        NOT NULL,
    "OccurredOn"    timestamp without time zone NOT NULL,
    "Type"          text                        NOT NULL,
    "Data"          text                        NOT NULL,
    "ProcessedDate" timestamp without time zone NULL,
    CONSTRAINT "PK_Players_OutboxMessages" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Players_OutboxMessages_ProcessedDate_OccurredOn"
    ON "players"."OutboxMessages" ("ProcessedDate", "OccurredOn");
