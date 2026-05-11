CREATE TABLE IF NOT EXISTS "games"."OutboxMessages"
(
    "Id"            uuid                        NOT NULL,
    "OccurredOn"    timestamp without time zone NOT NULL,
    "Type"          text                        NOT NULL,
    "Data"          text                        NOT NULL,
    "ProcessedDate" timestamp without time zone NULL,
    CONSTRAINT "PK_OutboxMessages" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_OutboxMessages_ProcessedDate_OccurredOn"
    ON "games"."OutboxMessages" ("ProcessedDate", "OccurredOn");
