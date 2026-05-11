CREATE TABLE IF NOT EXISTS "games"."Links"
(
    "Id"          uuid    NOT NULL,
    "Value"       text    NOT NULL,
    "Description" text    NOT NULL,
    "IsActive"    boolean NOT NULL,
    "CategoryId"  uuid    NOT NULL,
    CONSTRAINT "PK_Links" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Links_Categories" FOREIGN KEY ("CategoryId")
        REFERENCES "games"."Categories" ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Links_CategoryId"
    ON "games"."Links" ("CategoryId");
