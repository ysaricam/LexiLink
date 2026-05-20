-- Seed the four MVP quest definitions previously hardcoded in
-- Quests.Infrastructure/Domain/PlayerQuests/QuestCatalog.cs. Deterministic
-- ids so future migrations / admin tooling can reference the same rows
-- without lookup-by-type races. ON CONFLICT DO NOTHING keeps this safe to
-- re-run and idempotent across environments.
INSERT INTO "quests"."QuestDefinitions"
    ("Id", "QuestType", "Cadence", "Goal", "RewardAmount", "PrerequisiteQuestType", "IsActive")
VALUES
    ('11111111-0000-0000-0000-000000000001', 'FirstGameCompleted',  'OneTime', 1, 3, NULL,                  TRUE),
    ('11111111-0000-0000-0000-000000000002', 'ThreeGamesCompleted', 'OneTime', 3, 5, NULL,                  TRUE),
    ('11111111-0000-0000-0000-000000000003', 'AccountLinked',       'OneTime', 1, 5, 'ThreeGamesCompleted', TRUE),
    ('11111111-0000-0000-0000-000000000004', 'DailyThreeGames',     'Daily',   3, 5, NULL,                  TRUE)
ON CONFLICT ("Id") DO NOTHING;
