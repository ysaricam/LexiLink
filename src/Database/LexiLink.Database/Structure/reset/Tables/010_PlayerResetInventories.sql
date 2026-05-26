CREATE TABLE IF NOT EXISTS "reset"."PlayerResetInventories"
(
    "PlayerId" uuid    NOT NULL,
    "Balance"  integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_PlayerResetInventories" PRIMARY KEY ("PlayerId")
);
