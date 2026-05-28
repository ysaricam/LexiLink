CREATE TABLE IF NOT EXISTS payments."IapPurchases" (
    "Id" uuid NOT NULL,
    "PlayerId" uuid NOT NULL,
    "Platform" integer NOT NULL,
    "Environment" integer NOT NULL,
    "StoreProductId" varchar(128) NOT NULL,
    "StoreTransactionId" varchar(256) NULL,
    "PurchaseToken" varchar(2048) NULL,
    "OrderId" varchar(256) NULL,
    "ClientRequestId" varchar(128) NULL,
    "DiamondAmount" integer NOT NULL,
    "Status" integer NOT NULL,
    "PostProcessingAction" integer NOT NULL DEFAULT 0,
    "PostProcessingStatus" integer NOT NULL DEFAULT 0,
    "ReceivedAt" timestamp without time zone NOT NULL,
    "VerifiedAt" timestamp without time zone NULL,
    "GrantedAt" timestamp without time zone NULL,
    "FailureReason" varchar(1000) NULL,
    "PostProcessedAt" timestamp without time zone NULL,
    "PostProcessingFailureReason" varchar(1000) NULL,
    CONSTRAINT "PK_Payments_IapPurchases" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Payments_IapPurchases_PlayerId"
    ON payments."IapPurchases" ("PlayerId");

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Payments_IapPurchases_Platform_StoreTransactionId"
    ON payments."IapPurchases" ("Platform", "StoreTransactionId")
    WHERE "StoreTransactionId" IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Payments_IapPurchases_Platform_PurchaseToken"
    ON payments."IapPurchases" ("Platform", "PurchaseToken")
    WHERE "PurchaseToken" IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Payments_IapPurchases_Player_ClientRequestId"
    ON payments."IapPurchases" ("PlayerId", "ClientRequestId")
    WHERE "ClientRequestId" IS NOT NULL;
