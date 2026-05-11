CREATE TABLE IF NOT EXISTS "games"."Categories"
(
    "Id"          uuid NOT NULL,
    "Name"        text NOT NULL,
    "Description" text NOT NULL,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);
