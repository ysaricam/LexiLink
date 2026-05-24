CREATE TABLE IF NOT EXISTS "hint"."PlayerHintInventories"
(
    "PlayerId" uuid    NOT NULL,
    "Balance"  integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_PlayerHintInventories" PRIMARY KEY ("PlayerId")
);
