# Operations

Runtime configuration and operational endpoints for LexiLink API.

## Configuration Sources

ASP.NET Core configuration precedence applies. Prefer environment variables for
deployment secrets and production values.

- Local development defaults live in
  `src/API/LexiLink.API/appsettings.Development.json`.
- Shared defaults live in `src/API/LexiLink.API/appsettings.json`.
- Environment variable keys use double underscores, for example
  `ConnectionStrings__LexiLinkDb`.

## Required Production Settings

The API fails fast at startup when required production values are missing or an
unsafe development mode is enabled.

| Setting | Required | Default | Notes |
| --- | --- | --- | --- |
| `ConnectionStrings__LexiLinkDb` | Yes | empty | PostgreSQL connection string. Empty or missing values fail startup. |
| `Authentication__Mode` | Yes | `DevelopmentBearer` | Use `ProductionJwt` outside local/dev test runs. `DevelopmentBearer` is blocked in `Production`. |
| `Authentication__Jwt__Issuer` | Yes for `ProductionJwt` | empty | JWT issuer expected by API token validation. |
| `Authentication__Jwt__Audience` | Yes for `ProductionJwt` | empty | JWT audience expected by API token validation. |
| `Authentication__Jwt__SigningKey` | Yes for `ProductionJwt` | empty | HMAC signing key. Must be at least 32 characters. Treat as a secret. |
| `Authentication__Jwt__AccessTokenLifetimeMinutes` | No | `60` | First-party access token lifetime. |
| `Authentication__TokenExchange__Mode` | No | `Disabled` | `DevelopmentExternalToken` is allowed only outside `Production`. |
| `Authentication__AdminTokenExchange__Mode` | No | `Disabled` | Controls `POST /auth/admin/token`. `DevelopmentExternalToken` is allowed only outside `Production`. |

Production baseline:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__LexiLinkDb='Host=...;Port=5432;Database=...;Username=...;Password=...'
Authentication__Mode=ProductionJwt
Authentication__Jwt__Issuer='LexiLink'
Authentication__Jwt__Audience='LexiLink.Api'
Authentication__Jwt__SigningKey='<at-least-32-character-secret>'
Authentication__TokenExchange__Mode=Disabled
```

## Development Defaults

Local development uses:

```bash
ConnectionStrings__LexiLinkDb='Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852'
Authentication__Mode=DevelopmentBearer
Authentication__TokenExchange__Mode=DevelopmentExternalToken
```

`DevelopmentBearer` accepts `Authorization: Bearer <player-guid>` and exists
only as a local/test convenience. Do not use it for production traffic.

## Administration Module Settings

| Setting | Default | Notes |
| --- | --- | --- |
| `Administration:Bootstrap:AdminEmails` | empty | String array of admin emails ensured on every API start. Idempotent: each email is normalized to lowercase and registered once. Production must supply this via env/secret store (no hardcoded admins in code or appsettings.json). |

Environment variable form (PascalCase double-underscore + zero-based index):

```bash
Administration__Bootstrap__AdminEmails__0='ops@lexilink.example'
Administration__Bootstrap__AdminEmails__1='admin@lexilink.example'
```

Bootstrap behavior:

- An `IHostedService` runs once on API start and calls
  `RegisterAdminUserCommand` per email. The command handler is idempotent
  on email (case-insensitive); duplicates short-circuit to the existing
  admin's id.
- One scope per email so each command owns a fresh DbContext (avoids EF
  OwnsOne shadow-FK conflicts when seeding multiple admins).
- Failures are logged and the API still starts — fix config and restart.
- Each new registration writes an `AdminUserRegisteredIntegrationEvent`
  to `administration.OutboxMessages`. The shared outbox processor
  publishes it via `IEventsBus`.

## Energy Module Settings

| Setting | Default | Notes |
| --- | --- | --- |
| `Energy:MaxAmount` | `5` | Maximum energy units a player can hold at full. |
| `Energy:RechargeIntervalSeconds` | `900` | Seconds between regenerated units. `900` = 15 minutes per unit. |
| `Energy:GameStartCost` | `1` | Units consumed when `StartGameCommand` runs through `IEnergyGuard`. |

All three keys are read by `EnergyConfigurationService` via `IConfiguration`.
Missing or unparseable values fall back to the defaults above.

## Background Processing Defaults

Outbox and inbox processing is scheduled by Quartz from the API host.

| Setting | Default | Notes |
| --- | --- | --- |
| `OutboxProcessing__PollingInterval` | `00:00:05` | Quartz trigger interval for outbox and Stats inbox/internal-command processing. |
| `OutboxProcessing__MaxRetryCount` | `10` | Outbox poison threshold. Stats inbox/internal-command processors also use a retry threshold of `10`. |
| `OutboxProcessing__RetryBackoff` | `00:00:30` | Outbox retry delay. Stats inbox/internal-command processors currently use a fixed `30s` delay. |

Processor logs include structured fields for operational search:

- `CorrelationId`
- `BackgroundJob`
- `ProcessorQueue`
- `ProcessorType`
- `QuartzFireInstanceId`
- `QuartzTrigger`

## Health And Operational Endpoints

| Endpoint | Auth | Purpose |
| --- | --- | --- |
| `GET /health/live` | anonymous | API process liveness. |
| `GET /health/ready` | anonymous | API readiness, including PostgreSQL connectivity and DbUp migration journal validation. |
| `GET /operations/processors` | `AuthenticatedPlayer` | Backlog/error visibility for `games-outbox`, `players-outbox`, `stats-inbox`, and `stats-internal-commands`. |
| `GET /energy/me` | `AuthenticatedPlayer` | Current player's energy snapshot: `currentAmount`, `maximumAmount`, `isFull`, `rechargeIntervalSeconds`, `lastRefilledOn`, `secondsUntilNextRefill`, `fullyRefilledAt`. Returns 404 ProblemDetails when the energy aggregate hasn't been initialized yet (normally a brief race after registration before the outbox processes `PlayerRegisteredIntegrationEvent`). |
| `GET /quests/me` | `AuthenticatedPlayer` | Current player's quests (`Active`, `ReadyToClaim`, `Claimed`, `Expired`) as a `PlayerQuestDto[]`. Lazy expiry projection is applied at read time. |
| `POST /quests/{id:guid}/claim` | `AuthenticatedPlayer` | Marks a `ReadyToClaim` quest as `Claimed`. Returns 204 NoContent. Cross-player or missing ids return 404 ProblemDetails (no id-leakage). Triggers `QuestClaimedIntegrationEvent` via the Quests outbox, which Energy consumes to grant the reward via `PlayerEnergy.GrantBonus`. |
| `POST /auth/admin/token` | anonymous | Exchanges an external admin identity (currently `dev:admin:{email}` development verifier) for a first-party admin JWT (subject = AdminUserId, claims `role=Admin` + `admin_id`). 400 on missing fields, 401 on bad external token, 404 when email is not a registered active admin. `Authentication:AdminTokenExchange:Mode` controls the verifier; `DevelopmentExternalToken` is rejected in Production. |
| `GET /admin/whoami` | `AuthenticatedAdmin` | Returns `{ adminUserId, role }` for the current admin. 401 anonymous, 403 player-only token. In dev-bearer mode any GUID that matches an Active `administration.AdminUsers.Id` is recognized; in production-JWT mode the role and admin_id claims are required and the AdminUser is re-checked for Active status (revoked tokens fail with 401). |
| `GET /admin/audit` | `AuthenticatedAdmin` | Paged list of admin actions from `administration.AdminActionAudit`, newest first. Optional filters: `adminUserId`, `targetType`, `targetId`. Pagination via `offset` / `limit` (default 50, max 200). Each row carries actor, action type, target type/id, opaque JSON payload, and OccurredOn. Populated by the per-module admin auditing decorator (first wired in B7 for Quests). |
| `GET /admin/quests/definitions` | `AuthenticatedAdmin` | All quest definitions including deactivated. |
| `POST /admin/quests/definitions` | `AuthenticatedAdmin` | Create a new quest definition (`{ questType, cadence, goal, rewardAmount, prerequisiteQuestType? }`). 201 with new id; 400 when a definition already exists for the type. |
| `PUT /admin/quests/definitions/{id:guid}` | `AuthenticatedAdmin` | Update goal / reward / prerequisite on an existing definition. 404 when id is unknown. |
| `POST /admin/quests/definitions/{id:guid}/deactivate` | `AuthenticatedAdmin` | Soft-deactivate the definition. Existing PlayerQuests are untouched; new issuances stop. |
| `POST /admin/quests/players/{playerId:guid}/issue` | `AuthenticatedAdmin` | Force-issue a quest to a player. Wraps the internal `IssueQuestCommand` — same prerequisite / cadence / idempotency rules. |
| `POST /admin/quests/players/{playerId:guid}/{playerQuestId:guid}/reset` | `AuthenticatedAdmin` | Reset progress + state of a PlayerQuest to Active. Daily quests get a fresh "next UTC midnight" window; OneTime stays open-ended. 404 when the playerQuestId is unknown. |
| `POST /admin/players/{playerId:guid}/energy/set` | `AuthenticatedAdmin` | Snap a player's energy to a specific amount (`{ amount }`, 0 ≤ amount ≤ max). 404 when the player has no energy aggregate yet. |
| `POST /admin/players/{playerId:guid}/energy/grant` | `AuthenticatedAdmin` | Grant bonus energy (`{ amount }`, > 0). Wraps the internal `GrantEnergyCommand` — intentionally permits over-max balance. |
| `POST /admin/players/{playerId:guid}/energy/reset` | `AuthenticatedAdmin` | Restore current to maximum and rearm the recharge timestamp. |
| `GET /admin/players/{playerId:guid}` | `AuthenticatedAdmin` | Returns `PlayerAdminDetailDto` (id, displayName, discriminator, handle, avatarUrl, locale, isGuest, isBanned, bannedReason, bannedAt, createdAt, authProvidersLinked). 404 when the player is unknown. |
| `POST /admin/players/{playerId:guid}/ban` | `AuthenticatedAdmin` | Mark a player banned (`{ reason }`, NotEmpty, max 500). Idempotent: re-banning the same player is a no-op. Banned tokens are refused at the auth boundary with 401. |
| `POST /admin/players/{playerId:guid}/unban` | `AuthenticatedAdmin` | Lift the ban. Idempotent. |
| `POST /admin/content/categories` | `AuthenticatedAdmin` | Create a new category (`{ name, description }`). Returns 201 with new id. Audited under `Games.Category`. |
| `PATCH /admin/content/categories/{id:guid}` | `AuthenticatedAdmin` | Edit category name/description. Audited. |
| `POST /admin/content/links` | `AuthenticatedAdmin` | Create a new link (`{ categoryId, value, description, isActive }`). Returns 201 with new id. Audited under `Games.Link`. |
| `POST /admin/content/links/{linkId:guid}/outgoing/{outgoingLinkId:guid}` | `AuthenticatedAdmin` | Add an outgoing edge between two links. Audited. |
| `DELETE /admin/content/links/{linkId:guid}/outgoing/{outgoingLinkId:guid}` | `AuthenticatedAdmin` | Remove an outgoing edge. Audited. |
| `POST /admin/content/links/{id:guid}/activate` | `AuthenticatedAdmin` | Re-activate a soft-deactivated link. Audited. |
| `POST /admin/content/links/{id:guid}/deactivate` | `AuthenticatedAdmin` | Soft-deactivate a link (kept for history). Audited. |

`/operations/processors` returns unprocessed, ready, scheduled retry, poisoned,
failed counts, oldest unprocessed timestamp, and a small error sample per queue.

Readiness performs a lightweight schema drift guard instead of a full schema
diff. The API artifact carries the expected DbUp SQL scripts and compares them
with `public.MigrationsJournal`. If a script in the artifact has not been
journaled, `/health/ready` returns unhealthy. This catches the operationally
important drift case: deploying code before applying its migration scripts.

## Database Migrations

The API does not run schema migrations on startup. Apply DbUp scripts before
starting the API.

The migrator:

- creates the target PostgreSQL database when it does not exist;
- scans `src/Database/LexiLink.Database/Structure` recursively;
- executes pending scripts in DbUp order;
- records applied scripts in `public.MigrationsJournal`;
- is safe to re-run when scripts have already been journaled.

The API does not mutate the database during readiness checks. It only validates
that the scripts shipped with the running API artifact are already present in
the DbUp journal.

### Standard Command

```bash
dotnet run \
  --project src/Database/LexiLink.DatabaseMigrator/LexiLink.DatabaseMigrator.csproj \
  -- \
  "$ConnectionStrings__LexiLinkDb" \
  src/Database/LexiLink.Database/Structure
```

CI uses PostgreSQL 17 with database `lexilink`, user `lexiadmin`, and password
`0852`.

Five module schemas live under `src/Database/LexiLink.Database/Structure/`:
`games`, `players`, `stats`, `energy`, `quests`. Each schema owns its own
`OutboxMessages` table; outbox PK names are namespaced
(`PK_Games_OutboxMessages`, `PK_Quests_OutboxMessages`, …) to keep them
disjoint at the SQL level.

### Fresh Database

Use this path for a new local, test, staging, or production database.

1. Provision PostgreSQL and a user with permission to create/use the target
   database.
2. Set `ConnectionStrings__LexiLinkDb`.
3. Run the standard migration command.
4. Confirm the migrator reports success and creates `public.MigrationsJournal`.
5. Start the API.
6. Check `GET /health/ready`.

Expected behavior: all schema, table, index, and view scripts are applied once.
Re-running should report zero pending scripts.

### Existing Database

Use this path before deploying an API version that expects new schema.

1. Take a database backup or snapshot before applying migrations.
2. Confirm the code revision and the SQL scripts being deployed are the same
   artifact/version.
3. Run the standard migration command against the existing database.
4. Confirm the pending script count matches the expected release notes.
5. Confirm success in the migrator output.
6. Start or roll forward the API deployment.
7. Check `GET /health/ready` and `GET /operations/processors`.

Do not edit previously journaled scripts. Add a new numbered script for every
schema change, including corrections.

### Failure During Migration

If the migrator fails:

1. Do not start the new API version.
2. Save the migrator output and the failing script name.
3. Inspect `public.MigrationsJournal` to see which scripts were applied.
4. Inspect the database for partial objects created by the failing script.
5. Recover using one of these paths:
   - restore the pre-migration backup or snapshot;
   - manually undo the partial change, then rerun after fixing the script before
     it has been journaled;
   - add a new forward-only corrective script when the failed or problematic
     change has already been journaled in any shared environment.

The preferred shared-environment recovery is restore or forward-only corrective
script. Manual changes must be recorded in the incident/release notes and
followed by a script that brings future environments to the same state.

### Rollback Policy

DbUp migrations are forward-only in this repository. There is no automatic
down-migration mechanism.

- Application rollback is allowed only when the previous API version is
  compatible with the migrated schema.
- Destructive schema changes require a staged deployment: add nullable/new
  structures first, deploy compatible code, backfill if needed, then remove old
  structures in a later release.
- For emergency rollback after an incompatible migration, restore the database
  backup/snapshot taken before migration.

## Local Verification

```bash
dotnet build LexiLink.sln --no-restore --disable-build-servers -v minimal -m:1
./scripts/test.sh --no-restore -v minimal
```

For a production-mode HTTP smoke check against local PostgreSQL:

```bash
./scripts/smoke.sh
```

The smoke script builds the API, applies DbUp migrations, starts the API with
`ASPNETCORE_ENVIRONMENT=Production` and `Authentication__Mode=ProductionJwt`,
then checks `/health/live` and `/health/ready` over HTTP. Override
`ConnectionStrings__LexiLinkDb` or `LEXILINK_SMOKE_PORT` when needed.
