CREATE TABLE IF NOT EXISTS "administration"."AdminActionAudit"
(
    "Id"          uuid                        NOT NULL,
    "OccurredOn"  timestamp without time zone NOT NULL,
    "AdminUserId" uuid                        NOT NULL,
    "ActionType"  text                        NOT NULL,
    "TargetType"  text                        NOT NULL,
    "TargetId"    text                        NULL,
    "PayloadJson" text                        NOT NULL,
    CONSTRAINT "PK_AdminActionAudit" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_AdminActionAudit_Actor_OccurredOn"
    ON "administration"."AdminActionAudit" ("AdminUserId", "OccurredOn" DESC);

CREATE INDEX IF NOT EXISTS "IX_AdminActionAudit_Target_OccurredOn"
    ON "administration"."AdminActionAudit" ("TargetType", "TargetId", "OccurredOn" DESC);

CREATE INDEX IF NOT EXISTS "IX_AdminActionAudit_OccurredOn"
    ON "administration"."AdminActionAudit" ("OccurredOn" DESC);
