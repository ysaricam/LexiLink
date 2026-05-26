CREATE TABLE IF NOT EXISTS "games"."Games"
(
    "Id"               uuid           NOT NULL,
    "PlayerId"         uuid           NOT NULL,
    "CurrentLinkId"    uuid           NOT NULL,
    "State"            varchar(32)    NOT NULL,
    "CategoryId"       uuid           NOT NULL,
    "Difficulty"       varchar(32)    NOT NULL,
    "StartLinkId"      uuid           NOT NULL,
    "TargetLinkId"     uuid           NOT NULL,
    "Score"            integer        NULL,
    "MaxSteps"         integer        NOT NULL,
    "StepsTaken"       integer        NOT NULL,
    "HintsRemaining"   integer        NOT NULL,
    "HintsUsed"        integer        NOT NULL,
    "UndosUsed"        integer        NOT NULL,
    "ResetsUsed"       integer        NOT NULL,
    CONSTRAINT "PK_Games" PRIMARY KEY ("Id")
);
