CREATE TABLE IF NOT EXISTS "games"."LinkOutgoingLinks"
(
    "LinkId"         uuid NOT NULL,
    "OutgoingLinkId" uuid NOT NULL,
    CONSTRAINT "PK_LinkOutgoingLinks" PRIMARY KEY ("LinkId", "OutgoingLinkId"),
    CONSTRAINT "FK_LinkOutgoingLinks_Source_Links" FOREIGN KEY ("LinkId")
        REFERENCES "games"."Links" ("Id")
        ON DELETE CASCADE,
    CONSTRAINT "FK_LinkOutgoingLinks_Target_Links" FOREIGN KEY ("OutgoingLinkId")
        REFERENCES "games"."Links" ("Id")
);
