CREATE TABLE IF NOT EXISTS payments."PaymentProducts" (
    "Id" uuid NOT NULL,
    "StoreProductId" varchar(128) NOT NULL,
    "DiamondAmount" integer NOT NULL,
    "IsAppleAvailable" boolean NOT NULL,
    "IsGoogleAvailable" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_Payments_PaymentProducts" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Payments_PaymentProducts_StoreProductId"
    ON payments."PaymentProducts" ("StoreProductId");
