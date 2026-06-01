CREATE TABLE IF NOT EXISTS "ads"."RewardedAdGrants"
(
    "Id"            uuid                        NOT NULL,
    "PlayerId"      uuid                        NOT NULL,
    "DiamondAmount" integer                     NOT NULL,
    "TransactionId" text                        NOT NULL,
    "GrantedOn"     timestamp without time zone NOT NULL,
    CONSTRAINT "PK_RewardedAdGrants" PRIMARY KEY ("Id")
);

-- Idempotency: a verified AdMob SSV transaction grants Diamond at most once.
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Ads_RewardedAdGrants_TransactionId"
    ON "ads"."RewardedAdGrants" ("TransactionId");

-- Supports the per-player daily-cap count.
CREATE INDEX IF NOT EXISTS "IX_Ads_RewardedAdGrants_Player_GrantedOn"
    ON "ads"."RewardedAdGrants" ("PlayerId", "GrantedOn");
