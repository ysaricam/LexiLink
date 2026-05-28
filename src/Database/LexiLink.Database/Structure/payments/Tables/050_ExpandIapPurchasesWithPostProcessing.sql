ALTER TABLE payments."IapPurchases"
    ADD COLUMN IF NOT EXISTS "PostProcessingAction" integer NOT NULL DEFAULT 0;

ALTER TABLE payments."IapPurchases"
    ADD COLUMN IF NOT EXISTS "PostProcessingStatus" integer NOT NULL DEFAULT 0;

ALTER TABLE payments."IapPurchases"
    ADD COLUMN IF NOT EXISTS "PostProcessedAt" timestamp without time zone NULL;

ALTER TABLE payments."IapPurchases"
    ADD COLUMN IF NOT EXISTS "PostProcessingFailureReason" varchar(1000) NULL;
