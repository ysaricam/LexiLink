CREATE TABLE IF NOT EXISTS "market"."Categories"
(
    "Id"                 uuid                        NOT NULL,
    "Name"               character varying(100)      NOT NULL,
    "SortOrder"          integer                     NOT NULL,
    "Icon"               character varying(64)       NULL,
    "IsActive"           boolean                     NOT NULL DEFAULT TRUE,
    "VisibilityStartsAt" timestamp without time zone NULL,
    "VisibilityEndsAt"   timestamp without time zone NULL,
    CONSTRAINT "PK_Market_Categories" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Market_Categories_SortOrder"
    ON "market"."Categories" ("SortOrder");
