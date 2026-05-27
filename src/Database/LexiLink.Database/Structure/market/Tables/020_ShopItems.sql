CREATE TABLE IF NOT EXISTS "market"."ShopItems"
(
    "Id"                   uuid                        NOT NULL,
    "CategoryId"           uuid                        NOT NULL,
    "ItemType"             integer                     NOT NULL,
    "Quantity"             integer                     NOT NULL,
    "Price"                integer                     NOT NULL,
    "PromoPrice"           integer                     NULL,
    "PromotionStartsAt"    timestamp without time zone NULL,
    "PromotionEndsAt"      timestamp without time zone NULL,
    "MaxStock"             integer                     NULL,
    "SoldCount"            integer                     NOT NULL DEFAULT 0,
    "PerPlayerLimit"       integer                     NULL,
    "PerPlayerLimitWindow" integer                     NOT NULL,
    "IsActive"             boolean                     NOT NULL DEFAULT TRUE,
    "Version"              integer                     NOT NULL DEFAULT 0,
    CONSTRAINT "PK_Market_ShopItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Market_ShopItems_Categories_CategoryId"
        FOREIGN KEY ("CategoryId") REFERENCES "market"."Categories" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_Market_ShopItems_CategoryId"
    ON "market"."ShopItems" ("CategoryId");
