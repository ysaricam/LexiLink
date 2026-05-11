CREATE TABLE IF NOT EXISTS "players"."PlayerAuthIdentities"
(
    "PlayerId"   uuid                        NOT NULL,
    "Provider"   varchar(32)                 NOT NULL,
    "ExternalId" text                        NOT NULL,
    "Email"      text                        NULL,
    "LinkedAt"   timestamp without time zone NOT NULL,
    CONSTRAINT "PK_PlayerAuthIdentities" PRIMARY KEY ("PlayerId", "Provider"),
    CONSTRAINT "FK_PlayerAuthIdentities_Players" FOREIGN KEY ("PlayerId")
        REFERENCES "players"."Players" ("Id")
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_PlayerAuthIdentities_Provider_ExternalId"
    ON "players"."PlayerAuthIdentities" ("Provider", "ExternalId");
