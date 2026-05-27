CREATE TABLE IF NOT EXISTS "diamond"."PlayerDiamondInventories"
(
    "PlayerId" uuid    NOT NULL,
    "Balance"  integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_PlayerDiamondInventories" PRIMARY KEY ("PlayerId")
);
