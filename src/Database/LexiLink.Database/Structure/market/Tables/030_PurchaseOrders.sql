CREATE TABLE IF NOT EXISTS "market"."PurchaseOrders"
(
    "Id"             uuid                        NOT NULL,
    "PlayerId"       uuid                        NOT NULL,
    "ShopItemId"     uuid                        NOT NULL,
    "ItemType"       integer                     NOT NULL,
    "Quantity"       integer                     NOT NULL,
    "DiamondsPaid"   integer                     NOT NULL,
    "PurchasedAt"    timestamp without time zone NOT NULL,
    "IdempotencyKey" character varying(128)      NOT NULL,
    CONSTRAINT "PK_Market_PurchaseOrders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Market_PurchaseOrders_ShopItems_ShopItemId"
        FOREIGN KEY ("ShopItemId") REFERENCES "market"."ShopItems" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_Market_PurchaseOrders_ShopItemId"
    ON "market"."PurchaseOrders" ("ShopItemId");

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Market_PurchaseOrders_PlayerId_IdempotencyKey"
    ON "market"."PurchaseOrders" ("PlayerId", "IdempotencyKey");
