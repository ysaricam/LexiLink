CREATE TABLE IF NOT EXISTS "undo"."PlayerUndoInventories"
(
    "PlayerId" uuid    NOT NULL,
    "Balance"  integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_PlayerUndoInventories" PRIMARY KEY ("PlayerId")
);
