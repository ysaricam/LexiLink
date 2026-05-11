CREATE INDEX IF NOT EXISTS "IX_Games_PlayerId_CategoryId_State_StartLinkId_TargetLinkId"
    ON "games"."Games" ("PlayerId", "CategoryId", "State", "StartLinkId", "TargetLinkId");
