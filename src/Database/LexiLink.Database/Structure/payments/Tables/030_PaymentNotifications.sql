CREATE TABLE IF NOT EXISTS payments."PaymentNotifications" (
    "Id" uuid NOT NULL,
    "Platform" integer NOT NULL,
    "Environment" integer NOT NULL,
    "NotificationId" varchar(256) NOT NULL,
    "NotificationType" varchar(128) NOT NULL,
    "PayloadJson" text NOT NULL,
    "ReceivedAt" timestamp without time zone NOT NULL,
    "ProcessedAt" timestamp without time zone NULL,
    "Status" integer NOT NULL,
    "FailureReason" varchar(1000) NULL,
    CONSTRAINT "PK_Payments_PaymentNotifications" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Payments_PaymentNotifications_Platform_NotificationId"
    ON payments."PaymentNotifications" ("Platform", "NotificationId");
