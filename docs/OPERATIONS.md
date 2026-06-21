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
| `Authentication__TokenExchange__Mode` | Yes (for guest login) | `Disabled` | Controls `POST /auth/token`. `Disabled` rejects all token exchange → **no player can authenticate**. Use **`GuestDevice`** in Production to enable the guest-first flow (Guest provider only; Apple/Google rejected until real social sign-in is wired). `DevelopmentExternalToken` is allowed only outside `Production`. |
| `Authentication__SocialIdentity__GoogleClientIds__0` | Yes for Google sign-in | empty | Google OAuth client id accepted as the `aud` of Google ID tokens. Add more indexes for additional client ids. |
| `Authentication__SocialIdentity__AppleClientIds__0` | Yes for Apple sign-in | empty | Apple bundle id / service id accepted as the `aud` of Apple identity tokens. Add more indexes for additional ids. |
| `Authentication__AdminTokenExchange__Mode` | No | `Disabled` | Controls `POST /auth/admin/token`. Use `AdminSharedSecret` for the first production browser admin console. `DevelopmentExternalToken` is allowed only outside `Production`. |
| `Authentication__AdminTokenExchange__SharedSecret` | Yes for `AdminSharedSecret` | empty | Strong operator-owned token entered as the admin login "External token". Must be at least 32 characters and kept out of git. |

> **Guest auth note.** A guest's identity is its client-generated device id
> (a high-entropy random value the device keeps), which is the actual bearer
> credential. `GuestDevice` mode accepts the Guest provider with that handshake
> and rejects Apple/Google. Real social sign-in (server-side Google/Apple
> ID-token verification) is a planned follow-up; until then, leaving
> `TokenExchange__Mode=Disabled` in Production means even guests get 401.

Production baseline:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__LexiLinkDb='Host=...;Port=5432;Database=...;Username=...;Password=...'
Authentication__Mode=ProductionJwt
Authentication__Jwt__Issuer='LexiLink'
Authentication__Jwt__Audience='LexiLink.Api'
Authentication__Jwt__SigningKey='<at-least-32-character-secret>'
Authentication__TokenExchange__Mode=GuestDeviceAndSocial
Authentication__SocialIdentity__GoogleClientIds__0='<google-oauth-client-id>'
Authentication__SocialIdentity__AppleClientIds__0='com.wordlope.app'
Authentication__AdminTokenExchange__Mode=AdminSharedSecret
Authentication__AdminTokenExchange__SharedSecret='<at-least-32-character-admin-secret>'
```

## Development Defaults

Local development uses:

```bash
ConnectionStrings__LexiLinkDb='Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852'
Authentication__Mode=ProductionJwt
Authentication__Jwt__Issuer=LexiLink
Authentication__Jwt__Audience=LexiLink.Api
Authentication__Jwt__SigningKey=local-dev-signing-key-must-be-at-least-32-chars
Authentication__TokenExchange__Mode=DevelopmentExternalToken
```

The Flutter app uses `/players/guest` followed by `/auth/token`, then stores
the returned JWT. Keep local preview API runs in JWT mode unless intentionally
running backend-only smoke tests.

`DevelopmentBearer` accepts `Authorization: Bearer <player-guid>` and exists
only as a backend local/test convenience. Do not use it for production traffic
or normal Flutter preview sessions; stale JWTs will 401 against a
`DevelopmentBearer` API.

## Localization And Locale

Frontend UI localization supports Turkish, English, German, French, and
Spanish. Flutter ARB files are keyed by language code (`tr`, `en`, `de`,
`fr`, `es`) and unsupported device locales fall back to English.

Backend profile locale uses the existing Players format:

| Locale | Meaning |
| --- | --- |
| `tr-TR` | Turkish |
| `en-US` | English |
| `de-DE` | German |
| `fr-FR` | French |
| `es-ES` | Spanish |

Operational notes:

- Settings language changes apply live in the app, persist device-local via
  SharedPreferences, and best-effort write `Player.Locale` by preserving the
  current avatar and calling `PATCH /players/{id}/profile`.
- `Player.Locale` is validated as `^[a-z]{2}-[A-Z]{2}$`; use the
  region-qualified form when calling backend APIs.
- Phase 1 localizes app UI strings only. Phase 2 (Sprint CL) added the
  per-language content model: `games.Categories.Language` stores the content
  locale, existing content defaults to `tr-TR`, and
  `GET /categories?locale=xx-XX` filters player category lists by language
  (authoring handoff in `CONTENT_AUTHORING.md`). Backend rule, validation, and
  mixed cubit/API error messages remain **English** — Phase 3 (error-code
  translation) is **deferred** (low ROI for a mobile game; see
  `ROADMAP.md > Sprint L10N > Phase 3 — deferred`).

Content import:

```bash
dotnet run --project src/Tools/LexiLink.Tools.CategoryImporter/LexiLink.Tools.CategoryImporter.csproj -- \
  "$ConnectionStrings__LexiLinkDb" \
  docs/category-animals-en.json
```

`category.language` in the JSON is optional for older files and defaults to
`tr-TR`. Stable import ids include language, so the same category name can be
authored independently per locale.

For the full repeatable authoring handoff — JSON schema field reference,
graph design rules, importer validation, per-language stable-id behavior,
verify steps, and a content-ops checklist — see
[`CONTENT_AUTHORING.md`](CONTENT_AUTHORING.md). Authoring per-language word
graphs (DE/FR/ES) is a content-ops task; the code path is complete and needs
no change.

Admin content UI:

- `/admin/content` lists Games content categories.
- The language filter calls `GET /admin/content/categories?locale=xx-XX`.
- Create/edit category sends the explicit `language` field to
  `/admin/content/categories`.

Browser admin console:

- The Flutter web app already has admin routes under `/admin/*`.
- Build it against production with:

```bash
cd frontend
flutter build web --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.wordlope.com
```

- Caddy serves the files mounted from `ADMIN_WEB_ROOT` at
  `https://admin.<LEXILINK_DOMAIN>`. The directory must contain
  `index.html`; by default it is `./frontend/build/web`.
- Login path: `https://admin.<LEXILINK_DOMAIN>/admin/login`.
- Email must match an active admin bootstrapped via
  `Administration__Bootstrap__AdminEmails__0`.
- External token is the server-side
  `Authentication__AdminTokenExchange__SharedSecret` when
  `Authentication__AdminTokenExchange__Mode=AdminSharedSecret`.
- Add the exact admin origin to `Cors__AllowedOrigins__0`, otherwise browser
  requests to `https://api.wordlope.com` will be blocked by CORS. For
  Wordlope production this is `https://admin.wordlope.com`.

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

## Payments Module Settings

Payments owns Apple/Google IAP verification, server notification
reconciliation, and support-facing purchase history. The local/dev adapter
shells are intentionally fail-closed until real store credentials are
configured.

| Setting | Default | Notes |
| --- | --- | --- |
| `Payments__Apple__BundleId` | empty | iOS app bundle id expected in App Store verification responses. |
| `Payments__Apple__Environment` | `Sandbox` | Store environment for verification. Production must match the App Store deployment target. |
| `Payments__Apple__SharedSecret` / credential material | empty | App Store Server API / notification verification secrets. Store in secret config, not source. |
| `Payments__Google__PackageName` | empty | Android package name expected in Google Play verification responses. |
| `Payments__Google__Environment` | `Sandbox` | Store environment for verification. |
| `Payments__Google__ServiceAccountJson` / credential material | empty | Google Play Developer API credentials. Store in secret config, not source. |

Operational notes:

- `GET /payments/products?platform=Apple|Google` returns the active backend
  Diamond bundle catalog. Storefront price/currency stays client/store-owned.
- `POST /payments/iap/verify` is the normal grant path. The backend ignores
  client-supplied amount/price and resolves Diamond amount from
  `payments.PaymentProducts`.
- Duplicate Apple transaction ids, Google purchase tokens, or player
  client-request ids replay the existing purchase result without double grant.
- iOS clients finish StoreKit transactions only when the response returns
  `canFinishTransaction=true`.
- Google consume/acknowledge is backend-owned after Diamond delivery.
- `POST /admin/payments/purchases/{id}/retry-delivery` retries stuck
  `VerifiedButGrantFailed` deliveries and failed Google post-processing.
- Apple App Store Server Notifications V2 and Google RTDN endpoint surfaces
  persist raw notifications before idempotent processing. Real cryptographic
  verification requires production store credentials.

Pre-production manual store checks:

- Apple sandbox purchase grants Diamond once and allows transaction finish only
  after backend delivery.
- Google internal-test purchase grants Diamond once and backend
  consume/acknowledge succeeds.
- Replaying the same Apple transaction or Google purchase token does not grant
  twice.
- App kill/restart during a pending purchase replays the store proof and
  reaches a final delivered or retryable state.
- Store refund/revocation notifications update the ledger to
  `Refunded`/`Revoked` without automatic Diamond clawback in v1.

## Ads Module Settings

Ads owns the rewarded-ad → Diamond grant path, verified through AdMob
Server-Side Verification (SSV). The backend is the grant authority; the
client only requests/shows the ad and passes the player id as the SSV
`user_id`. Interstitial placements are pure frontend (no backend).

| Setting | Default | Notes |
| --- | --- | --- |
| `Ads__RewardedDiamondAmount` | `5` | Backend-owned Diamond granted per verified rewarded ad. The ad-network/client reward value is ignored. |
| `Ads__RewardedDailyLimit` | `10` | Max rewarded-ad grants per player per UTC day. Hitting the cap is a benign "no reward", not an error. |
| `Ads__Ssv__Mode` | `Production` | `Production` selects the fail-closed `AdMobSsvVerifier` (rejects until real key verification is wired). `DevelopmentFailOpen` selects the fail-open dev verifier (set in `appsettings.Development.json`) because Google's SSV servers cannot reach `localhost`. **Never use `DevelopmentFailOpen` in production.** |
| `Ads__Ssv__VerificationKeysUrl` | Google's verifier-keys URL | Source of AdMob's rotating public keys for signature verification (used by the real verifier once implemented). |

Frontend ad-unit ids are Google **test** ids by default and override via
`--dart-define` (`ADMOB_INTERSTITIAL_AD_UNIT_ID`,
`ADMOB_REWARDED_AD_UNIT_ID`); AdMob **app** ids live in the Android
`AndroidManifest.xml` and iOS `Info.plist` (also Google test ids until
real credentials arrive).

Operational notes:

- `GET /ads/rewarded/callback?...&signature=...&key_id=...` is the AdMob
  SSV ingress: **anonymous** but signature-verified inside the handler.
  It always returns 200 (a non-2xx makes AdMob retry); the body reports
  the outcome (`Granted` / `AlreadyGranted` / `DailyLimitReached` /
  `VerificationFailed`).
- Idempotency is on the SSV `transaction_id` (unique index): a replayed
  callback never grants twice.
- `GET /ads/rewarded/status` (authenticated player) returns the player's
  grants today, the daily cap, remaining, and Diamond-per-ad.
- A Diamond-grant failure (for example a player with no Diamond
  inventory) throws → 500 → AdMob retries the callback; v1 has no
  `VerifiedButGrantFailed` recovery state (the ledger is append-only).
- Local dev: Google cannot reach `localhost`, so the SSV callback won't
  fire automatically after watching — trigger
  `GET /ads/rewarded/callback` manually to exercise the grant.

Pre-production manual checks (operator/device-owned):

- Interstitial shows at ~1/3 of game starts and ~1/2 of finishes; a
  failed ad load never blocks navigation or the result sheet.
- Rewarded watch with a real AdMob/SSV setup grants exactly
  `Ads:RewardedDiamondAmount` once per verified `transaction_id`.
- Daily cap blocks further grants for the day; the watch button disables.
- UMP consent + iOS ATT prompts appear on startup before ad requests.
- Real AdMob account/ad-unit ids and SSV signature credentials are
  operator-owned (test ids ship in code/config).

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
| `GET /payments/products?platform=Apple\|Google` | `AuthenticatedPlayer` | Active Diamond bundle catalog for the requested store platform. Backend returns product id/store product id/Diamond amount; localized price comes from Apple/Google on the client. |
| `POST /payments/iap/verify` | `AuthenticatedPlayer` | Verifies an Apple transaction JWS/id or Google purchase token, records the IAP ledger row, grants Diamond exactly once, applies backend-owned post-processing where needed, and returns delivery/finish status. |
| `POST /payments/notifications/apple` | anonymous | App Store Server Notifications V2 ingress. Persists raw notification before verifier/processor handling. Production requires cryptographic verification credentials. |
| `POST /payments/notifications/google` | anonymous | Google RTDN ingress. Persists raw notification before verifier/processor handling. Production requires Pub/Sub/Google verification credentials. |
| `POST /admin/payments/purchases/{id:guid}/retry-delivery` | `AuthenticatedAdmin` | Retries recoverable paid-but-not-delivered purchases and failed Google post-processing. |
| `POST /quests/{id:guid}/claim` | `AuthenticatedPlayer` | Marks a `ReadyToClaim` quest as `Claimed`. Returns 204 NoContent. Cross-player or missing ids return 404 ProblemDetails (no id-leakage). Triggers `QuestClaimedIntegrationEvent` via the Quests outbox, which Energy consumes to grant the reward via `PlayerEnergy.GrantBonus`. |
| `POST /auth/admin/token` | anonymous | Exchanges an external admin identity for a first-party admin JWT (subject = AdminUserId, claims `role=Admin` + `admin_id`). Local development can use `dev:admin:{email}` with `DevelopmentExternalToken`; production can use `AdminSharedSecret` with `Authentication:AdminTokenExchange:SharedSecret`. 400 on missing fields, 401 on bad external token, 404 when email is not a registered active admin. `DevelopmentExternalToken` is rejected in Production. |
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
| `GET /admin/players/by-handle?handle=DisplayName%231234` | `AuthenticatedAdmin` | Returns `PlayerAdminDetailDto` by public player handle (`DisplayName#1234`). The `#` must be URL-encoded as `%23` by callers. 400 when the handle shape is invalid; 404 when no player matches. |
| `GET /admin/players/{playerId:guid}` | `AuthenticatedAdmin` | Returns `PlayerAdminDetailDto` (id, displayName, discriminator, handle, avatarUrl, locale, isGuest, isBanned, bannedReason, bannedAt, createdAt, authProvidersLinked). 404 when the player is unknown. |
| `POST /admin/players/{playerId:guid}/ban` | `AuthenticatedAdmin` | Mark a player banned (`{ reason }`, NotEmpty, max 500). Idempotent: re-banning the same player is a no-op. Banned tokens are refused at the auth boundary with 401. |
| `POST /admin/players/{playerId:guid}/unban` | `AuthenticatedAdmin` | Lift the ban. Idempotent. |
| `GET /admin/content/categories?locale=xx-XX` | `AuthenticatedAdmin` | List Games content categories; optional locale filters by `Category.Language`. |
| `GET /admin/content/categories/{id:guid}` | `AuthenticatedAdmin` | Read category details (name, description, language, linkCount). |
| `POST /admin/content/categories` | `AuthenticatedAdmin` | Create a new category (`{ name, description, language }`; language defaults to `tr-TR` for old callers). Returns 201 with new id. Audited under `Games.Category`. |
| `PATCH /admin/content/categories/{id:guid}` | `AuthenticatedAdmin` | Edit category name/description/language. Audited. |
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
