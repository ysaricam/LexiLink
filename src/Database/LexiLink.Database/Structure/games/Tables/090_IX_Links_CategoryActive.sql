CREATE INDEX IF NOT EXISTS "IX_Links_CategoryId_IsActive_Id"
    ON "games"."Links" ("CategoryId", "IsActive", "Id");
