CREATE TABLE IF NOT EXISTS "administration"."AdminUsers"
(
    "Id"           uuid                        NOT NULL,
    "Email"        text                        NOT NULL,
    "Role"         varchar(32)                 NOT NULL,
    "Status"       varchar(32)                 NOT NULL,
    "RegisteredOn" timestamp without time zone NOT NULL,
    "DisabledOn"   timestamp without time zone NULL,
    CONSTRAINT "PK_AdminUsers" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UX_AdminUsers_Email"
    ON "administration"."AdminUsers" ("Email");
