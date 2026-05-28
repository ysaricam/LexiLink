CREATE TABLE IF NOT EXISTS payments."InboxMessages" (
    "Id" uuid NOT NULL,
    "OccurredOn" timestamp without time zone NOT NULL,
    "Type" text NOT NULL,
    "Data" text NOT NULL,
    "ProcessedDate" timestamp without time zone NULL,
    "Error" text NULL,
    CONSTRAINT "PK_Payments_InboxMessages" PRIMARY KEY ("Id")
);
