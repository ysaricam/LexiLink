CREATE TABLE IF NOT EXISTS "energy"."PlayerEnergies"
(
    "PlayerId"                uuid                        NOT NULL,
    "CurrentAmount"           integer                     NOT NULL,
    "MaximumAmount"           integer                     NOT NULL,
    "RechargeIntervalSeconds" integer                     NOT NULL,
    "LastRefilledOn"          timestamp without time zone NOT NULL,
    CONSTRAINT "PK_PlayerEnergies" PRIMARY KEY ("PlayerId")
);
