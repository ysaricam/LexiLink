CREATE TABLE IF NOT EXISTS "players"."Players"
(
    "Id"                  uuid                        NOT NULL,
    "DisplayName"         text                        NOT NULL,
    "DiscriminatorValue"  integer                     NOT NULL,
    "AvatarUrl"           text                        NULL,
    "Locale"              text                        NOT NULL,
    "CreatedAt"           timestamp without time zone NOT NULL,
    "IsGuest"             boolean                     NOT NULL,
    CONSTRAINT "PK_Players" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_Players_DisplayName_DiscriminatorValue"
    ON "players"."Players" ("DisplayName", "DiscriminatorValue");
