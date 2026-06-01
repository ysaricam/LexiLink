# activeContext.md

Project'in o anki yönü ve en yakın sıra. Geçmiş teslimatlar `progress.md`,
uzun vadeli plan `ROADMAP.md`, mimari karşılaştırma notları
`kamil-modular-monolith-comparison.md` içindedir.

> Last updated: 2026-06-01 (Sprint GO — GO5 closed; backup + server hardening complete.)

---

## Active Sprint

**Sprint GO — Production Launch (Hetzner) — active 2026-06-01.** Goal: take
LexiLink live on a single Hetzner Ubuntu VPS. **API only — no web frontend;**
the game ships to the iOS/Android stores. Backend = Docker Compose: Caddy
(auto-HTTPS) → .NET 10 API; PostgreSQL 17 in a container; DbUp migrator as a
one-shot before the API. Locked decisions + 6-slice plan (GO1–GO6) in
`ROADMAP.md > Sprint GO`. Domain is **wordlope.com**; production API is
`https://api.wordlope.com`.

**GO1 ✅ done (uncommitted on `main`, 2026-06-01):** containerization. Added
root `Dockerfile` (multi-stage: `dotnet/sdk:10.0` build → `aspnet:10.0`
runtime) publishing the API to `/app` and the DbUp migrator to `/app/migrator`
in one image; `ASPNETCORE_URLS=http://+:8080`, `ASPNETCORE_ENVIRONMENT=Production`,
`EXPOSE 8080`, default `ENTRYPOINT` runs the API (compose's `migrate` one-shot
overrides it). Added `.dockerignore` (excludes `bin/`/`obj/`, `frontend/`,
docs/git/CI/scripts, secrets, and iCloud `* 2.*` duplicates). **Key wiring:**
the API csproj already copies `Database/Structure/**/*.sql` into its publish
output, so both the migrator one-shot and the API `/health/ready` journal
check read SQL from `/app/Database/Structure` — no separate copy needed.
**Validation (local Docker unavailable on the dev Mac):** Release `dotnet
publish` verified for both projects — API output has `LexiLink.API.dll` + **84**
SQL scripts under `Database/Structure` (matches source), migrator output has
`LexiLink.DatabaseMigrator.dll`; both exit 0 (only pre-existing nullable
warnings). Full image build (base-image pull + runtime) validates on the
server at GO4.

**GO-A ✅ done (uncommitted on `main`, 2026-06-01):** production auth —
**launch blocker fixed.** Discovered while planning GO2's `.env.example`:
Production had no usable `IExternalIdentityVerifier`. `Authentication:Mode`
must be `ProductionJwt` (DevelopmentBearer blocked) and `TokenExchange:Mode`
must be non-dev (DevelopmentExternalToken blocked) → only `Disabled` was
selectable → `DisabledExternalIdentityVerifier` makes `POST /auth/token`
return 401 for everything → **no player, not even a guest, could authenticate;
the deployed game would be unplayable.** Fix (user chose the minimal
guest-only path): added `ExternalIdentityValidationMode.GuestDevice` +
`GuestExternalIdentityVerifier` (accepts only the `Guest` provider with the
existing `dev:Guest:{deviceId}` client handshake — the deviceId is the actual
bearer credential; Apple/Google return `false` until real social sign-in is
wired), selected via a switch in `Program.cs`. `LexiLinkAuthOptionsValidator`
already permits any non-`DevelopmentExternalToken` mode in Production, so
`GuestDevice` is allowed with no validator change and no client change.
Production sets `Authentication__TokenExchange__Mode=GuestDevice`. Gate: API
build 0 errors (19 pre-existing warnings); 5/5 focused
`GuestExternalIdentityVerifierTests` (Guest-accept, token-mismatch reject,
empty-id reject, Apple/Google reject). **Follow-up (not GO):** real
server-side Google/Apple ID-token verification — and **gate real-money IAP
until it exists** (guest accounts are device-bound). See `ROADMAP.md > Sprint
GO > Deliberate non-actions`.

**GO2 ✅ done (uncommitted on `main`, 2026-06-01):** compose stack + Caddy +
env template. Added `docker-compose.yml` (services: `postgres` 17 + named
`pgdata` volume + `pg_isready` healthcheck; `migrate` one-shot that overrides
the image entrypoint to run the DbUp migrator against `/app/Database/Structure`,
gated on postgres-healthy, `restart: no`; `api` gated on
migrate-completed-successfully + postgres-healthy, `curl /health/live`
healthcheck, `expose 8080`; `caddy` 80/443 + `caddy_data`/`caddy_config`
volumes). Shared image/build/env via a YAML `x-app` anchor; `${LEXILINK_IMAGE:-lexilink:local}`
(GO6 swaps to the GHCR tag). Added `Caddyfile` (`api.{$LEXILINK_DOMAIN}`
reverse-proxies `api:8080`, auto-TLS via `{$LEXILINK_ACME_EMAIL}`) and
`.env.example` documenting every prod key (domain/ACME, Postgres creds +
matching connection string with `Host=postgres`, ProductionJwt + 32+ char
signing key, `TokenExchange__Mode=GuestDevice`, admin bootstrap emails, empty
CORS). Added `curl` to the Dockerfile runtime stage for the api healthcheck.
**Validation:** compose YAML + `<<: *app` anchor merge + `depends_on`
conditions parse-validated; Docker is unavailable on the dev Mac so a full
`docker compose up` validates on the server at GO4. **Secondary gap surfaced:**
production **admin** login is also unimplemented (`AdminTokenExchange__Mode=Disabled`
→ `/auth/admin/token` 401); not a launch blocker (content imports via the
`CategoryImporter` CLI; the game needs no admin intervention) — production
admin verifier is a follow-up. Recorded in `ROADMAP.md > Sprint GO`.

**GO3 ✅ done (uncommitted on `main`, 2026-06-01):** store build readiness
(no web). Added `docs/MOBILE_RELEASE.md` — prod API wiring
(`--dart-define LEXILINK_API_BASE_URL=https://api.<domain>`; the default is
`localhost`, so a release build *must* pass it), real-AdMob id wiring
(`ADMOB_INTERSTITIAL/REWARDED_AD_UNIT_ID` defines + manifest/plist app ids) +
the SSV callback URL (`https://api.<domain>/ads/rewarded/callback`), IAP
product setup (gated until social sign-in), `flutter build appbundle`/`ipa`
release commands, and a store-readiness checklist. **Blockers surfaced for the
operator:** Android `applicationId` (`build.gradle.kts`) and iOS bundle id
(`project.pbxproj`) are still the **`com.example.*` Flutter placeholders →
cannot publish**; must become a real reverse-DNS id (e.g. `com.lexilink.app`).
Version is `0.1.0+1` (bump to `1.0.0+1`); display name is `lexilink_app`
(→ `LexiLink`). Signing material, store accounts, and real credentials are
operator-owned. No code change this slice (docs only).

**GO4 ✅ done (operator, 2026-06-01):** server provisioning + first backend
deploy. The backend is installed on the server and `https://api.wordlope.com`
is healthy. Domain acquisition/DNS/TLS are no longer launch blockers.

**GO5 ✅ done (operator + repo, 2026-06-01):** backups + ops hardening.
Repo-side: added `scripts/backup-db.sh` for PostgreSQL custom-format dumps
with checksum, format verification, local retention pruning, and server-safe
defaults under `/opt/lexilink/backups/postgres`; added
`scripts/restore-db.sh` for explicit-confirm restore drills; added
`docs/DEPLOYMENT.md` runbook covering deploy, content import, backup cron,
offsite copy, restore, rollback, firewall, SSH hardening, resource/log checks,
secret rotation, and incident commands. `docker-compose.yml` now has log
rotation plus conservative CPU/memory/pids limits for postgres/api/caddy.
Server-side: manual backup completed at
`/opt/lexilink/backups/postgres/lexilink-20260601T171539Z.dump`, copied
off-server to the operator's PC, restored successfully, nightly backup cron
installed, `ufw` applied, SSH hardening applied, and
`https://api.wordlope.com/health/ready` verified healthy afterward.

**GO6 🔵 in progress (repo-side prepared, 2026-06-01):** CI/CD. Added
`.github/workflows/deploy-production.yml`: manual dispatch or `v*` tag builds
the Docker image, pushes `ghcr.io/ysaricam/lexilink:<git-sha>` plus `latest`,
SSHes to the VPS, checks out the exact commit under `/opt/lexilink/app`, runs
`LEXILINK_IMAGE=<sha-image> ./scripts/deploy.sh`, and verifies
`https://api.wordlope.com/health/ready`. Updated `scripts/deploy.sh` so local
manual deploy still builds, while GHCR deploy pulls a prebuilt image and starts
Compose with `--no-build`. Updated `docs/DEPLOYMENT.md` with required GitHub
secrets and server prerequisites.

**Next action: configure GitHub secrets and run first workflow deploy** —
`PROD_SSH_HOST`, `PROD_SSH_USER`, `PROD_SSH_KEY`, optional `PROD_SSH_PORT`, and
`GHCR_TOKEN` if the package remains private.

### Previous Sprint Context

**Sprint CL — Content Localization (Phase 2) — closed 2026-06-01.** Goal: move from
localized app chrome to localized **game content**. Unlike UI strings,
word-graph content is authored per language; a Turkish graph cannot be
machine-translated into a valid English/German graph. Locale codes remain
region-qualified (`tr-TR`/`en-US`/`de-DE`/`fr-FR`/`es-ES`) to align with
`Player.Locale`.

**CL1 ✅ done (uncommitted on `main`, 2026-06-01):** content language
foundation. Added `Category.Language` to the Games domain + EF mapping +
DbUp migration (`games.Categories.Language`, default `tr-TR`, indexed by
language/name) and projected it through `v_Categories`,
`CategoryListItemDto`, and `CategoryDetailsDto`. Admin category create/edit
commands and `/admin/content/categories` accept `Language` while preserving
`tr-TR` as the backward-compatible default. Player-facing
`GET /categories?locale=xx-XX` filters by content language. Frontend
category loading now sends the active `LocaleCubit` backend locale; guest
registration uses the selected locale instead of hardcoded `en-US`. Gate:
DbUp applied **2 pending scripts**; solution build **0 warnings / 0
errors**; Games unit **94/94**; Games integration **35/35**; ArchTests
**67/67**; `flutter analyze` clean; Flutter **166/166**.

**CL2 ✅ done (uncommitted on `main`, 2026-06-01):** first playable
`en-US` content graph. Added `docs/category-animals-en.json` (Animals,
`en-US`, 12 links / 30 directed edges) and made `CategoryImporter`
language-aware. The importer now reads optional `category.language`, defaults
older JSON to `tr-TR`, uses language in stable IDs so same-name categories
can exist per locale, and writes `games.Categories.Language`. Imported
Animals `[en-US]` locally and re-imported Spor `[tr-TR]` to verify backward
compatibility. Added Games integration coverage for creating and starting a
game with English content. Gate: solution build **0 warnings / 0 errors**;
importer build **0 warnings / 0 errors**; Games unit **94/94**; Games
integration **36/36**; ArchTests **67/67**; `flutter analyze` clean.

**CL3 ✅ done (uncommitted on `main`, 2026-06-01):** admin content
language controls. Added admin read endpoints for Games content categories:
`GET /admin/content/categories?locale=xx-XX` and
`GET /admin/content/categories/{id}`. Added Flutter `admin_content` feature
(repository/cubit/screen) under `/admin/content`, wired it into
`AppAdminShell`, and exposed a language filter plus create/edit category
dialog with `Language` selection. Added localized admin content strings
across `en/tr/de/fr/es` and regenerated `AppLocalizations`. Gate: solution
build **0 warnings / 0 errors**; API tests **50/50**; ArchTests **67/67**;
Flutter **173/173**; `flutter analyze` clean. While running API tests, fixed
the test factories that expected DevelopmentBearer GUIDs to explicitly
override `Authentication:Mode`, because `appsettings.Development` now
defaults local dev to ProductionJwt.

**CL4 ✅ done (uncommitted on `main`, 2026-06-01):** content-ops handoff.
Decided to document the repeatable authoring handoff rather than author
DE/FR/ES graphs in-repo — the content-language code path (CL1–CL3) is
complete and per-language word graphs are a content-ops effort, not a code
task. Added `docs/CONTENT_AUTHORING.md`: the `lexilink/category/v1` JSON
schema field-by-field (incl. which fields are imported vs advisory —
`wikipediaUrl`/`depthFromAnchor`/`metadata` are parsed but **not** written by
the importer), graph design rules (directed edges, author both directions for
two-way movement, keep connected), the importer's six validation rules,
language-aware stable-id behavior (same category name coexists per locale;
re-import idempotent; edges rebuilt each run), a step-by-step new-language
authoring walkthrough, the importer command, a verify section
(`GET /categories?locale=xx-XX` + `/admin/content`), admin-UI-vs-JSON editing
boundary, a content-ops checklist, and out-of-scope notes. No code change.
Sprint CL closed. Gate: docs-only slice; no code/test churn (existing CL3
gate — solution build 0/0, ArchTests 67/67, Flutter 173/173 — stands).

**Localization Phase 3 — deferred (decided 2026-06-01).** Backend
rule/validation/error-message localization (stable error codes + client
translation) is **parked in the backlog**, not cancelled. Rationale: this is
a mobile game; ~95% of the 78 `IBusinessRule` messages are internal
invariants a player never sees, and the handful a player actually hits
(insufficient energy/diamond, daily cap, game-state) don't justify a full
5-language sprint + cubit rewiring + test churn. If revisited, prefer a
**mini-slice** localizing only the ~6 player-facing messages; full Phase 3
only on a concrete external need. Seam already exists (middleware emits
`extensions["rule"]`; frontend `ApiProblemDetails` reads extensions). Full
rationale in `ROADMAP.md > Sprint L10N > Phase 3 — deferred`.

**Next action:** open — no localization slice queued. DE/FR/ES content
authoring is operator/content-ops owned and follows `docs/CONTENT_AUTHORING.md`.
Both localization phases that needed code (Phase 1 UI, Phase 2 content model)
are closed; next direction is a new product sprint to be chosen with the user.

### Previous Sprint Context

**Sprint L10N — Localization (App UI i18n) — closed 2026-06-01 for
Phase 1 UI i18n.** L1–L8 delivered the app UI localization layer for
Turkish, English, German, French, Spanish. `flutter analyze` is clean and
Flutter tests are **166/166**. Game content moved to this Sprint CL; backend
rule/validation message localization remains Phase 3.

**L1 ✅ done (uncommitted on `main`, 2026-05-31):** i18n infrastructure.
Added `flutter_localizations` + `intl` + `generate: true`, `l10n.yaml`
(output to `lib/l10n`, non-synthetic, non-nullable getter), `app_en.arb`
template + `tr/de/fr/es` ARBs (seed keys: `appTitle`, `settingsTitle`,
`languageLabel`, `commonCancel/Apply/Retry`), `flutter gen-l10n` →
`AppLocalizations`, and a `context.l10n` extension
(`lib/shared/l10n/`). `MaterialApp.router` wired with
`localizationsDelegates` + `supportedLocales` + `onGenerateTitle` +
`localeListResolutionCallback` (device language match → else English).
Locale is device-driven in L1; **L2** adds the explicit `LocaleCubit`
override. Gate: `flutter analyze` clean (12 pre-existing info warnings
unchanged), Flutter **158/158** (+2 — supported-locales set + per-locale
string resolution). String extraction (L3–L6) deliberately not started;
L1 is infra only.

**L2 ✅ done (uncommitted on `main`, 2026-05-31):** locale preference +
picker. `AppLanguage` enum (5 languages, each carrying ARB `code`,
`backendLocale` `xx-XX`, and `nativeName` endonym) +
`LocalePreferencesRepository` (SharedPreferences + InMemory) +
`PlayerLocaleWriter` (`ApiPlayerLocaleWriter` GETs the player to preserve
the avatar, then `PATCH /players/{id}/profile`; `Noop` for tests) +
`LocaleCubit` (load = stored else device language else English; `setLanguage`
= emit + persist device-local + best-effort `Player.Locale` write). Added
`ApiClient.patchJson`. `LocaleCubit` provided above the router via
`MultiBlocProvider`; `MaterialApp.router` `locale` now driven by a
`BlocBuilder<LocaleCubit, AppLanguage>`. `SettingsScreen` gains a language
dropdown (endonyms) and uses the **first real l10n keys**
(`settingsTitle`, `languageLabel`). The `xx-XX` codes satisfy
`LocaleMustBeValidFormatRule`. Gate: `flutter analyze` clean (12
pre-existing), Flutter **166/166** (+8; existing `settings_screen_test`
updated for the new l10n delegates + `LocaleCubit` provider).

**L3 ✅ done (uncommitted on `main`, 2026-05-31):** string extraction —
gameplay surface (game/home/categories). ~55 ARB keys added across all 5
languages (en/tr/de/fr/es), including ICU placeholders
(`hudSteps(taken,max)`, `hintAction(balance)`,
`outcomeCompletedSubtitle(target)`, etc.). All static UI strings in
`game_screen`, `home_screen`, `category_selection_screen` now go through
`context.l10n.*` (titles, labels, tooltips, buttons, dialogs, HUD chips,
result sheet, outcome titles/subtitles, action loading messages). Threaded
`context` into `_actionMessage` and `_outcomeFor`. Brand wordmark
`'LexiLink'` left literal (deliberate non-translation). Gate:
`flutter analyze` clean (12 pre-existing), Flutter **166/166** (no test
churn — `en` ARB values mirror the prior copy).

**Decision (scope boundary):** L3–L6 localize **presentation-layer**
strings only. **Cubit-emitted `message` strings stay English** (e.g.
"We could not load the game. Try again.", "Guest session is missing.")
because they intermix with `ApiException` server messages and a
`game_start_cubit` test asserts the exact text — uniform message
localization is **Phase 3** (error-code approach). Widget-side literal
fallbacks (`?? 'Try again.'`) were localized to `context.l10n.commonTryAgain`.

**L4 ✅ done (uncommitted on `main`, 2026-06-01):** string extraction —
economy surface. ~40 more ARB keys × 5 languages (incl. ICU
placeholders `promoPrice/price/stockLabel/yourRemaining/buyConfirmTitle/
buyConfirmMessage/rewardWatchEarn/rewardCardTitle/rewardToday`). Localized
`market_screen` (incl. the stray Turkish `'Mağaza'` → `marketTitle`),
`payment_screen`, `diamond_badge`, and `earn_diamonds_screen`: titles,
loading/empty/error states, price pills, stock/remaining, buy confirm
dialog, bundle action labels, rewarded watch button/card/footer + snackbar.
New common keys (`commonBuy/Unavailable/Processing/CheckBackLater/Unlimited`).
Gate: `flutter analyze` clean (12 pre-existing), Flutter **166/166** (no
test churn). Same scope boundary as L3 (cubit messages stay English →
Phase 3).

**L5 ✅ done (uncommitted on `main`, 2026-06-01):** string extraction —
quests/profile/leaderboard/settings. ~38 more ARB keys × 5 languages,
including the ICU **plural** `providersLinked`. Localized `quests_screen`
(fixed several more stray Turkish literals → proper keys incl. quest state
badges), `profile_summary_screen` (stat labels, guest/provider labels),
`leaderboard_screen` (period subtitles/empty messages/tabs — threaded
`context` into `_periodSubtitle`/`_emptyMessage`), and the
`settings_screen` audio section. `splash` only has the `'LexiLink'`
wordmark (left literal); auth has no presentation screen. Gate:
`flutter analyze` clean (12 pre-existing), Flutter **166/166**.

**L6 ✅ done (uncommitted on `main`, 2026-06-01):** string extraction —
admin features + shared widgets/dialogs. Added the admin/shared ARB key set
across all 5 locales and regenerated `AppLocalizations`. Localized
`app_admin_shell`, `admin_login_screen`, admin quest catalog/form,
players, energy/hint/undo/reset/diamond consoles, market admin, audit log,
and shared `AppErrorState`/`AppLoadingState` defaults. Quest enum labels
and market enum/limit labels now resolve through l10n at presentation time.
Cubit/API error messages remain English per the Phase 3 boundary. Admin
widget tests now include l10n delegates and English expectations were
updated. Gate: `flutter analyze` has only the 6 pre-existing info warnings;
Flutter **166/166**.

**L7 ✅ done (uncommitted on `main`, 2026-06-01):** translation polish.
Updated the newly stabilized L6 admin/shared key set in `tr`/`de`/`fr`/`es`
instead of leaving English copies. TR is authored/polished; DE/FR/ES are
usable draft translations for admin/common controls, dialogs, enum labels,
inventory consoles, market admin, and audit. Only natural same-as-English
values remain (brand, symbols/format strings, or shared technical terms
like `Admin`, `Audit`, `Offset`, `Normal`). Regenerated
`AppLocalizations`. Gate: `flutter analyze` has only the 6 pre-existing
info warnings; Flutter **166/166**.

**L8 ✅ done (uncommitted on `main`, 2026-06-01):** tests + analyze + docs
close-out. Re-ran the final Flutter gate after L1–L7: Flutter **166/166**
passed and `flutter analyze` is now **clean (No issues found)** after the
post-closeout analyzer-info cleanup. Closed the sprint docs and recorded
the Phase 1 boundaries: game content remains Phase 2; backend/cubit/API
message localization remains Phase 3. Updated global/frontend progress,
roadmap, glossary, and operations notes for locale behavior.

**Sprint AD — Advertising (AdMob) — closed 2026-05-31 for
repo-deliverable work (AD1–AD7).** Full-stack sprint: three
ad placements — interstitial @ game start (**1/3** random), interstitial
@ game end (**1/2** random), and a **rewarded ad → Diamond**. SDK:
`google_mobile_ads` (AdMob), mobile-only, web-safe. The rewarded reward
is **backend-verified** via AdMob Server-Side Verification (SSV): a new
**Ads** bounded context (schema `ads`) verifies AdMob's signed callback,
enforces idempotency on `transaction_id` + a daily-cap rule, then grants
through `IDiamondGrant` — mirroring the Payments "backend is the grant
authority" discipline; real signature verification stays behind a
fail-closed shell with a dev verifier for local testing. Locked: **5
Diamond/ad, daily cap 10/player**; **UMP consent + iOS ATT in v1**;
AdMob **test** ad-unit ids in dev (real ids via config, no code change).
Interstitials are pure frontend. 7 slices (AD1 → AD7) locked in
`ROADMAP.md > Sprint AD`.

**AD1 ✅ done (uncommitted on `main`, 2026-05-30):** Ads bounded context
scaffolded (Domain/Application/Infrastructure/IntegrationEvents/Tests/
IntegrationTests). `RewardedAdGrant` append-only ledger aggregate
(idempotency key = AdMob SSV `TransactionId`, unique index), rules
(amount positive, transactionId non-empty, `RewardedAdDailyLimitRule`),
domain event, repository (`GetByTransactionIdAsync`,
`CountForPlayerSinceAsync`, `AddAsync`), EF mapping, `AdsContext`. DbUp
`ads` schema + `RewardedAdGrants` + outbox applied locally (journal 82).
`RewardedAdRewardedIntegrationEvent` project created (published in AD2).
API host boots `AdsStartup`; sln/test.sh/ArchTests registered. Gate:
full build 0 errors, Ads unit 4/4, Ads integration smoke 1/1, ArchTests
64/64. Admin-audit/cross-module/consumer plumbing deliberately stripped
(Ads has none in v1).

**AD2 ✅ done (uncommitted on `main`, 2026-05-31):** SSV verify + grant
backend. `IAdMobSsvVerifier` contract + request/result models
(`Ads.Application/Configuration/Verification`); fail-closed
`AdMobSsvVerifier` shell (rejects until real key verification is wired)
and fail-open `DevelopmentAdMobSsvVerifier` (dev only — Google can't
reach localhost). Host selects the verifier from `Ads:Ssv:Mode`
(`Production` default → fail-closed; `DevelopmentFailOpen` set in
`appsettings.Development.json`), registered as a singleton in `Program.cs`
mirroring the auth external-verifier seam. `GrantRewardedAdRewardCommand`
+ handler: **idempotency (txid) → verify signature → parse `user_id` →
daily cap → `IDiamondGrant.GrantAsync` → `RewardedAdGrant.Create` ledger
row**; outcome DTO (`Granted`/`AlreadyGranted`/`DailyLimitReached`/
`VerificationFailed`) — cap and verification-fail return gracefully (no
throw), so the SSV endpoint always answers 200. `RewardedAdGrantedDomainEvent`
→ outbox notification + publisher → `RewardedAdRewardedIntegrationEvent`
(BI/audit). `GetRewardedAdStatusQuery` (Dapper count of today's grants).
`GET /ads/rewarded/callback` (anonymous, signature-verified inside) +
`GET /ads/rewarded/status` (player). `IAdsConfigurationService`
(`Ads:RewardedDiamondAmount` 5 / `Ads:RewardedDailyLimit` 10).
`Ads.Application → Diamond.Application` project ref added (only
`Configuration.CrossModule.IDiamondGrant` used); `Ads.Infrastructure →
Ads.IntegrationEvents`. Granular ArchTest allow added (3 Ads ModuleLayer
cases). Gate: full build 0 errors, Ads unit **8/8**, ArchTests **67/67**,
API health host-boot **2/2**.

**Design notes (AD2):** (1) Idempotency is checked *before* signature
verification — a replayed callback for an already-granted txid
short-circuits to `AlreadyGranted` without re-verifying or re-granting.
(2) Diamond-grant failure (e.g. player has no Diamond inventory) is left
to **throw** → 500 → AdMob retries the callback; v1 has no
`VerifiedButGrantFailed` recovery state (AD1 stripped recovery plumbing),
and the txid idempotency + AdMob's ~day-long retry window cover it. (3)
Daily cap reuses `RewardedAdDailyLimitRule` but the handler evaluates
`IsBroken()` and returns gracefully rather than throwing — hitting the
cap is a benign "no reward", not an error.

**AD3 ✅ done (uncommitted on `main`, 2026-05-31):** frontend ads infra.
`google_mobile_ads ^5.1.0` dep (resolved 5.3.1). Global `AdsService`
(`lib/shared/ads/`) over an `AdsPlatform` seam — `MobileAdsPlatform` is
the only file importing the SDK; `AdsService` itself is SDK-free and
unit-testable. Mobile-only/web-safe: `isSupported` false on web/desktop,
`initialize()` a no-op there and best-effort (swallows failures) so app
start never depends on the ad network. `AdConfig` holds Google **test**
ad-unit ids, overridable via `--dart-define` (real ids, no code change).
AdMob **test** app ids in `AndroidManifest.xml`
(`com.google.android.gms.ads.APPLICATION_ID`) + iOS `Info.plist`
(`GADApplicationIdentifier`). Wired in `main.dart` (`unawaited`
fire-and-forget init) + provided above the router in `app.dart`
(`RepositoryProvider<AdsService>`), mirroring `AudioService`. Gate:
`flutter analyze` clean on ads files (12 pre-existing info warnings
unchanged), Flutter **142/142** (+5 ads/config smoke via the injectable
platform seam). Interstitial/rewarded **show** logic deliberately
deferred to AD4/AD5 — AD3 is infra only (no speculative show API).

**AD4 ✅ done (uncommitted on `main`, 2026-05-31):** interstitial
placements. Added `AdsPlatform.showInterstitial(adUnitId)` (load+show,
fire-and-forget, self-disposing via `FullScreenContentCallback`) +
`AdsService.maybeShowInterstitial(probability)` — a probability gate over
an injectable `Random` that no-ops when unsupported, not initialized, or
the roll misses. `InterstitialChance` constants (`gameStart` 1/3,
`gameEnd` 1/2) in `ad_config.dart`. Hooked at **GameStart success** (home
`BlocListener`, before `context.go('/games/...')`) and **game finish**
(game-screen finish `BlocListener`, alongside the result sheet). Both
calls are best-effort/web-safe and never block navigation or the result
sheet. Gate: `flutter analyze` clean on touched files (12 pre-existing
info warnings unchanged), Flutter **147/147** (+5 — probability gate
hit/miss, not-initialized/unsupported no-op, `InterstitialChance`
sanity). No home/game widget tests exercise the new provider read, so no
test-provider plumbing was needed.

**AD5 ✅ done (uncommitted on `main`, 2026-05-31):** rewarded ad →
Diamond. `AdsPlatform.showRewarded({adUnitId, userId, onClosed})` +
`AdsService.showRewarded({userId, onClosed})` — loads/shows a `RewardedAd`,
tags the request with the player id as the SSV `user_id`
(`ServerSideVerificationOptions(userId: ...)`), and the local
`onUserEarnedReward` is intentionally a **no-op** (grant is backend-owned
via the verified SSV callback). `onClosed` is guaranteed exactly once
(dismiss / fail-to-show / fail-to-load / unsupported / not-initialized)
so the UI never hangs. New `features/rewarded_ads/`:
`RewardedAdStatus` model + `RewardedAdRepository` (`GET /ads/rewarded/status`)
+ `RewardedAdCubit` (load → ready/unavailable/failure; `watch()` guards on
cap, shows the ad, then re-fetches status and flags `rewardJustWatched`)
+ `EarnDiamondsScreen` (watch button, `today X/limit • N left` display,
disabled-when-capped, web/desktop unavailable state). Player id read from
`TokenStore.readPlayerId()`. On `rewardJustWatched` the screen refreshes
`DiamondCubit` + plays the purchase SFX + shows a "diamonds arrive once
verified" snackbar. Route `/earn-diamonds` + HomeScreen side-icon entry
(`ondemand_video_outlined`, "Earn Diamonds"). Gate: `flutter analyze`
clean on ads/feature files (12 pre-existing info warnings unchanged),
Flutter **154/154** (+7 — showRewarded forward/recover/no-op, cubit
load/watch/cap). **Note:** in local dev Google's SSV servers can't reach
localhost, so the grant won't auto-land after watching — the operator
hits the callback manually (AD7); the UI refresh path is wired and tested.

**AD6 ✅ done (uncommitted on `main`, 2026-05-31):** consent + ATT. Added
`app_tracking_transparency ^2.0.6` (resolved 2.0.7). New
`AdsPlatform.gatherConsent()` seam method; `MobileAdsPlatform` runs the
**iOS ATT** prompt (only when `TrackingStatus.notDetermined`) then the
**AdMob UMP** consent flow (`ConsentInformation.requestConsentInfoUpdate`
→ `ConsentForm.loadAndShowConsentFormIfRequired`, wrapped in a `Completer`),
both best-effort. `AdsService.initialize()` now calls `gatherConsent()`
**before** `_platform.initialize()`, so consent precedes any ad request.
iOS `Info.plist` gains `NSUserTrackingUsageDescription` (required or ATT
crashes). UMP ships in `google_mobile_ads` (already a dep); no UMP code
in `AdsService` itself (stays SDK-free behind the seam). Gate:
`flutter analyze` clean on ads files (12 pre-existing info warnings
unchanged), Flutter **156/156** (+2 — consent-before-init ordering,
no-consent-when-unsupported). Mobile-only; web/desktop skip consent
(unsupported → no-op).

**AD7 ✅ done (uncommitted on `main`, 2026-05-31):** sprint close-out.
Gates re-run: DbUp **0 pending**; all module unit suites + **ArchTests
67/67** green (incl. **Ads unit 8/8**); all integration suites green
(incl. **Ads integration 1/1**); Flutter **156/156**, `flutter analyze`
clean on ads files (12 pre-existing info warnings). Docs closed out:
`ROADMAP.md` Sprint AD marked ✅, `GLOSSARY.md` Ads ubiquitous language
(`RewardedAdGrant`, rules, `IAdMobSsvVerifier`,
`RewardedAdRewardedIntegrationEvent`), `OPERATIONS.md` Ads settings +
SSV/endpoint notes, `progress.md`/frontend docs.

**Known pre-existing gate condition (not AD):** `LexiLink.API.Tests` has
15 failures under the local `Authentication:Mode=ProductionJwt` dev
config — those tests send a raw-GUID bearer that only `DevelopmentBearer`
accepts (verified: they pass with `Authentication__Mode=DevelopmentBearer`).
The dev mode was switched to ProductionJwt during the Administration
sprint; no AD code touches auth. **Operator-owned remaining:** manual
device verification with AdMob test ads + real AdMob/SSV credentials
(see `progress.md > AD7`).

No further AD repo slice is queued; Sprint AD is closed. Current active
direction is Sprint L10N above.

### Previous Sprint Context

**Sprint A — Audio (Sound & Music) — closed 2026-05-30.** A1–A7
delivered. Frontend-only: SFX + background music via `audioplayers`,
with music/SFX on-off + volume kept device-local (SharedPreferences)
behind `AudioPreferencesRepository` (sync-ready, no backend in v1).
`AudioService` is one global instance (init in `main.dart`, provided
above `MaterialApp.router`); `AudioMusicOrchestrator` drives
route-based track switching + lifecycle pause/resume + web
first-gesture autoplay; SFX wired at gameplay
(`soundEffectForGameTransition`), home, quest/market/payment cues.
Gate: Flutter **137/137**, analyze adds no Audio findings. Operator
verified on Chrome web against the live API. **Bug fixed in close-out:**
audioplayers 6.7.0 web throws `UnsupportedError` on the 2nd+ play of an
asset (`dart:io` cache recheck) — best-effort catches widened to
`on Object` and web evicts the cache entry before each play
(`AudioCache.clear`); no-op on native. **Remaining (intentional, not a
slice):** shipped sounds are **placeholder tones** — real audio drops
into `frontend/assets/audio/` under the same filenames with no code
change (manifest in `frontend/assets/audio/README.md`). Full slice
table in `ROADMAP.md > Sprint A`.

### Previous Sprint Context

**Sprint P — Payments / In-App Purchase — closed for repo-deliverable
work; P1-P8 delivered.** 8 slices (P1 → P8) locked in
`ROADMAP.md > Sprint P`. Shipped the real-money purchase path for
iOS and Android: players buy **Diamond bundles** through Apple App
Store / Google Play in-app purchase, backend verifies the store
transaction server-side, records an append-mostly payment ledger, and
grants Diamond exactly once. This is intentionally separate from
Market: **Market spends Diamond** on Energy/Hint/Undo/Reset;
**Payments earns Diamond** from platform commerce.

**Why this sprint is high-risk:** real money is involved. Client data
is never trusted for amount, price, or entitlement. Apple/Google store
proof must be verified by the backend before any Diamond grant.
Idempotency, recovery, audit/support visibility, and refund/revocation
reconciliation are mandatory parts of the feature, not polish.

**Locked decisions (excerpt; full table in ROADMAP):**

- **New bounded context: Payments.** Schema `payments`; separate from
  Market and Diamond. Payments owns platform transaction state,
  receipt/purchase-token verification, notification reconciliation,
  and support-facing payment history.
- **v1 product type: consumable Diamond bundles only.** Proposed
  store product ids: `diamond_100`, `diamond_550`, `diamond_1200`,
  `diamond_2500`. No subscription, battle pass, no-ads, or paid
  unlocks in v1.
- **Backend is verification authority.** iOS/Android clients only
  initiate platform purchase and submit store proof. Backend resolves
  ProductId → DiamondAmount from `PaymentProduct`; localized price is
  storefront-owned display data.
- **Diamond grant after verification only.** Payments calls
  `IDiamondGrant.GrantAsync(playerId, amount)` after Apple/Google
  verification and idempotency checks pass.
- **Idempotency at store proof level.** Unique Apple transaction id
  and Google purchase token indexes prevent double grant. Optional
  `(PlayerId, ClientRequestId)` gives friendly replay responses.
- **Recoverable delivery.** Store verification success + Diamond grant
  failure must persist as a retryable state (for example
  `VerifiedButGrantFailed`); a paid purchase cannot vanish because a
  downstream module was temporarily unavailable.
- **Refund/revocation v1 policy.** Mark payments refunded/revoked and
  emit support/audit signal; do not automatically create negative
  Diamond until product policy is explicit.

**Final delivery note:** P1 foundation, P2 product catalog, P3
platform verifier contracts, P4 verify+grant, P5 post-processing
+ recovery, P6 notifications/reconciliation, and P7 frontend purchase
UI are implemented. P8 closed the local/backend/frontend gates and
docs. P3 added
`IAppleIapVerifier`, `IGooglePlayIapVerifier`,
`IGooglePlayPurchaseProcessor`, store verification request/result
models, Apple/Google options (`Payments:Apple`, `Payments:Google`),
test fake verifiers/processors, and fail-closed infrastructure adapter
shells. P4 added `VerifyIapPurchaseCommand`,
`POST /payments/iap/verify`, catalog/platform/account-binding
validation, replay-safe store proof idempotency, `IDiamondGrant`
delivery, `IapPurchaseGrantedIntegrationEvent`, and recoverable
`VerifiedButGrantFailed` responses when Diamond grant fails. Real App
Store / Play Developer API calls are still deferred behind fail-closed
adapter shells. P5 added ledger fields for post-processing action/status,
iOS `CanFinishTransaction` response semantics, backend-owned Google
acknowledge/consume invocation after grant, post-processing failure
tracking, `RetryIapPurchaseDeliveryCommand`, and
`POST /admin/payments/purchases/{id}/retry-delivery` for stuck delivery
or post-processing retries. Verification: Payments unit tests 18/18,
Payments integration smoke 1/1, ArchitectureTests 64/64. P6 added
Apple/Google notification endpoints, verifier contracts + fail-closed
infrastructure shells, raw `PaymentNotification` persistence before
processing, idempotent notification replay, refund/revocation/failure
status transitions, and `IapPurchaseStatusChangedIntegrationEvent`
support/audit signal. Real App Store Server Notifications V2 and
Google RTDN cryptographic verification remain behind the fail-closed
shells until production credentials/SDK integration are configured.
Verification: Payments unit tests 20/20, Payments integration smoke
1/1, ArchitectureTests 64/64. P7 added the Flutter `in_app_purchase`
dependency, `features/payments/` data/store/application/presentation
layers, `/payments` route, HomeScreen Diamonds shortcut, mobile-only
purchase controls, web-safe unavailable state, backend verify call,
Diamond badge refresh on granted delivery, and transaction finish via
backend `CanFinishTransaction`. Verification: payments Flutter tests
6/6, full Flutter suite 113/113. `flutter analyze` has no
Payments-specific findings; it still reports 12 pre-existing
info-level frontend warnings. P8 verification: Payments unit tests
20/20, Payments integration smoke 1/1, ArchitectureTests 64/64,
Flutter tests 113/113, DbUp migrator re-run 0 pending scripts, local
API readiness healthy with 79/79 DbUp scripts applied, and JWT-mode
guest/category smoke passed. Apple sandbox purchase, Google internal
test purchase, real store notification cryptographic verification,
and native app-kill recovery remain **operator-owned credential/store
setup checks** before production because this workspace does not carry
App Store / Play Console credentials or native signing/test tracks.

**Next action:** operator manual store verification when Apple/Google
credentials and test products are available. No additional repo slice
is queued from Sprint P.

**Slice cadence:** foundation → product catalog → platform verifier
contracts → verify+grant command → post-processing/recovery →
notifications/reconciliation → frontend purchase UI → tests/manual
verification/docs close-out. Full slice table + architecture notes +
deliberate non-actions live in `ROADMAP.md > Sprint P`.

### Earlier Sprint Context

**Sprint M — Market Module — closed 2026-05-27, commit `5c0f4e6`.**
Delivered the Diamond-spend bounded context where players use Diamond
to buy Energy / Hint / Undo / Reset inventory top-ups. M1-M8 added
the 3 Market aggregates (`Category`, `ShopItem`, `PurchaseOrder`),
6 sync gateway contracts (`IDiamondGuard`, `IDiamondGrant`,
`IEnergyGrant`, `IHintGrant`, `IUndoGrant`, `IResetGrant`), buy
saga orchestration with compensating Diamond refund, admin CRUD/audit,
player/admin GET endpoints, player Market frontend, admin Market
console, M7 manual verification, and M8 docs close-out. Final gate:
Market unit tests 6/6, Market integration smoke 1/1,
ArchitectureTests 61/61, Flutter tests 107/107. `flutter analyze`
had no Market-specific findings; only pre-existing info-level
frontend warnings remained.

**Sprint D — Diamond Module + Quest 5-Reward — closed
2026-05-27, commit `30b3971` (single Sprint D commit covering
D1-D8 + the hint algorithm reshape found during D7 manual
verification).** Delivered the 5th inventory module — Diamond,
the in-game currency Sprint M will spend against. Mechanically
mirrors the Hint/Undo/Reset template (lazy init from
`PlayerRegisteredIntegrationEvent`, per-module Autofac module,
outbox, decorator chain) but **without a sync gateway** — Sprint
D explicitly deferred sync spend to a later sprint. Sprint M is
that sprint and introduces `IDiamondGuard` + `IDiamondGrant` as
Diamond's first cross-module sync contracts.

The same commit folded in two gameplay fixes triggered by D7
testing:

- **`StandardGameConfigurationService.ResolveHints() == 0`** —
  per-game free hint allowance dropped to zero. Every hint now
  routes through `IHintGuard` and the player's inventory; the
  legacy `HasFreeHintRemaining` branch in `UseHintCommandHandler`
  is kept for tests but the production path always falls through
  to the gateway.
- **`Puzzle.RequestHint` rewritten with live BFS over
  `LinkNeighborResolver`.** Previous implementation returned
  `_optimalPath[0]` for off-path positions, which gave a useless
  hint pointing back to start. New flow does a fresh BFS from
  `currentLinkId` to `TargetLinkId`; the **first hop** of that
  path is the hint. `LinkNeighborResolver` returns outgoing link
  ids **sorted** so BFS is deterministic and matches
  `OptionsHandler`'s 6-option panel ordering.

Quality gate at close: **502 .NET tests + 107 Flutter tests
green**. Local DbUp migration
`quests/060_ExpandQuestRewardsWithDiamond.sql` applied.
Detailed Sprint D slice notes, architecture rationale, and
deliberate non-actions remain in `ROADMAP.md > Sprint D` and
`progress.md`.

Older sprint context (Sprint UR — Undo + Reset Modules, Sprint H
— Hint module, Sprint Q1 — Quests redesign, etc.) lives in
`progress.md`.

### Recently closed (Sprint UR session, 2026-05-26)

- **Game.cs destructive reshape (UR1).** `UndoAllowance` +
  `ResetAllowance` VO/rule/test surface removed. `Game` keeps only
  `_undosUsed` + `_resetsUsed` counters; command handlers call
  `IUndoGuard` / `IResetGuard` before domain mutation. API and test
  composition roots used temporary no-op guard adapters before UR4.
- **Undo + Reset module foundation (UR2).** Added
  `Modules/Undo/` and `Modules/Reset/` with Domain/Application/
  Infrastructure/Tests/IntegrationTests projects, inventory aggregate
  foundations, rules/events/repositories, EF mappings, schema +
  outbox DbUp scripts, API startup wiring, solution registration, and
  ArchitectureTest coverage. Unit tests: 19 Undo + 19 Reset.
- **Lazy init from PlayerRegistered (UR3).** Added
  `EnsurePlayerUndoInventoryExistsCommand` +
  `EnsurePlayerResetInventoryExistsCommand`, idempotent handlers,
  validators, `I{Undo,Reset}ConfigurationService.InitialBalance`
  contracts + infrastructure config services, and
  `PlayerRegisteredIntegrationEventHandler` consumers in both modules.
  Undo/Reset Application now has a granular public-contract dependency
  on `Players.IntegrationEvents`. Integration tests prove outbox
  registration creates inventory rows and replayed events do not
  duplicate them.
- **Sync gateway integration (UR4).** Added
  `ConsumePlayerUndoCommand` + `ConsumePlayerResetCommand`, real API
  `UndoGuard` / `ResetGuard` adapters, Games.IT
  `RecordingUndoGuard` / `RecordingResetGuard`, and fall-through
  tests proving every in-game Undo/Reset call invokes the gateway.
  Undo/Reset integration tests now cover consume success and empty
  balance rejection. No DbUp migration was needed.
- **Quest 4-reward destructive (UR5).** `QuestDefinition` gains
  `_undoReward + _resetReward`; `Create` / `Update` 4-arg;
  `QuestRewardMustHaveAtLeastOnePositiveRule` widens.
  `PlayerQuestClaimedDomainEvent` + `QuestClaimedIntegrationEvent`
  4 reward fields. 2 new outbox consumers
  (Undo.Application + Reset.Application), each guarding on its
  reward > 0. DbUp `quests/050_ExpandQuestRewardsWithUndoReset.sql`
  idempotent ALTER ADD COLUMN. Admin Quest commands / endpoints /
  EF mapping reshape.
- **Admin operations + GET endpoints + audit (UR6).** 6 new admin
  commands (Set / GrantBonus / Reset per module) + 2 new player
  queries; 7th + 8th per-module `AdminAuditingCommandHandler` +
  audit notification + handler. API endpoints:
  `GET /{undo,reset}/me` and `/admin/players/{id}/{undo,reset}/*`.
- **Frontend reshape (UR7).** 2 new player features
  (`features/undo/` + `features/reset/`) + 2 new admin consoles;
  HomeScreen 4-badge row; admin quest form 4 reward inputs;
  player quest tile `Wrap` for 4 badges. 5 test fixtures
  reshape.
- **Admin command IT + docs (UR8).** `UndoAdminCommandTests` +
  `ResetAdminCommandTests` mirror Hint H7's
  `HintAdminCommandTests`. Undo.IT + Reset.IT TestBase
  extended with AdministrationStartup so audit rows land in
  the test DB. Docs polish.

### Recently closed (Sprint H session, 2026-05-25 → 2026-05-26)

- **Hint module foundation (H1).** Full Kamil-style module
  scaffolding (Domain / Application / Infrastructure / Tests /
  IntegrationTests + Autofac module + Startup + UoW + decorators
  + outbox + EF mapping). Aggregate has no max cap and no refill
  timer — hints are earned, not regenerated.
- **Lazy init from PlayerRegistered (H2).** Mirrors Energy's
  pattern: `Hint.Application` consumes
  `PlayerRegisteredIntegrationEvent` and dispatches
  `EnsurePlayerHintInventoryExistsCommand` (idempotent).
  `Hint:InitialBalance` config (default 0).
- **IHintGuard sync gateway (H3).** Contract in Games.Application;
  adapter in API host. `Game.HasFreeHintRemaining` +
  `UseHintWithExternalInventory()` route the call.
  `ResolveHints(difficulty)` flattened to 1 — the per-game free
  quota no longer scales with difficulty.
- **Quest multi-reward (H4, destructive).**
  `QuestDefinition._energyReward` + `_hintReward`,
  `QuestRewardMustHaveAtLeastOnePositiveRule`,
  `PlayerQuest.Claim(now, ready, energyReward, hintReward)`,
  `QuestClaimedIntegrationEvent` carries both fields. DbUp
  `040_ReshapeQuestRewardsForSprintH.sql` is idempotent (column
  RENAME + ADD COLUMN with information_schema guards).
- **Hint admin console + audit (H5).** `AdminSet` / `AdminReset`
  domain methods. Three `IAdminCommand` commands (Set / GrantBonus
  / Reset) wired through a per-module
  `AdminAuditingCommandHandlerDecorator` (5th copy of the
  template). `GET /hint/me` (player) + admin GET/POST endpoints.
- **Flutter reshape (H6).** New `hint/` and `admin_hint/` features.
  HintBadge in HomeScreen next to EnergyBadge. Admin quest form
  collects two reward inputs with form-level at-least-one rule.
  Player quest tile renders both badges side-by-side when
  positive. 5 test files reshaped; 103/103 Flutter pass.
- **Tests + quality gate (H7).** 19 Hint domain + 8 Hint
  integration + 3 Games.IT UseHint fall-through scenarios via the
  new `RecordingHintGuard` (configurable + call-counted).
  `scripts/test.sh` registers Hint.Tests and Hint.IntegrationTests.
- **Manual verification (H8).** Operator restarted the stack,
  created an Energy+Hint reward quest, completed games as a test
  player, verified both inventories update after claim, exercised
  the Game UseHint free→inventory→empty flow end-to-end. All
  passed.

### Lessons captured this session (Sprint H, 2026-05-25 → 2026-05-26)

- **HintsUsed is legacy/free-quota count, not inventory usage.**
  Sprint D sets `ResolveHints() == 0`, so every hint goes through
  `IHintGuard` and `GameDetailsDto.HintsUsed` stays 0 for normal
  inventory-backed hints. Codified in `UseHintFallThroughTests`.
- **Reverse event dependency justifies a granular ArchTest
  allow.** Hint.Application needs `Quests.IntegrationEvents` for
  its consumer; Hint.Infrastructure needs
  `Administration.IntegrationEvents` for the audit notification.
  The Architecture test allowlist accepts these two pairings
  explicitly — the module's Domain remains forbidden from any
  cross-module namespace.
- **Two consumers > one fat consumer.** A single multi-reward
  consumer would couple Energy and Hint to each other. Two
  independent consumers each guard on their reward's positivity
  and degrade independently: a Hint outage doesn't block energy
  delivery and vice versa.
- **`GrantBonus` permits over-cap balance — twice.** Both
  `PlayerEnergy.GrantBonus` and `PlayerHintInventory.GrantBonus`
  intentionally bypass max checks. The semantics are "reward
  earned, not regenerated" — capping the bonus would silently
  swallow the operator's intent. Documented in the aggregates.
- **iCloud duplicate `.sql` files break DbUp.** Embedded SQL
  resources accumulated `* 2.sql` copies in `bin/Debug/` from
  iCloud sync. DbUp health check listed them as
  "missing scripts" and ready probe failed. `find . -name "* 2.sql"
  -delete` cleans up; commit happened to be unrelated, so no
  source change was needed.

---

## Last sprint closure (kept for context)

## Sprint Q1 — Quests Module Redesign — closed (2026-05-24)

Backend slices Q1.1 → Q1.5 + tests Q1.7 + frontend slice Q1.6 +
manual verification Q1.8 all delivered. Closed with **361 .NET +
103 Flutter tests green**. (Sprint H later raised the Flutter
count via test reshape and lowered the .NET count by 13 because
the multi-reward shape collapses some narrower
`QuestRewardMustBePositive` cases into broader
`QuestRewardMustHaveAtLeastOnePositive` cases — net 348 backend
tests at H7 close.)

Goal (delivered): replaced the fixed-enum `QuestType` catalog with a
fully data-driven `QuestDefinition` shape carrying `Name`,
`Description`, `Trigger`, `Threshold`, `Reward`,
`PrerequisiteQuestDefinitionId`, `ProgressBaseline`. Players see new
admin-defined quests **lazily**: the next `GET /quests/me` call
(splash sync or quest page open) deletes expired daily rows, inserts
missing eligible PlayerQuests with baseline snapshots, then projects
progress + DisplayState from Stats counters in memory. PlayerQuest
progress is **computed** from Stats counters at read time, never
written. Eager broadcast + `PlayerRegisteredIntegrationEventHandler`
+ the hardcoded `GameCompletedIntegrationEventHandler` /
`AuthProviderLinkedIntegrationEventHandler` were deleted in Q1.3.

Why: manual testing of the closed admin frontend (F1–F6) exposed
three product gaps — closed catalog (enum-fixed), hardcoded behavior
(handlers know only 4 quest types), and eager broadcast cost (B15
auto-issuance scales poorly with player count). Operator preference
(2026-05-23) for "lazy, pull-based" issuance triggered on splash + quest
page open is the architectural commitment.

Schema migration was **destructive** (dropped `QuestType` column from
PlayerQuests + QuestDefinitions). No production data existed, so no
migration path was needed; existing local test PlayerQuests rows
were dropped on the Q1.2 migration. The destructive script
(`030_ReshapeQuestsForSprintQ1.sql`) is idempotent so re-running
`scripts/smoke.sh` / DbUp on a fresh DB stays a no-op after first
apply.

### Recently closed (Sprint Q1 session, 2026-05-24)

- **Backend Q1.1–Q1.5 + Q1.7 — Quests redesign shipped.** All six
  per-slice commits land on `main`; 361/361 .NET tests green. See
  `progress.md > Sprint Q1` for slice-level detail.
- **Frontend Q1.6 — Flutter reshape shipped.** Admin form pivoted
  to free-text Name + Description + QuestTrigger dropdown +
  Threshold/Reward inputs + ProgressBaseline (visible only when
  trigger is GameCompletedTotal) + Prereq picker populated from
  other active definitions. Player tile renders the admin-defined
  Name + Description; the hardcoded QuestType → human-copy mapping
  is gone. 103/103 Flutter tests green.
- **Q1.3 bug found + fixed.** `GetActiveQuestsQueryHandler`
  originally Sync→Delete; expired Daily rows looked "existing" to
  Sync (no insert) and then got deleted, leaving the player with
  zero rows. Order swapped (Delete first, then Sync). Comment
  added.
- **Energy consumer ripple.** `QuestClaimedIntegrationEvent` now
  carries `QuestDefinitionId` + `Reward` (was `QuestType` +
  `RewardAmount`); `Energy.Application/
  QuestClaimedIntegrationEventHandler` reads the new field. Bonus
  delivery stays event-driven.
- **IntegrationTests stub `MutableQuestCounterReader`.** Quests.IT
  doesn't boot Stats/Players; the stub satisfies
  `IQuestCounterReader` resolution and lets each test set
  counters explicitly. Production reader (`API/CrossModule/
  QuestCounterReader`) hits stats.PlayerStats +
  stats.PlayerPeriodStats + players.PlayerAuthIdentities directly.

### Recently closed (Administration session, 2026-05-22…23)

- **Backend B11 — admin energy GET.** `GET /admin/players/{playerId}/energy`
  passthrough reusing `GetPlayerEnergyQuery` under `AuthenticatedAdmin`.
  Unblocks the admin energy console UI.
- **Backend B12 — Reactivate quest definition.** Mirror of Deactivate;
  `POST /admin/quests/definitions/{id}/reactivate`. Domain method already
  existed. Will be preserved in Q1 — Reactivate stays meaningful in the
  new model.
- **Backend B15 — Quests listens to `PlayerRegisteredIntegrationEvent`.**
  New guest player gets all active QuestDefinitions issued (idempotent,
  prereq-respecting). **Will be deleted in Q1.3** — lazy issuance
  replaces eager-on-register issuance.
- **Backend Npgsql fix — `EnableLegacyTimestampBehavior=true`.** Was
  silently converting `DateTime.UtcNow` → local-shifted value when
  writing to `timestamp without time zone` columns, then reading back +
  re-tagging as UTC. Net effect: 7-hour shift on Mac dev box → energy
  refill projection returned a fully-refilled bucket. AppContext switch
  in `Program.cs` line 1 is the cleanest fix; columns stay as-is.
- **Backend Player /quests/me filter.** Join with `QuestDefinitions` +
  `WHERE IsActive=TRUE`; deactivated definitions hide their PlayerQuests
  immediately from the player view without deleting the rows (claim
  history intact). Reactivate brings them back.
- **Backend QuestType.Custom1/2/3 placeholder slots.** Added to the
  enum to let the admin Create flow be exercised against types that
  don't already have a definition. Behaviorally inert (no event handler
  triggers issuance/progress). **Will be deleted with the enum itself
  in Q1.1.**
- **Backend ProductionJwt dev preset.** API now runs in
  `Authentication__Mode=ProductionJwt` with a 32+ char dev signing key
  so admin JWTs roundtrip correctly (`DevelopmentBearer` only accepts
  raw GUIDs, which broke admin endpoint auth after F1 admin login
  started returning a real JWT).
- **Admin frontend F1–F6 closed** (2026-05-22…23). See
  `frontendActiveContext.md > Admin frontend sprint (F1–F6)` for slice
  shape detail.
- **Frontend player guest flow now exchanges a JWT.** Pre-existing
  anti-pattern (store `playerId` as the bearer) worked only in
  `DevelopmentBearer` mode and broke under `ProductionJwt`.
  `GuestPlayerRepository.registerGuest` now also calls `POST /auth/token`
  (provider=Guest, externalToken=`dev:Guest:{deviceId}`) and stores the
  returned JWT. `TokenStore` extended with `readPlayerId` /
  `savePlayerId` so cubits that need the player's identity stop reading
  it from the access token field.
- **Frontend Flutter web path strategy.** `usePathUrlStrategy()` in
  `lib/main.dart` so address-bar `/admin/login` resolves directly
  instead of falling into the splash redirect. `flutter_web_plugins`
  added to pubspec.
- **Frontend admin dialogs use `useRootNavigator: false`.** go_router
  16 + `ShellRoute` + default `showDialog` trips a "popped last page"
  assertion that blanks the shell. Every admin showDialog call now
  bypasses the root navigator.
- **Frontend admin energy card stays visible during saving.** Earlier
  the whole card was replaced by a spinner; the new value flip was
  invisible to the operator. Now the card stays mounted with a dimmed
  overlay + small spinner during saving.

### Lessons captured this session (intentional patterns)

- **Npgsql 6+ `timestamp without time zone` requires `Kind=Local` /
  `Unspecified`.** Passing `Kind=Utc` silently shifts. We chose the
  AppContext legacy switch over a `timestamptz` column-level migration
  because the legacy behavior is correct for our UTC-everywhere code
  and column migration adds churn without architectural benefit.
- **go_router + `showDialog` in a ShellRoute requires
  `useRootNavigator: false`.** Default makes Navigator.pop bubble up
  to the delegate and trip the "popped last page" assertion. Every
  admin dialog needs the explicit override.
- **Player token store separation.** Treating the bearer token as a
  player ID is a category error. `TokenStore` now persists both
  values; cubits use the dedicated `readPlayerId` API.
- **Quest catalog limitations exposed.** The current enum-based
  catalog is unfit for the product: admin can't introduce new quests
  without a code+seed change, and seeded types are all "taken" so
  Create is functionally inert. Q1 redesign is the response.

### Lessons captured this session (Sprint Q1, 2026-05-24)

- **Two-pass `GET /quests/me` order matters.** Delete expired daily
  rows BEFORE the sync pass — otherwise an expired row counts as
  "already issued" and the slot is empty until next sync. Cheap fix,
  expensive to debug. Comment in the handler explains the why.
- **Query handlers can mutate.** The Kamil convention says no, but
  Q1's lazy-sync semantics need it: a separate sync-then-query call
  pattern is more chatty and races between the two calls would
  leave a window where the player sees no quests. The mutation is
  pure SQL with `ON CONFLICT DO NOTHING`; no UoW needed because the
  query handler is outside the decorator stack.
- **Reward flows through the domain event, not the aggregate.**
  PlayerQuest doesn't know its definition's reward; the handler
  reads it from the catalog and passes it into `Claim`. The
  resulting `PlayerQuestClaimedDomainEvent` carries `Reward` so
  Energy's outbox consumer stays event-driven and doesn't need a
  cross-module gateway back to Quests.
- **`http.Response` in Flutter tests defaults to Latin-1.** Test
  fixtures using `'İ'` (U+0130) blow up with
  `Invalid argument: Contains invalid characters`. Easy fix: keep
  test JSON ASCII (`'First Game'`); production payloads stay UTF-8
  via the framework's content-type negotiation. Codified in the
  Q1.6 test files.
- **Cross-module reads don't need a module facade.** `EnergyGuard`
  and `AdminLookup` use module facades because they call commands;
  `QuestCounterReader` reads three counters across two modules and
  doesn't fit that shape. Going straight to SQL from the API host
  composition root keeps Quests structurally isolated and is faster
  than wrapping each read in a Query+QueryHandler+IModuleFacade
  bounce. Granted only because the read targets are stable
  projection tables.

---

## Last sprint closure (kept for context)

**Administration Module** — sixth backend module. Sprint plan locked
in `ROADMAP.md > Administration Module`. All ten slices (B1–B10)
shipped 2026-05-21.

Why now: an admin frontend is on the backlog (quest catalog CRUD,
per-player energy edits, content management). A real permission model
finally has a concrete need, so the previous *non-action: broad
permission/UserAccess module* rule is lifted — but bounded. The new
module ships a single `Admin` role, no permission matrix, no
UserAccess-style ceremony.

Kamil discipline carried over: strict module isolation, separate
schema, per-module Application contracts, sync gateway only for the
authorization check (`IAdminAuthorizationContext`), reverse-direction
integration events for the audit trail
(`AdminActionPerformedIntegrationEvent`). Microservice extraction
remains a first-class design constraint.

The previously closed **Production Readiness Pass** baseline is intact;
its closure summary is preserved below for historical reference.

Kamil Architecture Alignment Pass tamamlandı. Ardından public/real usage'a
yaklaşmak için auth, product-facing Stats derinliği, API contract hardening,
operational readiness, database hygiene ve release smoke gate baseline'ları
kapandı.

Tamamlanan Kamil alignment slice'ları:

1. ArchTests baseline.
2. API endpoint'lerinin module facade kullanması.
3. API composition root'un module startup API'larına yaslanması.
4. Games içinde completed start-target pair tekrarını engelleme.
5. Games/Players IntegrationEvents + Outbox -> Stats projection akışı.
6. Stats read surface ve shared-container composition hardening.
7. `Directory.Build.props`, `Directory.Packages.props`, application convention
   ArchTests.
8. GitHub Actions CI quality gate: restore, build, DbUp migrations,
   `scripts/test.sh`.
9. Auth/authorization baseline: `LexiLinkBearer` scheme, authenticated-player
   policy, protected endpoint groups, API auth smoke tests.
10. Outbox scheduling hardening: Quartz hosted scheduler, retry metadata
    columns, persisted errors, delayed retry eligibility, partial-failure
    integration test.
11. Raw Stats Inbox pattern: consuming integration handlers append serialized
    messages first; scheduled processor projects, retries failures, and keeps
    duplicate event ids idempotent.
12. Stats Internal Commands baseline: module-owned command storage, scheduler,
    retry/error processor, Quartz-triggered `ProcessStatsInboxCommand`, and
    architecture convention coverage.
13. Event bus abstraction baseline: public integration events no longer inherit
    MediatR `INotification`; producers publish through `IEventsBus`, Stats
    consumes through `IIntegrationEventHandler<T>`, and the first implementation
    remains in-process.
14. Module composition isolation review: shared host container kept; event bus
    lifetime tightened to scoped; architecture tests now guard against global
    `DbContext`/`IUnitOfWork`/dispatcher/outbox leakage and singleton bus scope
    capture.
15. Time abstraction: Common `IClock`/`SystemClock` introduced; Players
    time-dependent command decisions and processing retry/processed timestamps
    now use the clock; direct production `DateTime.UtcNow` remains only in the
    clock implementation and domain-event occurrence metadata.

---

## Current Direction

Tamamlanan production readiness baseline:

1. **Production Auth / Identity** — fake bearer baseline'ı production'da
   kapatıldı; first-party signed JWT validation, token issuing boundary,
   guest-to-auth integration coverage ve command-level execution context
   testleri eklendi. Real Apple/Google verifier provider credential'ları
   gelene kadar deferred.
2. **Stats Feature Depth** — daily/weekly leaderboard tamamlandı. Existing
   all-time leaderboard korundu; yeni period read model daily/weekly aggregate
   tutuyor.
3. **API Contract Hardening** — validation/problem-details/OpenAPI contract ve
   endpoint smoke coverage baseline'ı tamamlandı.
4. **Operational Readiness** — health checks, structured logs ve async processor
   görünürlüğü eklendi.
5. **Database Hygiene** — index/query review, DbUp runbook ve migration drift
   readiness guard tamamlandı.
6. **Release Smoke Gate** — local production-mode migration + startup + HTTP
   health smoke script eklendi.

Kalanlar artık aktif production-readiness slice değil:

- Real Apple/Google external token verifier, provider credential/client config
  gelene kadar deferred.
- Full schema diff tooling, tekrar eden drift problemi oluşana kadar non-action.
- Broad permission/UserAccess module, gerçek permission modeli oluşana kadar
  non-action.
- Warnings-as-errors/analyzer policy, mevcut warning borcu temizlenene kadar
  deferred.

Backend tarafında önerilen sıradaki ürün fazı **Game content/admin tooling**
idi. İlk pratik content import hattı `docs/category-spor.json` üzerinden
başlatıldı. 2026-05-13 itibarıyla backend aktif çalışma bilinçli olarak
bekletiliyor; odak Flutter frontend MVP akışında. Frontend'in güncel aktif
sırası `docs/frontendActiveContext.md` içindedir.

**2026-05-14 güncellemesi — Energy modülü kapandı.** Detaylar
`ROADMAP.md > Energy Module` ve `progress.md` içindedir. Önemli mimari
değişiklik: ilk synchronous cross-module gateway (`IEnergyGuard`) eklendi;
`Games.Application` Energy contract'larına structural reference vermez,
adapter API host'ta (`LexiLink.API/CrossModule/EnergyGuard.cs`) yaşar.
ArchTest'ler bu sınırı koruyor. Energy modülü `PlayerRegisteredIntegrationEvent`
dinler ve aggregate'i lazy şekilde init eder; raw inbox pattern'i bilinçli
olarak henüz eklenmedi (Stats'te var, Energy'de gerçek ihtiyaç çıkana kadar
yok).

**2026-05-15 güncellemesi — Quests modülü kapandı.** 8 slice teslim edildi.
Detaylar `ROADMAP.md > Quests Module ✅ closed 2026-05-15` ve `progress.md`
içindedir. Önemli mimari değişiklikler:

- **İlk reverse cross-module event dependency** canlı — `Energy.Application`
  artık `Quests.IntegrationEvents.QuestClaimedIntegrationEvent`'i tüketiyor.
  Stats'in Games/Players IntegrationEvents'i tüketmesiyle aynı pattern;
  granular ArchTest allow eklendi (`LexiLink.Modules.Quests.Domain/Application/
  Infrastructure` Energy'de hâlâ forbidden).
- **`PlayerEnergy.GrantBonus(amount, now)`** eklendi — max kontrolü yapmaz,
  over-max balance'ı bilinçli olarak tutar. `Consume` timer davranışı
  düzeltildi: artık sadece bucket "at/above max → below max" geçerken
  `_lastRefilledOn` set ediliyor (önceki davranış 10/5 → 9/5 case'inde
  timer'ı yanlışlıkla sıfırlıyordu).
- **Hardcoded 4 quest catalog** (FirstGameCompleted +3⚡, ThreeGamesCompleted
  +5⚡, AccountLinked +5⚡ prereq=ThreeGames, DailyThreeGames +5⚡). Daily
  expiry lazy: `IssueQuestCommand` daily için `_expiresAt = NextUtcMidnight`,
  `RecordQuestProgressCommand` ve `GetActiveQuestsQuery` `ExpireIfPast(now)`
  ile lazy projection.
- **API:** `GET /quests/me`, `POST /quests/{id}/claim` (her ikisi de
  `AuthenticatedPlayer` policy; başka oyuncunun questi → 404).

---

## Active Constraints

- API endpoint dağıtım stili bilinçli karar; Kamil'e benzetmek için
  controller/minimal API yapısı değiştirilmeyecek.
- PostgreSQL + DbUp + SQL script yapısı bilinçli karar; SQL Server/SSDT veya EF
  migrations'a geçilmeyecek.
- Shared host container şimdilik kabul edilebilir. Bu model altında module-owned
  UnitOfWork/domain dispatcher yaklaşımı korunmalı.
- Decorator registration split'i Kamil'den birebir kopyalanmayacak; LexiLink'te
  denenmiş ve command decorator bypass riski doğurmuştu.
- Stats şu an read-model/projection module'dür. Gerçek invariant oluşmadan
  yapay Domain layer eklenmeyecek.
- Energy modülü ilk synchronous cross-module gateway sahibi. Yeni sync gateway
  *yalnızca* invariant-level cross-module check için açılır; her açılışta
  `IEnergyGuard` pattern'i (consumer-module Application interface + API host
  adapter) korunmalı, ArchTest'lerle reverse-dependency yasak.
- Quests reward delivery **event-driven** kalır — Energy bonus için yeni bir
  sync gateway açılmaz. `QuestClaimedIntegrationEvent` outbox üzerinden
  `IEventsBus.PublishAsync`, sonra Energy.Application
  `QuestClaimedIntegrationEventHandler` defensive `EnsurePlayerEnergyExists`
  + `GrantEnergyCommand`. Bu pattern reverse cross-module event dep için
  şablondur; benzer ihtiyaçlar (ör. başka modüllerin Quests event'i tüketmesi)
  aynı şekilde granular `IntegrationEvents` allow ile çözülür.
- `PlayerEnergy.GrantBonus(amount, now)` over-max'ı bilinçli olarak izin verir;
  `EnergyAmountCannotExceedMaximumRule` `Consume`/`GrantBonus` üzerinde
  enforce edilmez (defansif invariant olarak kalır). `Consume` timer'ı
  yalnızca at/above max → below max geçişinde set edilir.
- Quests'te raw inbox pattern *bilinçli olarak yok* — Energy gibi inline.
  Gerçek duplicate/retry sorunu çıkana kadar Stats-style raw inbox
  eklenmeyecek.
- **Sprint Q1 sonrası**: Quests artık Game/Auth integration event'lerini
  *dinlemiyor*. `GameCompletedIntegrationEvent` ve
  `AuthProviderLinkedIntegrationEvent` *yalnız* Stats tarafından
  tüketiliyor (counter projection). Quests counter'ları Stats'ten
  `IQuestCounterReader` sync gateway'i üzerinden lazy okuyor.
- **Sprint Q1 sonrası**: Quest progress *persist edilmiyor* — read
  time'da Stats counter - PlayerQuest.ProgressBaselineSnapshot ile
  hesaplanıyor. `PlayerQuest.RecordProgress` ve `ExpireIfPast`
  Domain'den silindi. Daily quest expiry row-deletion ile yönetiliyor.
- **Sprint Q1 sonrası**: `QuestDefinition.Name` ve `QuestDefinition.Trigger`
  oluşturulduktan sonra immutable; Update yalnız Description/Threshold/
  Reward/Prereq/ProgressBaseline değiştirir. Name değişikliği PlayerQuest
  history'sini bozardı (snapshots eski threshold'la boyutlandırılmıştır);
  Trigger değişikliği baseline'i invalidate ederdi.
- `LexiLinkBearer`/`DevelopmentBearer` şimdilik baseline/test scheme'idir.
  `Authentication:Mode=DevelopmentBearer` production'da startup fail eder.
- `Authentication:Mode=ProductionJwt` issuer, audience, lifetime, HMAC signature
  ve GUID `sub` doğrular.
- `POST /auth/token` sadece external identity verifier başarılıysa JWT üretir.
  Mevcut `DevelopmentExternalToken` verifier production'da yasaktır; gerçek
  Apple/Google verifier provider credential'ları geldiğinde eklenecek.
- Full local gate: `./scripts/test.sh --no-restore -v minimal`. Integration test
  projeleri shared local DB kullandığı için serial çalıştırılmalı.
- Package version değişiklikleri `Directory.Packages.props` üzerinden yapılmalı.

---

## Working Files To Watch

- `docs/ROADMAP.md` — kapanan production readiness baseline ve sıradaki faz
  adayları.
- `docs/kamil-modular-monolith-comparison.md` — farkların gerekçesi ve kapsam
  dışı kararlar.
- `docs/progress.md` — teslim edilen işlerin kronolojik kaydı.
- `scripts/test.sh` — local quality gate.
- `scripts/smoke.sh` — production-mode local smoke gate.
- `src/Tests/ArchitectureTests/` — boundary ve convention koruma testleri.

---

## Archived Payments Next Action

Historical note from Sprint P: the next implementation action at that time
was P7 — frontend purchase UI. Backend semantics covered catalog,
verify+grant, delivery retry, post-processing, and notification
reconciliation shells.

P7 guardrails:

- Mobile-only purchase controls; web must show a safe unavailable
  state, not a fake checkout.
- Client displays store-localized price only; backend still owns
  product id → Diamond amount.
- Client must submit platform proof to `POST /payments/iap/verify`
  and finish iOS transactions only when `CanFinishTransaction` is true.
- Pending purchases should be replayed on app start.
- Successful delivery refreshes Diamond badge.

Backlog candidates intentionally not active while Sprint P continues:

- **Game completion → Diamond bonus.** New reverse event dependency:
  `Diamond.Application → Games.IntegrationEvents.GameCompletedIntegrationEvent`.
- **Analyzer cleanup.** Pre-existing Flutter info warnings if the
  project wants `flutter analyze` green.
- **Tutorial flow / Quest trigger expansion / scoring formula reshape**
  remain product backlog candidates after real-money infrastructure.

Sprint UR sonrası diğer backlog adayları (D sonrası):

- **Quest yeni trigger türleri.** `StreakReached`,
  `CategoryMastered`, `LeaderboardReached`. Her biri
  `IQuestCounterReader` adapter'ına yeni read path ekler;
  `QuestTrigger` enum genişler.
- **Tutorial flow.** Yeni guest player için 1-2 puzzle'lık
  rehberli giriş; "Hint öğren", "Undo öğren", "Reset öğren"
  tarzı sıfır-zorluk puzzle'larıyla.
- **Game scoring formula reshape.** Scoring şu an Hint allowance
  kullanımına bağlı; Undo/Reset counter'ları artık inventory
  consumption'a karşılık geliyor. Score formülünün 4 inventory
  arasında nasıl ayrılacağını gözden geçirmek.

---

## Last sprint closure (kept for context)

**Administration backend sprint — B1–B10 kapandı.**
B1 modül foundation 2026-05-18, B2 admin registration + outbox publish +
bootstrap seed 2026-05-19, B3 admin authentication 2026-05-20, B4
admin authorization cross-cut 2026-05-20, B5 audit projection +
`/admin/audit` endpoint 2026-05-20, B6 Quests catalog data-driven
2026-05-20, B7 quest admin operations + first per-module
AdminAuditing decorator 2026-05-20, B8 energy admin operations
2026-05-20, B9 players ban/unban + auth boundary 2026-05-20,
B10 content admin guard 2026-05-21.

Backend tarafı tamam. F1-F6 frontend slice'ları sıraya alındı.

B3 ile birlikte:
- `IExecutionContextAccessor` artık `IsAdmin` + `AdminUserId` taşıyor
  (her modülün `TestExecutionContextAccessor`'ı güncellendi).
- `AuthenticatedAdmin` policy + `role=Admin` claim + `admin_id`
  claim — `RoleClaimType`/`AdminUserIdClaimType` `AuthConstants`'ta.
- `LexiLinkBearerAuthenticationHandler` admin lookup yapıyor: dev
  modda bearer GUID `administration.AdminUsers`'ta Active'se claim'ler
  eklenir; production JWT'de admin role claim varsa Active doğrulaması
  yapılır (revoke edilmiş admin token çürür).
- `IAdminLookup` API host adapter (`AdminLookup` → `IAdministrationModule`
  query). Kamil `IEnergyGuard` pattern'inin aynısı.
- `Administration.Application` iki query: `GetActiveAdminUserByIdQuery`,
  `GetActiveAdminUserByEmailQuery` + `AdminUserDto`.
- `JwtTokenIssuer.IssueAdmin(adminUserId)` admin JWT üretir (sub + role +
  admin_id claim).
- `POST /auth/admin/token` — `DevelopmentExternalAdminIdentityVerifier`
  ile e-mail bazlı doğrulama; başarılıysa admin JWT döner.
- `GET /admin/whoami` — `AuthenticatedAdmin` policy korumalı, current
  admin'i döner. 401/403/Active-doğrulama 8 API.Tests ile kilitli.
- `Authentication:AdminTokenExchange:Mode` config bölümü;
  `LexiLinkAuthOptionsValidator` production'da
  `DevelopmentExternalToken`'ı reddediyor.

B4 ile birlikte:
- `Common.Application.Admin/IAdminCommand` marker — B5 audit
  decorator admin command'larını bu marker üzerinden keşfedecek.
- `Common.Application.Admin/IAdminAuthorizationContext` interface —
  `IsAdmin`, `AdminUserId`, `RequireAdminUserId()`,
  `EnsureAuthorized()`. Per-module re-deklare etmek yerine
  Common'a koyuldu (Kamil disiplini gerekçesi:
  `IExecutionContextAccessor` zaten Common'da, admin context aynı
  cross-cutting kategoriden — tek role/no permission matrix
  varsayımıyla per-module interface gereksiz tekrar olur).
- `Common.Application.Admin/AdminAuthorizationException` — yetkisiz
  admin akışında fırlatılır.
- `LexiLink.API/CrossModule/AdminAuthorizationContext` adapter —
  `IExecutionContextAccessor`'dan okur, B3'te stamp'lenen claim'lere
  yaslanır. `ExceptionHandlingMiddleware` artık
  `AdminAuthorizationException`'ı 403 ProblemDetails'e çeviriyor.

B5 ile birlikte (consumer-side):
- `Administration.IntegrationEvents/AdminActionPerformedIntegrationEvent`
  public contract (Id, OccurredOn, AdminUserId, ActionType, TargetType,
  TargetId, PayloadJson). PayloadJson kasıtlı olarak opak — her modülün
  command'ı farklı şekilli, merkezi audit storage bu varyansı encode
  etmemeli.
- `administration.AdminActionAudit` tablosu (PK Id, indexes on
  Actor/OccurredOn, Target/OccurredOn, OccurredOn).
- `IAdminActionAuditWriter` (Administration.Application interface) →
  `AdminActionAuditWriter` (Administration.Infrastructure Dapper impl,
  INSERT ... ON CONFLICT DO NOTHING ile idempotent).
- `AdminActionPerformedIntegrationEventHandler` (Administration.Application
  IIntegrationEventHandler) writer'ı çağırır.
- `GetAdminActionsQuery` + handler + `AdminActionDto` —
  filtered (adminUserId / targetType / targetId) + paged (default 50,
  max 200).
- `GET /admin/audit` endpoint (`AuthenticatedAdmin` policy).
- 4 IT (synthetic publish → projection / republish idempotent /
  filter by target / OccurredOn DESC) + 4 API.Tests (401 anon, 403
  player, 200 admin + filter).

B5 deferred (her hedef modülün admin slice'ında geliyor): producer-side
`AdminAuditingCommandHandlerDecorator<TCommand>` per-module
(Kamil decorator-per-module rule); decorator `IAdminCommand` marker'lı
command'ı yakalar, actor + serialized payload ile event'i modülün
outbox'ına yazar.

B6 ile birlikte:
- `QuestDefinition` artık aggregate (Entity + IAggregateRoot,
  `QuestDefinitionId` typed id). Static `Create`, `Update`,
  `Deactivate`, `Reactivate` davranışları + 3 domain event
  (Created/Updated/ActivationChanged). Goal/Reward kuralları mevcut
  `QuestGoalMustBePositiveRule` / `QuestRewardAmountMustBePositiveRule`
  ile re-used.
- `IQuestDefinitionRepository` (Quests.Domain): GetById /
  GetByQuestType / GetAll / Add. EF-backed.
- `IQuestCatalog` async'e geçti — `ResolveAsync` + `GetAllActiveAsync`,
  Active filtre catalog seviyesinde. Deaktif quest → `null` döner,
  `IssueQuestCommandHandler` no-op'a düşer (mevcut PlayerQuest
  history bozulmaz).
- `quests.QuestDefinitions` tablosu (UX QuestType, IsActive index) +
  `021_SeedQuestDefinitions.sql` ile mevcut 4 tanım deterministic
  UUID'lerle (`11111111-0000-0000-0000-00000000000{1..4}`) seed'lendi.
  ON CONFLICT DO NOTHING ile idempotent.
- Mevcut 23 Quests.Tests + 5 Quests.IT seed sayesinde hâlâ yeşil;
  9 yeni QuestDefinition unit test eklendi (Create happy + rule
  ihlalleri, Update tunable fields, Deactivate/Reactivate
  idempotency).
- ArchTests `AggregateRoots_Should_ImplementIAggregateRoot`
  listesinde `QuestDefinition` artık var.

Bilinçli karar: mevcut 4 seed tanım `QuestDefinition.Create` çağırmadan
SQL ile insert edildi. Domain aggregate'in factory'sini bypass eder
ama bu dört tanım known-good ve aggregate'ten önce vardı; production
seed'in bypass'ı SQL idempotency garantisinin tek noktada kalmasını
sağlıyor. Admin command'larıyla (B7) eklenecek yeni tanımlar
`QuestDefinition.Create` üzerinden geçecek.

B7 ile birlikte:
- `IAdminCommand` artık `AuditTargetType` ve `AuditTargetId` taşıyor
  (mandatory). Audit decorator bunları kullanarak target metadata
  doldurur. Mevcut implementasyon yoktu, breaking değil.
- `Quests.Application/Admin/` altında 5 admin command + 1 admin query:
  Create/Update/Deactivate QuestDefinition, IssueQuestToPlayer (internal
  `IssueQuestCommand`'ı sarar — idempotency aynı), ResetPlayerQuest,
  GetQuestDefinitions (Active+Inactive listesi).
- `PlayerQuest.AdminReset(now, newExpiresAt)` domain method +
  `PlayerQuestAdminResetDomainEvent`. Caller cadence'a göre yeni expiry
  hesaplar.
- `Quests.Infrastructure/Configuration/Processing/AdminAuditingCommandHandlerDecorator`
  ilk per-module audit template: `IAdminCommand`'lı command'ları yakalar,
  `IAdminAuthorizationContext.RequireAdminUserId()` ile fail-fast 403,
  inner handler başarılıysa actor + serialized payload ile
  `QuestsAdminActionPerformedNotification` outbox'a yazar. RegisterGenericDecorator
  INNERMOST sırada — UoW commit aynı tx'te outbox row'unu saklar.
- `QuestsAdminActionPerformedNotificationHandler` outbox processor
  drain edince `AdminActionPerformedIntegrationEvent`'i IEventsBus'ta
  yayınlar; Administration consumer'ı `administration.AdminActionAudit`'e
  yazar.
- `Quests.Infrastructure` → `Administration.IntegrationEvents` project
  reference (granular ArchTest allow eklendi —
  Administration.Domain/Application/Infrastructure hâlâ forbidden).
- API host `AdminQuestEndpoints`: `GET /admin/quests/definitions`,
  `POST /admin/quests/definitions` (201 + id), `PUT /admin/quests/definitions/{id}`,
  `POST /admin/quests/definitions/{id}/deactivate`,
  `POST /admin/quests/players/{playerId}/issue`,
  `POST /admin/quests/players/{playerId}/{playerQuestId}/reset`. Hepsi
  `AuthenticatedAdmin`.

B7 IT (Quests.IT 5→11): non-admin command admin endpoint'inde
`AdminAuthorizationException`; admin login → command çalışır →
DB state değişir → ProcessOutboxAsync sonrası
`administration.AdminActionAudit`'te actor + TargetType +
TargetId ile audit row görünür. Create duplicate type → 400, audit row
YOK (UoW rollback nedeniyle outbox commit edilmemiş). Quests.IT
TestBase artık Administration modülünü de boot ediyor —
end-to-end audit roundtrip için.

Quests.IT'ye eklenen test stub: `TestAdminAuthorizationContext`
(mutable, LoginAs/Logout). Non-admin testler default'ta. Admin tests
[SetUp] sonrası `AdminContext.LoginAs(adminId)` çağırır.

B8 ile birlikte:
- `PlayerEnergy.AdminSet(newAmount, now)` — 0..max range'inde set
  (over-max yasak; `GrantBonus` zaten over-max'a izin veriyor).
  At/above max → below max geçişinde recharge timer rearm edilir.
- `PlayerEnergy.AdminReset(now)` — current=max, lastRefilledOn=now.
- 2 yeni event: `PlayerEnergyAdminSetDomainEvent`,
  `PlayerEnergyAdminResetDomainEvent`.
- `Energy.Application/Admin/`: `SetPlayerEnergyCommand`,
  `GrantBonusEnergyCommand` (internal `GrantEnergyCommand`'ı wrap;
  bonus path tek noktada kalır), `ResetPlayerEnergyCommand`.
  Validator + handler + 3 endpoint.
- `Energy.Infrastructure/Configuration/Processing/AdminAuditingCommandHandlerDecorator`
  Quests B7 template'inin Energy-private kopyası
  (decorator-per-module Kamil rule).
- `EnergyAdminActionPerformedNotification` + handler;
  `EnergyStartup` static ctor DomainNotificationsMap registration.
- `Energy.Infrastructure` → `Administration.IntegrationEvents` project
  reference (ArchTest granular allow eklendi).
- API: `POST /admin/players/{playerId}/energy/set|grant|reset`
  (`AuthenticatedAdmin`).
- Energy.IT 4→8: non-admin → AdminAuthorizationException;
  Set 1 (snap), Grant (+3 → over-max), Reset (current=max). Hepsi
  audit row roundtrip ile.

B9 ile birlikte:
- `Player.Ban(reason, now)` / `Player.Unban(now)` domain methods
  (idempotent). `_isBanned/_bannedReason/_bannedAt` fields,
  `BanReasonMustNotBeEmptyRule`, `PlayerBannedDomainEvent` +
  `PlayerUnbannedDomainEvent`. Player aggregate'in public read
  properties'leri (DisplayName, IsGuest, IsBanned vb.) admin detail
  query'si için açıldı.
- `players.Players` tablosuna `IsBanned/BannedReason/BannedAt`
  kolonları (`030_AddPlayerBanColumns.sql`, ALTER TABLE idempotent +
  partial index on banned).
- `Players.Application/Admin/`: `BanPlayerCommand` (Reason mandatory,
  max 500), `UnbanPlayerCommand`, `GetPlayerAdminDetailQuery` +
  `PlayerAdminDetailDto` (handle, providers count, ban state),
  `GetPlayerBanStatusQuery` (auth boundary için ucuz lookup).
- `Players.Infrastructure` per-module audit decorator + notification
  +handler (B7 template). PlayersStartup DomainNotificationsMap
  registration. Autofac decorator chain innermost.
- API host `IPlayerStatusLookup` + `PlayerStatusLookup` adapter
  (`IPlayersModule` üzerinden query). `LexiLinkBearerAuthenticationHandler`
  hem dev bearer hem production JWT path'inde ban check yapıyor —
  banned player → `AuthenticateResult.Fail` (401). Admin tokens
  ban check'ten exempt (admin hesabı banned player olabilir ama
  yine de admin olarak login olabilir). Bilinmeyen GUID'ler
  reddedilmez (fresh device registration için).
- `AdminPlayerEndpoints`: `GET /admin/players/{id}`,
  `POST /admin/players/{id}/ban`, `POST /admin/players/{id}/unban`.
- ArchTests: Players.Infrastructure → Administration.IntegrationEvents
  granular allow.

Tests:
- Players.IT 7→14 (+7): non-admin → AdminAuthorizationException;
  Ban happy path → DB flag + audit; Unban → DB clear + audit;
  Ban empty reason → 400; GetPlayerAdminDetail rich payload + null
  for missing; GetPlayerBanStatus false for unknown id (auth
  boundary safety).
- API.Tests 49→51 (+2): unknown GUID dev bearer → 200 (registration
  path); banned player dev bearer → 401.

Bootstrap admin SQL seed bypass'i artık aynı testte 2'inci slice'a
geliyor — Stats.IT TestBase de `NoAdminAuthorizationContext` stub'ı
kazandı (decorator activation için).

B10 ile birlikte:
- 7 Games command'ı `IAdminCommand`'a dönüştürüldü (Create/Edit
  Category, Create/Activate/Deactivate Link, Add/Remove Outgoing).
  `AuditTargetType` `"Games.Category"` veya `"Games.Link"`. Player
  gameplay command'ları (CreateGame, StartGame, MakeStep vb.) admin
  değil.
- `GamesAdminActionPerformedNotification` + handler +
  `AdminAuditingCommandHandlerDecorator` Games.Infrastructure'a
  eklendi (4'üncü ve son per-module template kopyası).
  `GamesStartup` DomainNotificationsMap kaydı.
- Games.Infrastructure → Administration.IntegrationEvents project
  reference + ArchTest granular allow.
- Yeni `AdminContentEndpoints`: POST `/admin/content/categories`,
  PATCH `/admin/content/categories/{id}`, POST `/admin/content/links`,
  POST/DELETE `/admin/content/links/{linkId}/outgoing/{outgoingLinkId}`,
  POST `/admin/content/links/{id}/activate|deactivate`. Tümü
  `AuthenticatedAdmin`.
- Eski `POST /categories`, `PATCH /categories/{id}`, `POST /links/*`
  silindi. GET `/categories`, GET `/links/{id}`, GET
  `/links/{id}/outgoing`, GET `/categories/{id}` player akışı için
  `AuthenticatedPlayer` policy'sinde kaldı.
- Games.IT TestBase Administration'ı da boot ediyor ve default'ta
  bir synthetic admin login eder; mevcut testler content command'larını
  seed amaçlı kullandığı için. ContentAdminCommandTests non-admin
  path için `Logout` çağırır.
- Stats.IT'nin `NoAdminAuthorizationContext` stub'ı artık always-
  logged-in synthetic admin döner — Games + Players cross-module
  decorator zincirini activate etmek için. Audit row'lar test
  arrangement by-product'ı olarak görülür ve tests arası silinir.
- API.Tests `ValidationProblemDetailsTests.CommandValidationFailure`
  testi `/admin/content/categories` endpoint'ine taşındı + admin
  seed.
- 4 yeni Games.IT (non-admin reject, CreateCategory audit, Edit
  category audit target id, Link activate/deactivate çift audit).

Backend sprint kapandı. **Toplam 368/368 test pass.** Modüler
monolith Kamil disiplini ile yazılmış audit edilebilir admin altyapısı
production-ready.

Sırada **Frontend F1-F6** slice'ları:
- F1: Admin login + ayrı session (`/admin/login`, `AdminSessionCubit`,
  ayrı `AdminApiClient`).
- F2: Admin shell + side nav (`AppAdminShell`).
- F3: Quest catalog UI (list + create + edit + deactivate).
- F4: Player search + admin detail + ban/unban UI.
- F5: Energy admin UI (set/grant/reset).
- F6: Audit view UI (paged + filter).

Veya alternatif olarak **Apple/Google external token verifier**
provider credential'ları gelirse backend production auth hattını
kapatmak da gündeme alınabilir.

Diğer aktif olmayan adaylar:

1. **Apple/Google external token verifier** — provider credential'ları
   geldiğinde production JWT issuance hattının son parçası tamamlanır.
2. **Game Content/Admin Tooling (CLI tarafı)** — `CategoryImporter`
   baseline mevcut; daha geniş import/validation workflow ihtiyaç
   çıkarsa Administration modülünden sonra.

Game Options Selection ve target reachability revizyonu için detaylar
`progress.md` ve `ROADMAP.md > Game Options Selection ✅ closed
2026-05-17` içindedir. Quality gate o slice kapanışında 285/285,
0 warning idi.
