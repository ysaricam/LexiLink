-- Sprint Q1 default seed: a single Daily quest so a fresh database
-- has something for the lazy chain to verify against. The Total
-- chain (1 -> 3 -> 5 -> 10 -> 50 -> 100) is left to admin tooling
-- so the chain-building UX is exercised end-to-end. Deterministic
-- id + ON CONFLICT DO NOTHING keep the script idempotent.
INSERT INTO "quests"."QuestDefinitions"
    ("Id",                                   "Name",          "Description",            "Trigger",            "Threshold", "Reward", "PrerequisiteQuestDefinitionId", "ProgressBaseline", "IsActive")
VALUES
    ('11111111-0000-0000-0000-000000000010', 'Günlük 3 Oyun', 'Bugün 3 oyun tamamla.',  'GameCompletedDaily', 3,           5,        NULL,                            'FromSnapshot',     TRUE)
ON CONFLICT ("Id") DO NOTHING;
