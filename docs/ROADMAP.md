# ROADMAP.md

The forward-looking sprint plan. *What's already shipped* lives in `progress.md`; *what's happening right now* lives in `activeContext.md`.

---

## Sprint GO 🔵 active — Production Launch (Hetzner)

Goal: take LexiLink live on a single Hetzner Ubuntu VPS. The game ships to
the **iOS/Android stores** — there is **no browser/web frontend**; the server
hosts the **API only**. Backend runs as Docker Compose: Caddy (auto-HTTPS)
reverse-proxies the .NET 10 API; PostgreSQL 17 runs in a container on the same
box; the DbUp migrator runs as a one-shot before the API on every deploy.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Orchestration | **Docker Compose** on one Ubuntu VPS (API + Postgres + Caddy + one-shot migrator). |
| Database | **PostgreSQL 17 in a container** on the same box, named volume + nightly `pg_dump`. Managed DB deferred. |
| Reverse proxy / TLS | **Caddy** — automatic Let's Encrypt TLS, minimal config. |
| Frontend scope | **API only.** Game is mobile-only via the stores; **no Flutter web** is built or served. CORS stays empty/locked (native clients don't enforce CORS). |
| Routing | `api.wordlope.com` → API container (port 8080). Apex left free for a future marketing site. |
| Registry (GO6) | **GHCR** (GitHub Container Registry). |
| Image shape | **Single image** holds both the API (`/app`) and the DbUp migrator (`/app/migrator`); the API csproj already copies `Database/Structure/**/*.sql` into the publish output, so both the migrator one-shot and the API `/health/ready` journal check read SQL from `/app/Database/Structure`. |
| Secrets | Server-side `/opt/lexilink/.env` (root-only), never committed. `.env.example` documents the keys. |
| Domain | **wordlope.com acquired/configured.** Production API is `https://api.wordlope.com`; GO4 domain/TLS blocker cleared. |

### Slice plan (6 slices)

| Slice | Status | Content |
| --- | --- | --- |
| **GO1** | ✅ Done | Containerization. Multi-stage `Dockerfile` (.NET 10 SDK build → `aspnet:10.0` runtime), publishes API → `/app` and migrator → `/app/migrator`; `ASPNETCORE_URLS=http://+:8080`; `.dockerignore`. SQL scripts ship via the API publish output (`/app/Database/Structure`). Local Docker unavailable on the dev Mac → image build is validated on the server at GO4. |
| **GO-A** | ✅ Done | **Production auth (launch blocker).** Production had no usable identity verifier → `POST /auth/token` 401'd every request (`DisabledExternalIdentityVerifier`), so no player — not even a guest — could log in. Added `ExternalIdentityValidationMode.GuestDevice` + `GuestExternalIdentityVerifier` (accepts the Guest provider with the existing client handshake, rejects Apple/Google) wired in `Program.cs`; allowed in Production. No client change. Set `Authentication__TokenExchange__Mode=GuestDevice` in Production. Gate: API build 0 errors, 5/5 focused verifier unit tests. Real Google/Apple sign-in is a follow-up (see non-actions). |
| **GO2** | ✅ Done | `docker-compose.yml` (postgres + named volume + healthcheck; `migrate` one-shot `depends_on` postgres-healthy, runs `dotnet /app/migrator/...dll "$ConnectionStrings__LexiLinkDb" /app/Database/Structure`; `api` `depends_on` migrate-completed + postgres-healthy, curl `/health/live` healthcheck; `caddy` 80/443 + named volumes) + `Caddyfile` (`api.{$LEXILINK_DOMAIN}` reverse proxy `api:8080`, auto-TLS via `{$LEXILINK_ACME_EMAIL}`) + `.env.example` (all prod env). Shared image/build via a YAML anchor; `curl` added to the Dockerfile runtime for the healthcheck. YAML + anchor-merge validated (Docker unavailable on the dev Mac → full `compose up` validates at GO4). |
| **GO3** | ✅ Done | Store build readiness (no web). Added `docs/MOBILE_RELEASE.md`: prod API wiring (`--dart-define LEXILINK_API_BASE_URL`), real-AdMob id wiring + SSV callback URL, IAP product setup (gated until social sign-in), `flutter build appbundle/ipa` release commands, and a store-readiness checklist. **Surfaced blockers:** Android `applicationId`/iOS bundle id are still the `com.example.*` Flutter placeholders (cannot publish — need a real reverse-DNS id), version is `0.1.0+1` (bump for release), display name is `lexilink_app`. Signing + store accounts + real creds are operator-owned. |
| **GO4** | ✅ Done | Server provisioning + first backend deploy completed by operator. Backend is installed on the server and `https://api.wordlope.com` is healthy; DNS/TLS are working. |
| **GO5** | 🔵 In progress | Backups + ops hardening. Repo-side artifacts prepared: `scripts/backup-db.sh`, `scripts/restore-db.sh`, `docs/DEPLOYMENT.md`, and compose resource/log limits. Server-side apply remains: manual backup, nightly cron, offsite copy, restore drill, `ufw`, SSH hardening, and final health check. |
| **GO6** | ⏭ *(optional, last)* | CI/CD. GitHub Actions on tag/release → build image → push to **GHCR** → SSH deploy (pull + migrate + up). Until then, manual deploy is documented. |

### Deliberate non-actions (launch v1)

- **No web frontend.** Mobile-only via stores; no Flutter web build/serve.
- **No multi-server/HA, no Kubernetes, no managed DB, no CDN.** Single VPS.
- **No store credentials in repo.** AdMob/Apple/Google ids + IAP/SSV creds
  are operator-owned and gate real ads/IAP — independent of server go-live.
  The game + Diamond economy work server-side without them.
- **No real social sign-in at launch.** GO-A ships a guest-only production
  verifier; server-side Google/Apple ID-token verification is a planned
  fast-follow. Guest accounts are device-bound, so **real-money IAP should
  not go live until social sign-in exists** (purchase loss on device change).
  Apple also requires Sign in with Apple once Google sign-in is offered.
- **No production admin-console login at launch.** Surfaced during GO2:
  `POST /auth/admin/token` has no production verifier either
  (`AdminTokenExchange__Mode=Disabled` → 401), so the admin console is not
  usable in production yet. Not a launch blocker — content is imported via the
  `CategoryImporter` CLI against the DB and the game needs no admin
  intervention to run. A production admin verifier is a follow-up.
- **Backend message localization stays deferred** (see Phase 3 above).

### Slice ordering rationale

Containerize first (GO1) so the runtime artifact is fixed before any host
work. Compose + Caddy + secrets (GO2) assemble the stack locally-describable
before touching the server. Store build config (GO3) is independent and can
run in parallel. First deploy (GO4) proved the manual server path on
`api.wordlope.com`. Hardening + backups (GO5) before real traffic. CI/CD
(GO6) automates the now-proven manual deploy.

---

## Sprint CL ✅ closed (2026-06-01) — Content Localization (Phase 2)

Goal: localize game content by moving Categories/Links toward
language-specific word graphs. Phase 1 UI i18n is closed; this sprint owns
content filtering and authoring. Backend rule/API message localization is
still Phase 3.

### Slice plan

| Slice | Status | Content |
| --- | --- | --- |
| **CL1** | ✅ Done | Content language foundation. Add `Category.Language`, migrate existing content to `tr-TR`, expose/filter `/categories?locale=xx-XX`, and thread `LocaleCubit` locale from frontend category loading. |
| **CL2** | ✅ Done | Seed/author first playable `en-US` graph (`docs/category-animals-en.json`), make `CategoryImporter` language-aware, import Animals locally, and verify English content can create/start a game. |
| **CL3** | ✅ Done | Admin content UI language controls: admin category read endpoints, `/admin/content` screen, locale filter, and create/edit language selector. |
| **CL4** | ✅ Done | Content-ops handoff documented. Authoring DE/FR/ES graphs is a content (not code) task and the code path is complete, so CL4 ships `docs/CONTENT_AUTHORING.md` — the repeatable JSON-schema + importer + validation + per-language stable-id + verify/checklist guide — instead of authoring more graphs in-repo. Actual DE/FR/ES authoring is operator/content-ops owned. |

### Closure note

CL1–CL3 delivered the content-language code path (model + migration +
filtering + importer + admin UI). CL4 closed the sprint by documenting the
repeatable authoring handoff (`docs/CONTENT_AUTHORING.md`) rather than
authoring additional language graphs in the repo — per-language word graphs
are a content-ops effort and don't need code work. The `en-US` Animals graph
(CL2) plus the original `tr-TR` content remain the shipped playable set;
DE/FR/ES graphs drop in via the documented importer flow with no code change.
Phase 3 (backend rule/error-message localization) remains deferred.

### Boundaries

- Links inherit language from their Category in this slice; no separate
  `Link.Language` yet.
- Existing Turkish content is treated as `tr-TR`.
- Phase 3 error-code/backend-message localization remains deferred.

---

## Sprint L10N ✅ closed (2026-06-01) — Localization (App UI i18n)

Goal: take the first step toward a real-world launch by localizing the
**app UI** into five languages — **Turkish, English, German, French,
Spanish**. This is **Phase 1 (UI strings only)**. The word-graph game
content stays Turkish for now (Phase 2), and backend rule/validation
messages stay English for now (Phase 3). At sprint start, the frontend UI
was hardcoded **English** (~105+ `Text` literals plus labels/snackbars/
dialogs across ~29 features) with no `flutter_localizations`/`intl` setup.
Current repo state: L1–L8 are delivered in the working tree; Phase 1 UI
i18n is closed. Follow-up content localization belongs to Phase 2, not
this sprint.

### Phased shape

| Phase | Scope | Status |
| --- | --- | --- |
| **Phase 1 — UI i18n** | All app UI strings in 5 languages | **Closed in Sprint L10N** |
| **Phase 2 — Content model** | `language` on Category, filter content by player locale, author TR+EN (then DE/FR/ES) word graphs | **Closed in Sprint CL** (code path; DE/FR/ES authoring is content-ops, see `CONTENT_AUTHORING.md`) |
| **Phase 3 — Backend messages** | Rule/validation → stable error codes + client translation; admin-authored content multilingual | **Deferred (low ROI) — decided 2026-06-01** |

#### Phase 3 — deferred (decided 2026-06-01)

Not started; parked in the backlog. **Why:** LexiLink is a mobile game and of
the ~78 `IBusinessRule` messages across 12 modules, the vast majority are
internal invariants (content/admin/"impossible-from-UI" cases) a normal
player never sees. The errors a player actually hits in normal play are a
small handful (insufficient energy/diamond, daily cap reached, game-state).
Localizing all 78 rules + generic API errors in 5 languages + rewiring cubits
+ test churn is a full sprint for value ~95% of users never see → poor ROI.

**If it resurfaces:** prefer a **mini-slice** — error-code + l10n for only the
~6 player-facing messages, leaving internal-invariant messages English. Full
Phase 3 only on a concrete external need (store review, regulation, "all
surfaces must be multilingual"). The seam already exists: the API
`ExceptionHandlingMiddleware` emits `extensions["rule"]` (rule class name) and
the frontend `ApiProblemDetails` (`api_error.dart`) reads ProblemDetails
extensions.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Languages / locale codes | `tr-TR`, `en-US`, `de-DE`, `fr-FR`, `es-ES`. Backend `Player.Locale` regex (`^[a-z]{2}-[A-Z]{2}$`) requires the region-qualified form; Flutter ARB resolution uses the language code (`tr`/`en`/`de`/`fr`/`es`) and maps to the `xx-XX` form for the backend. |
| Tech | `flutter_localizations` + `intl` + ARB files + `flutter gen-l10n` → `AppLocalizations`, accessed via a `context.l10n` extension. |
| Fallback locale | **English (`en-US`)** when the device locale isn't one of the five. |
| Preference source | Device locale on first launch (if supported), else fallback. Settings language picker. Persisted **device-local** (SharedPreferences) **and** to `Player.Locale` via the existing `UpdateProfile` endpoint. |
| Global wiring | `LocaleCubit` provided above the router (genuinely-global, like `AudioService`/`AdsService`). |
| Test strategy | `en` ARB values are kept **identical** to the current hardcoded copy and tests run in the `en` locale, so existing `find.text('...')` assertions stay green and churn is minimal. |
| Content `language` column | **Phase 2** — Phase 1 stays pure frontend (no schema change). |

### Architecture notes

- **Two localization layers, kept separate.** App UI (this sprint) vs
  game content (Phase 2). The word graph is language-specific — a Turkish
  puzzle can't be machine-translated into a valid German one — so content
  multilingual means per-language authored Category/Link graphs, a content
  effort, not a code task.
- **`Player.Locale` already exists** (format-validated, persisted,
  broadcast in `PlayerRegisteredDomainEvent`/`PlayerProfileUpdatedDomainEvent`)
  but drives nothing yet. The language picker will write to it; nothing
  consumes it server-side until Phase 2 content filtering.
- **Mirror the audio/ads global-service pattern.** `LocaleCubit` +
  `LocalePreferencesRepository` (SharedPreferences) provided above
  `MaterialApp.router`, exactly like `AudioSettingsCubit`/`AudioService`.
- **Phase 3 readiness.** Q1 signalled eventual backend-message
  localization; the client l10n layer is built so error **codes** can be
  mapped to localized strings later without re-plumbing.

### Slice plan (8 slices)

| Slice | Status | Content |
| --- | --- | --- |
| **L1** | ✅ Done | i18n infrastructure. `flutter_localizations` + `intl` deps, `generate: true`, `l10n.yaml`, `lib/l10n/app_en.arb` (template) + `app_{tr,de,fr,es}.arb` skeletons, `flutter gen-l10n`, `MaterialApp.router` `localizationsDelegates`/`supportedLocales` (en first → en fallback) + locale, `context.l10n` extension. Seed a few keys; smoke test resolves `AppLocalizations` for all 5 locales. |
| **L2** | ✅ Done | Locale preference + picker. `LocaleSettings` model, `LocalePreferencesRepository` (SharedPreferences + InMemory), `LocaleCubit` provided above the router; device-locale first-launch resolution with `en-US` fallback. `SettingsScreen` language dropdown → live apply + persist device-local + PATCH `Player.Locale` (`xx-XX`). Verify the locale string matches `LocaleMustBeValidFormatRule`. |
| **L3** | ✅ Done | Extraction — gameplay surface (game, home, categories). |
| **L4** | ✅ Done | Extraction — economy (market, diamond, payments, ads/earn-diamonds). |
| **L5** | ✅ Done | Extraction — quests, profile, leaderboard, settings, auth/splash. |
| **L6** | ✅ Done | Extraction — admin features + shared widgets/dialogs (cancel/apply/retry etc.). |
| **L7** | ✅ Done | Translations. Fill `tr`/`de`/`fr`/`es` ARBs (TR authored; DE/FR/ES start as drafts, swapped later like the audio placeholders). |
| **L8** | ✅ Done | Tests + analyze + docs close-out. Final Flutter suite **166/166**; `flutter analyze` is clean (**No issues found**); locale behavior + Phase 1 boundaries documented in GLOSSARY/OPERATIONS/frontend/global docs. |

### Acceptance criteria

- App UI renders in TR/EN/DE/FR/ES; switching the language in Settings
  applies live and survives restart (device-local) and is written to
  `Player.Locale`.
- Unsupported device locale falls back to English.
- No user-visible hardcoded UI string remains after L3–L6 (game content
  excluded — that's Phase 2).
- Existing widget tests stay green (en values mirror current copy).

### Deliberate non-actions (Phase 1)

- **No game-content translation.** Category/Link word graphs stay Turkish;
  per-language content is Phase 2 (model + authoring).
- **No backend message localization.** Rule/validation messages and
  admin-authored content stay English (Phase 3).
- **No `language` column on Category** in Phase 1.
- **No RTL/new-script languages** beyond the five Latin/Turkish-Latin set.

### Slice ordering rationale

Infrastructure (L1) and the preference/picker (L2) come first so every
later extraction has a working `context.l10n` and a way to switch
languages. Extraction (L3–L6) is mechanical but the bulk of the work, so
it's split by feature area to stay reviewable. Translations (L7) land
once the key set is stable. L8 closes the gate and docs.

---

## Sprint AD ✅ closed (2026-05-31) — Advertising (AdMob)

> AD1–AD7 delivered. Backend SSV verify+grant (Ads bounded context),
> frontend ads infra, interstitial placements, rewarded→Diamond UI, and
> UMP consent + iOS ATT all shipped. Gates: .NET unit/integration green
> (Ads 8/8 unit, 1/1 integration, ArchTests 67/67), Flutter 156/156,
> DbUp 0 pending. Operator-owned manual device verification with AdMob
> test ads + real SSV/AdMob credentials remain (see `progress.md > AD7`).
> Delivery detail in `progress.md` / `frontendProgress.md`.

Goal: add an ad system with three placements: an **interstitial at game
start** (shown ~1/3 of the time, by chance), an **interstitial at game
end** (shown ~1/2 of the time, by chance), and a **rewarded ad** the
player watches to earn Diamond. Interstitials are pure frontend. The
rewarded → Diamond reward is **backend-verified** through AdMob
Server-Side Verification (SSV): the client never grants Diamond; AdMob's
signed server callback is verified by the backend, which then grants
Diamond exactly once through `IDiamondGrant`. This mirrors the Payments
sprint's "backend is the grant authority" discipline.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Ad SDK | **`google_mobile_ads` (AdMob).** Mobile-only (iOS/Android). Web shows a safe unavailable state / no-op, mirroring Payments. |
| New module | **Ads** — separate bounded context, schema `ads`, microservice-extraction candidate. Owns the rewarded-ad grant ledger, SSV verification, idempotency, and daily-cap rule. |
| Placements | Interstitial @ game start (**1/3** random), interstitial @ game end (**1/2** random), rewarded ad (watch → Diamond). |
| Rewarded grant authority | **Backend via AdMob SSV.** Client "earned reward" is never trusted. AdMob's signed callback is verified server-side (signature over Google's public keys), idempotent on `transaction_id`, then `IDiamondGrant.GrantAsync`. Real signature verification stays behind a **fail-closed shell** until credentials are configured; a dev verifier path enables local testing (Payments pattern). |
| Reward economics | **5 Diamond per rewarded ad, daily cap 10 per player** (50 Diamond/day). Backend owns the amount — the ad-network/client reward value is ignored. Daily cap is a domain rule counting grants in the UTC day. |
| Consent / ATT | **In scope (v1).** AdMob UMP consent flow + iOS App Tracking Transparency prompt on startup. Mobile-only; web no-op. |
| Ad unit ids | **AdMob test ad unit ids in dev** (always fill, no real account needed), real ids via config — same "placeholder now, real later" shape as the audio assets. |
| Interstitial probabilities | Frontend constants (`1/3`, `1/2`), easily tunable. Not admin-configurable in v1. |

### Architecture notes

- **Ads mirrors Payments.** Backend is the grant authority; the client
  only requests/shows the ad and passes the player id as the SSV
  `user_id`. The local `onUserEarnedReward` callback does **not** grant —
  the verified SSV callback does. The client refreshes the Diamond badge
  after the grant lands.
- **Reuse `IDiamondGrant`.** `Ads.Application` consumes
  `Diamond.Application/Configuration/CrossModule/IDiamondGrant` (granular
  ArchTest allow, like Payments). Ads emits
  `RewardedAdRewardedIntegrationEvent` for BI/audit. No new Diamond
  contract is needed.
- **Interstitials are pure frontend.** No backend, no reward, no ledger —
  just a probability gate (`Random`) before `AdsService.showInterstitial`.
- **SSV endpoint is anonymous but signature-verified.** Google calls
  `GET /ads/rewarded/callback?...&signature=...&key_id=...`; the handler
  verifies the signature against Google's rotating public keys, resolves
  the player from `user_id`, enforces idempotency on `transaction_id`,
  applies the daily cap, then grants. Local dev uses a fail-open dev
  verifier because Google cannot reach `localhost`.
- **Daily cap + idempotency are domain invariants.** Unique index on
  `transaction_id` prevents double grant; `RewardedAdDailyLimitRule`
  blocks grants once the player has hit the day's cap. Hitting the cap is
  a benign "no reward" outcome, not an error.
- **AdMob test ad units enable local testing.** Like the audio
  placeholders, the app ships with Google's test ad unit ids so ads can
  be exercised without a real AdMob account; real ids drop in via config.
- **Web-safe.** No AdMob on web: `AdsService` no-ops and the rewarded UI
  shows an unavailable state. Mirrors the Payments web handling.

### Slice plan (7 slices)

| Slice | Content |
| --- | --- |
| **AD1** | Ads module foundation (backend). Projects Domain/Application/Infrastructure/IntegrationEvents/Tests/IntegrationTests. `RewardedAdGrant` aggregate (id, playerId, diamondAmount, transactionId, grantedAt) + status; idempotency on `transactionId`; `RewardedAdDailyLimitRule`. DbUp `ads` schema + table (unique index on `transactionId`) + outbox. Autofac module + Startup + UoW + decorator chain + outbox. sln/test.sh/ArchTests registration. |
| **AD2** | SSV verify + grant (backend). `IAdMobSsvVerifier` contract + fail-closed infra shell + dev verifier; `GrantRewardedAdRewardCommand` + handler (verify signature → idempotency → daily cap → `IDiamondGrant` → ledger → `RewardedAdRewardedIntegrationEvent`). `GET /ads/rewarded/callback` (SSV ingress, anonymous, signature-verified) + `GET /ads/rewarded/status` (player's remaining grants today). Config `Ads:RewardedDiamondAmount` (5), `Ads:RewardedDailyLimit` (10), AdMob keys. Tests: verify+grant, replay idempotent, daily-cap exceeded, bad signature rejected. |
| **AD3** | Frontend ads infra. `google_mobile_ads` dep, AdMob **test** app ids in Android `AndroidManifest.xml` + iOS `Info.plist`, test ad unit ids via config. `AdsService` (global, init `MobileAds`, mobile-only, web-safe no-op), `main.dart` init + provider above the router. Smoke test via injectable seam. |
| **AD4** | Interstitial placements. Probability gate (`1/3` at game start, `1/2` at game end) → `AdsService.showInterstitial`. Hook into GameStart success (home) and game finish (game screen). Web no-op; failures never block navigation/flow. |
| **AD5** | Rewarded ad → Diamond. `AdsService.showRewarded` with player id as SSV `user_id`. Player-facing "Earn Diamonds" entry + watch button + daily-remaining display (from `GET /ads/rewarded/status`) + disabled-when-capped state; Diamond badge refresh after grant lands. Web shows unavailable. |
| **AD6** | Consent + ATT. AdMob UMP consent flow + iOS App Tracking Transparency prompt on startup, before ad requests. Mobile-only; web no-op. |
| **AD7** | Tests + manual verification + docs. Full Flutter + .NET gate, DbUp re-run, manual verification with AdMob test ads (interstitial probabilities, rewarded grant via dev SSV path, daily cap, Diamond badge refresh), docs close-out. |

### Acceptance criteria

- A rewarded ad grants exactly the backend-configured Diamond once per
  verified SSV `transaction_id`; replays never double-grant.
- Client-reported reward completion alone never grants Diamond.
- The daily cap blocks further rewarded grants for the player that day.
- Game-start interstitial shows ~1/3 of starts; game-end ~1/2 of
  finishes; neither blocks gameplay if the ad fails to load.
- Web shows safe unavailable/no-op states; nothing crashes when muted.
- AdMob test ad units work locally without a real AdMob account; real
  ids drop in via config with no code change.

### Deliberate non-actions

- **No "remove ads" purchase** in v1 (Payments has no such product yet).
- **No banner ads / native ads / app-open ads** — only the three named
  placements.
- **No ad mediation** (AdMob mediation/other networks) in v1.
- **No web ads.** Mobile-only.
- **No admin-configurable probabilities or reward amount UI** — config
  + constants only.
- **No real AdMob account/credentials in repo.** Real app/ad-unit ids
  and SSV verification creds are operator-owned, like Payments store
  credentials.

### Slice ordering rationale

Backend foundation (AD1) and the SSV verify+grant money path (AD2) come
first so the reward is server-authoritative before any client code.
Frontend ads infra (AD3) precedes the placements (AD4) and rewarded UI
(AD5). Consent/ATT (AD6) wraps ad requests once placements exist. AD7
closes with test-ad manual verification and docs. The rewarded path
deliberately mirrors Payments so the "never trust the client for value"
discipline is reused, not reinvented.

---

## Sprint A ✅ closed — Audio (Sound & Music)

Goal: add sound effects (SFX) and background music to the game, with
per-player **music/SFX on-off and volume** preferences kept
**device-local** (SharedPreferences) behind a repository abstraction
that stays sync-ready. Uses `audioplayers` across iOS/Android/web/
desktop, web-safe. Frontend-only sprint — no backend module, no schema.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Audio package | **audioplayers** — short SFX + simple looping music; least-surprise API, supports all target platforms. |
| Asset strategy | **Placeholder / CC0 sounds first.** The audio infrastructure ships working against placeholders; real sounds later swap only the `assets/audio/` files, no code change. |
| Preference persistence | **Local-only (SharedPreferences) behind `AudioPreferencesRepository`.** No backend in v1. If a real cross-device "sync all my settings" need appears later, the repository's backing store swaps to a backend `Preferences` module without rewriting the frontend. |
| Defaults | Music ON, SFX ON; music volume ~0.5, SFX volume ~0.8 (tunable). |

### Architecture notes

- **Single global audio service.** `AudioService` is one instance,
  initialized in `main.dart` and provided above `MaterialApp.router`
  via `Provider`/`InheritedWidget` — the existing "genuinely global"
  category (auth/session/theme). It manages one background-music
  player + a short-lived SFX player pool and applies mute/volume by
  listening to `AudioSettingsCubit`.
- **Settings are entirely frontend.** `AudioPreferencesRepository`
  reads/writes a SharedPreferences store (same shape as `TokenStore`).
  The cubit reflects cache; no backend call. The abstraction keeps the
  door open to backend sync later.
- **SFX event points (verified in code).** `GameAction { step, hint,
  undo, reset, abandon }`, game outcome `Completed/Failed/Abandoned`
  (`game_screen._outcomeFor`), `AppPrimaryButton` tap, GameStart
  success/failure, quest claim, market/payment success.
- **Web autoplay policy.** Browsers block autoplay until a first user
  gesture; background music starts only after the first tap. Isolated,
  not spread through feature code.
- **Lifecycle.** `AppLifecycleListener` pauses/resumes music on
  background/foreground; route changes switch the menu↔game track.

### Slice plan (7 slices)

| Slice | Content |
| --- | --- |
| **A1** | Frontend audio infrastructure. `audioplayers` dependency, `assets/audio/{sfx,music}/` + placeholder files + pubspec asset registration. `AudioService` (bg player + SFX pool), `SoundEffect` + `MusicTrack` enum catalog. `main.dart` init + global provider wiring. Smoke: one test sound plays. Web-safe init. |
| **A2** | Settings data + state. `AudioSettings` model, `AudioPreferencesRepository` + SharedPreferences store, `AudioSettingsCubit`. Global provider. `AudioService` binds to the cubit (mute/volume). |
| **A3** | Settings UI. `features/settings/` screen: music/SFX toggles + 2 volume sliders. HomeScreen left icon column entry + `/settings` route. Changes apply to audio immediately + persist locally. Web-safe copy. |
| **A4** | Gameplay SFX wiring. step/hint/undo/reset, win (Completed)/lose (Failed), button tap, GameStart success/failure. Only when `sfxEnabled`. |
| **A5** | Background music lifecycle. Menu + in-game loop tracks, route-driven track switch, `AppLifecycleListener` pause/resume, web first-gesture start. Only when `musicEnabled`. |
| **A6** | Reward/economy SFX + polish. Quest claim, market purchase, payment granted sounds. Optional music ducking during SFX. Spam/double-trigger guard. |
| **A7** | Tests + manual verification + docs. Cubit/service/widget tests, `flutter test` + `flutter analyze` green. Manual: toggle/volume persistence, win/lose/step sounds, music lifecycle. Docs updated. |

### Acceptance criteria

- Music and SFX toggle independently with working volume; preference
  survives app restart (local).
- SFX fire at the correct events; background music switches menu↔game
  and pauses on background.
- Web autoplay block does not break the UI; music starts after the
  first tap; everything is sound-safe when muted.
- Swapping placeholder sounds for real ones requires no code change.

### Deliberate non-actions

- **No backend sync** (repository abstraction keeps the door open).
- No haptics/vibration, no remote/streaming assets, no per-sound
  mixer, no admin sound management, no voiceover.

### Slice ordering rationale

Infrastructure (A1) → settings data/state (A2) → settings UI (A3) so
the toggles exist while testing SFX (A4) and music (A5). Reward SFX
(A6) after the gameplay SFX template settles. A7 closes. Every slice
is small, runnable, and reversible.

### Closure note

A1–A7 delivered. Final gate: Flutter tests **137/137**; `flutter
analyze` adds no Audio findings (only the 12 pre-existing info
warnings remain). Operator manually verified on Chrome web against the
live API (Spor content imported): menu/in-game music, first-gesture
autoplay start, button/step/hint/undo/reset/win/lose SFX, settings
toggles + volume + local persistence, and quest/market/payment cues.

**Bug found + fixed during manual verification:** audioplayers 6.7.0 on
web calls `dart:io` `existsSync` on the **second+** play of the same
asset (cache recheck), throwing `UnsupportedError` in the browser. Our
best-effort `try/on Exception` did **not** catch it (it's an `Error`,
not an `Exception`), so it leaked as console spam and repeat plays were
silent. Fix: best-effort catches widened to `on Object`, and on web the
cache entry is evicted before each play (`AudioCache.clear`) so playback
takes the fresh-fetch path and skips the buggy recheck. No-op on native.

**Remaining (intentional, not a repo slice):** the shipped sounds are
**placeholder tones**. Real SFX/music drop into `frontend/assets/audio/`
under the same filenames with **no code change** — see
`frontend/assets/audio/README.md` for the file→effect manifest.

---

## Sprint P ✅ closed — Payments / In-App Purchase

Goal: ship the real-money purchase path for iOS and Android: players
buy **Diamond bundles** through Apple App Store / Google Play in-app
purchase, backend verifies the store transaction server-side, records
an append-only payment ledger, and grants Diamond exactly once. This is
intentionally separate from Market: **Market spends Diamond** on
Energy/Hint/Undo/Reset; **Payments earns Diamond** from platform
commerce. Because real money is involved, correctness, idempotency,
fraud resistance, and recoverability outrank UI speed.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Module name | **Payments** — separate bounded context, schema `payments`, microservice extraction candidate. |
| First product type | **Consumable Diamond bundles only.** No subscriptions, no battle pass, no no-ads, no paid unlocks in v1. |
| Store products | Apple/Google one-time consumable products mapped to backend `PaymentProduct` rows. Proposed IDs: `diamond_100`, `diamond_550`, `diamond_1200`, `diamond_2500`. |
| Price authority | Storefronts own localized price/currency. Backend owns only ProductId → DiamondAmount and active/platform availability. Client price is display-only. |
| Verification authority | Backend verifies every purchase with Apple/Google before Diamond grant. Client proof is input, never authority. |
| Diamond grant | Payments calls Diamond via `IDiamondGrant.GrantAsync(playerId, amount)` only after successful store verification and idempotency check. |
| Idempotency | Unique store transaction keys: `(Platform, StoreTransactionId)` for Apple where present, `(Platform, PurchaseToken)` for Google, plus optional `(PlayerId, ClientRequestId)` for friendly replay. Duplicate proof returns the existing result and never grants twice. |
| Apple flow | StoreKit 2 on client; transaction JWS / transaction id sent to backend; backend verifies signed transaction / App Store Server API state; client finishes transaction only after backend confirms delivery. |
| Google flow | Play Billing on client; purchase token + product id sent to backend; backend calls Google Play Developer API (`purchases.products` / products v2) and performs acknowledge/consume server-side where applicable. |
| Notifications | Apple App Store Server Notifications V2 + Google RTDN are persisted raw and processed idempotently to update payment/refund/revocation state. |
| Refund policy v1 | Do not automatically create negative Diamond. Mark payment `Refunded`/`Revoked`, emit audit/support signal, and defer automatic clawback until product policy is explicit. |
| Payment ledger | `IapPurchase` is append-mostly. Corrections are status transitions with timestamps/reasons, not row deletion. |
| External payment links | None. Use Apple/Google IAP only for mobile digital currency to avoid store policy risk. |

### Architecture notes

- **Payments is not Market.** A player buys Diamond with real money in
  Payments, then spends Diamond in Market. This keeps platform receipt
  verification, refunds, chargebacks, and support tooling out of the
  game-economy purchase log.
- **Server-side verification is mandatory.** The app can initiate the
  store UI, but Diamond is granted only after the backend checks Apple
  or Google. The backend ignores client-supplied amount/price/currency
  and resolves the grant amount from `PaymentProduct`.
- **Delivery before finish.** For iOS, the client must not finish the
  StoreKit transaction until backend delivery is persisted and Diamond
  is granted. For Android, backend acknowledge/consume is preferred so
  the secure server owns post-purchase state changes.
- **Recoverable grant failures.** If store verification succeeds but
  `IDiamondGrant` fails, persist `VerifiedButGrantFailed` (or
  equivalent) and retry from a background processor. A paid purchase
  must not vanish because a downstream module was temporarily down.
- **Store account binding.** Client purchase requests should attach a
  stable player account token: Apple `appAccountToken`; Google
  `obfuscatedAccountId` / profile id. Backend verifies it matches the
  authenticated player when the platform response includes it.
- **Notifications are reconciliation, not the first grant path.**
  The normal path is client purchase → backend verify → grant. Apple
  server notifications / Google RTDN reconcile out-of-band changes
  such as cancellation, refund, revocation, and delayed purchase state.
- **Supportability is a first-class acceptance criterion.** Admins must
  be able to inspect payment history by player, platform transaction
  id/token, product id, status, and failure reason before production.

### Slice plan (8 slices)

| Slice | Content |
| --- | --- |
| **P1 ✅ delivered** | Payments module foundation. Projects: Domain/Application/Infrastructure/IntegrationEvents/Tests/IntegrationTests. Aggregates: `PaymentProduct`, `IapPurchase`, `PaymentNotification`. VOs/enums: `PaymentPlatform {Apple, Google}`, `PaymentEnvironment {Sandbox, Production}`, `IapPurchaseStatus {Received, Verified, Granted, VerifiedButGrantFailed, Failed, Refunded, Revoked}`, `StoreProductId`, `StoreTransactionId`, `PurchaseToken`. DbUp `payments` schema/tables/outbox/inbox; unique indexes for Apple transaction id, Google purchase token, and optional player client request id. Autofac Startup + UoW + domain events dispatcher + decorators + outbox. API host startup registration, sln/test script registration, ArchitectureTests coverage. Verification: Payments unit tests 7/7, Payments integration smoke 1/1, ArchitectureTests 64/64, API build green. |
| **P2 ✅ delivered** | Product catalog. Admin CRUD for `PaymentProduct` (create/update/deactivate) marked `IAdminCommand` and audited through Payments outbox → Administration `AdminActionPerformedIntegrationEvent`. Seed default Diamond bundles (`diamond_100`, `diamond_550`, `diamond_1200`, `diamond_2500`). Player `GET /payments/products?platform=...` returns active products by platform with backend product id and Diamond amount; localized price remains frontend/store-provided. Admin endpoints expose product list/detail/CRUD and IAP purchase lookup surfaces (`/admin/payments/purchases`) for support. Verification: Payments unit tests 9/9, Payments integration smoke 1/1, ArchitectureTests 64/64, full solution build green. |
| **P3 ✅ delivered** | Platform verifier contracts. Application interfaces `IAppleIapVerifier`, `IGooglePlayIapVerifier`, `IGooglePlayPurchaseProcessor` (ack/consume), config objects for bundle id/package name/environment/credentials (`Payments:Apple`, `Payments:Google`), fake verifiers/processors for tests, and fail-closed infrastructure adapter shells behind interfaces. Production credentials stay env/secret-config only. Real App Store / Play Developer API calls are deferred; no Diamond grant yet. Verification: Payments unit tests 12/12, Payments integration smoke 1/1, ArchitectureTests 64/64, full solution build green. |
| **P4 ✅ delivered** | Verify + grant command. `VerifyIapPurchaseCommand` / `POST /payments/iap/verify`. Handler validates authenticated player, product catalog, platform proof, idempotency, store verification result, account binding, and purchase state. On success records `IapPurchase`, calls `IDiamondGrant.GrantAsync`, marks `Granted`, emits `IapPurchaseGrantedIntegrationEvent`, and returns `{ paymentId, productId, diamondAmount, status, isReplay }`. Duplicate client request / Apple transaction id / Google purchase token returns existing result without double grant. Store verification failure records `Failed`; Diamond grant exception records recoverable `VerifiedButGrantFailed`. Verification: Payments unit tests 15/15, Payments integration smoke 1/1, ArchitectureTests 64/64. |
| **P5 ✅ delivered** | Platform post-processing and recovery. Ledger now records `PostProcessingAction`, `PostProcessingStatus`, `PostProcessedAt`, and `PostProcessingFailureReason`. Android Google acknowledge/consume is invoked from backend after grant through `IGooglePlayPurchaseProcessor`; iOS verify responses include `CanFinishTransaction` once backend delivery is safely granted. `RetryIapPurchaseDeliveryCommand` + `POST /admin/payments/purchases/{id}/retry-delivery` retry stuck `VerifiedButGrantFailed` deliveries and failed Google post-processing. Admin purchase DTOs expose post-processing status/failure fields. Tests cover Google consume after grant, failed consume retry, and delivery-failure retry allowing iOS finish. Verification: Payments unit tests 18/18, Payments integration smoke 1/1, ArchitectureTests 64/64. |
| **P6 ✅ delivered** | Notifications/reconciliation. Added Apple App Store Server Notifications V2 and Google RTDN endpoint surfaces (`/payments/notifications/apple`, `/payments/notifications/google`), verifier contracts plus fail-closed infrastructure shells, raw `PaymentNotification` persistence before processing, idempotent notification replay, refund/revocation/failure transitions on `IapPurchase`, and `IapPurchaseStatusChangedIntegrationEvent` support/audit signal. No automatic negative Diamond in v1. Real cryptographic verification remains behind fail-closed shells until production credentials/SDK integration are configured. Verification: Payments unit tests 20/20, Payments integration smoke 1/1, ArchitectureTests 64/64. |
| **P7 ✅ delivered** | Frontend purchase UI. Added Flutter `in_app_purchase` integration behind `features/payments/`: product query, Diamond bundle cards, localized store price display, purchase state machine, pending purchase replay through the store purchase stream, backend verify call, Diamond badge refresh, error/retry UI, and transaction finish gated by backend `CanFinishTransaction`. HomeScreen links to `/payments`. Mobile-only purchase controls; web shows unavailable/state-safe copy. Verification: payments Flutter tests 6/6, full Flutter suite 113/113. `flutter analyze` has no Payments-specific findings; only pre-existing info-level frontend warnings remain. |
| **P8 ✅ delivered** | Tests/manual verification/docs close. Final local gates: Payments unit tests 20/20, Payments integration smoke 1/1, ArchitectureTests 64/64, Flutter tests 113/113, DbUp migrator re-run 0 pending scripts, local API readiness healthy with 79/79 DbUp scripts applied, JWT-mode guest/category smoke passed. Docs updated: `activeContext`, `progress`, `GLOSSARY`, `OPERATIONS`, `ROADMAP`, frontend active/progress. Store-network manual verification is operator-owned and credential-gated: Apple sandbox purchase, Google internal test purchase, real notification cryptographic verification, and native app-kill recovery require App Store / Play Console credentials, product setup, and signed native builds outside this workspace. |

### Acceptance criteria

- A valid Apple/Google purchase grants the configured Diamond amount
  exactly once.
- Replaying the same Apple transaction or Google purchase token never
  grants Diamond twice.
- Client-supplied Diamond amount, price, or currency is ignored.
- Invalid, pending, cancelled, refunded, or product-mismatched store
  proofs do not grant Diamond.
- Store verification success + Diamond grant failure is recoverable by
  backend retry.
- Android acknowledge/consume is idempotent and backend-owned.
- iOS transaction finish happens only after backend delivery succeeds.
- Pending purchases survive app restart and can be submitted again.
- Admin/support can inspect purchase history and failure reasons.

### Deliberate non-actions

- **No subscriptions / battle pass / no-ads** in Payments v1.
- **No external payment links** or alternate mobile payment providers.
- **No automatic Diamond clawback on refund** until product/support
  policy is explicitly decided.
- **No regional price logic in backend.** Storefronts own localized
  prices.
- **No RevenueCat/IAP aggregator initially.** Use official platform
  stores + backend verification; revisit only if operational cost
  outweighs control.
- **No web real-money checkout.** iOS/Android IAP only for this sprint.

### Slice ordering rationale

Foundation (P1) creates the ledger before any platform code. Product
catalog (P2) defines what a verified purchase is allowed to grant.
Verifier contracts (P3) isolate Apple/Google volatility before the
grant command. Verify + grant (P4) is the critical money path.
Post-processing/recovery (P5) closes the "paid but not delivered"
gap. Notifications (P6) reconcile out-of-band store changes. Frontend
(P7) comes only after backend semantics are fixed. P8 closes with
sandbox/internal-store manual verification and operations docs.

---

## Sprint M ✅ closed — Market Module

Goal: ship the **Market** bounded context — a single module owning
the SKU catalog and the purchase log, where players spend Diamond
to top up Energy / Hint / Undo / Reset inventories. First module
that synchronously charges Diamond at runtime — introduces the
`IDiamondGuard` sync gateway plus four new `IXxxGrant` gateways
(one per spendable inventory module), and a saga-light
compensating refund when the grant call fails after Diamond is
already debited. Categories carry an optional visibility window
for limited-time campaigns; SKUs carry an optional `Promotion` VO
plus `MaxStock` and per-player limits — the two patterns campaigns
typically need.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Module name | **Market** — single bounded context, schema `market`, microservice extraction candidate. |
| Aggregates | **3 in one module:** `Category` (admin-managed grouping + optional visibility window), `ShopItem` (SKU with price + promotion + stock + per-player limit), `PurchaseOrder` (idempotent purchase log). |
| Currency | **Diamond only.** No multi-currency, no real money, no localized pricing. Diamond is debited synchronously via the new `IDiamondGuard`. |
| Sync gateways | **6 new contracts.** `IDiamondGuard` (consume) + `IDiamondGrant` (refund), and `IEnergyGrant`, `IHintGrant`, `IUndoGrant`, `IResetGrant` for the buy targets. Existing `IEnergyGuard`/`IHintGuard`/`IUndoGuard`/`IResetGuard` (game-side consume) are unaffected. |
| `ItemType` enum | `Energy / Hint / Undo / Reset / Diamond` — Diamond reserved for future IAP, **not purchasable via Diamond currency in v1**. Buy handler rejects `ItemType.Diamond` with an explicit rule. |
| `Category.VisibilityWindow` | Optional `VO(StartsAt, EndsAt)`. Null = always visible. Window closed = entire category hidden from `/market/categories` and all its SKUs unbuyable. |
| `ShopItem.Promotion` | Optional `VO(PromoPrice, StartsAt, EndsAt)`. Domain rule `PromoPrice < Price`. Outside window the base `Price` is charged. **At most one Promotion per item** — no stacking. |
| Stock limit | `MaxStock?` (null = unlimited) + `SoldCount` counter. Domain rule `SoldCount < MaxStock`. EF Core `RowVersion` optimistic concurrency on the ShopItem row — concurrent buyers race; loser retries or fails. |
| Per-player limit | `PerPlayerLimit?` + `PerPlayerLimitWindow` enum (`Lifetime / Daily / PerPromo`). Buy handler counts purchases in window from `PurchaseOrder` and blocks if at/over limit. |
| Idempotency | `PurchaseOrder.IdempotencyKey` unique per `(PlayerId, IdempotencyKey)`. Duplicate `BuyShopItem` with the same key returns the existing order without re-charging. |
| Failure handling | **Saga-light compensating refund.** Buy flow: `IDiamondGuard.Consume` → target `IXxxGrant.Grant` (in try). On grant exception: `IDiamondGrant.Grant` refund + rethrow. Each module commits its own transaction; the refund is a separate cross-module call. |
| Audit | Both `Category` and `ShopItem` admin commands marked `IAdminCommand` with distinct `AuditTargetType`s. `PurchaseOrder` is **not** auditable — every purchase is its own outbox event. |
| Bundle SKUs | **None in v1.** Each `ShopItem` has exactly one `ItemType` and one `Quantity`. Composite/bundle SKUs spanning multiple inventory types are a separate future sprint. |

### Architecture notes

- **6 new sync gateway contracts** but only **5 new modules to
  modify** — `IDiamondGuard` and `IDiamondGrant` both go into
  `Diamond.Application` (its first sync-gateway contracts). `IEnergyGrant`/
  `IHintGrant`/`IUndoGrant`/`IResetGrant` each land in their
  respective module's `Application/Configuration/CrossModule/`,
  mirroring the existing `IXxxGuard` shape. API host
  `CrossModule/` ships 6 adapters consuming module commands. New
  granular ArchTest allows: `Market.Application` → each of the
  6 contracts.
- **Buy command is the saga.** A single command handler
  orchestrates three module boundaries (Diamond consume → target
  grant → Market commit). No `IPipelineBehavior`, no orchestration
  service — explicit try/catch inside the handler. Refund is a
  best-effort compensating `IDiamondGrant.Grant` in the catch
  block; the handler re-throws the original exception so the
  caller sees the failure.
- **No game-screen impact.** Market is invisible during gameplay;
  it's a separate HomeScreen entry / shop screen. Game module is
  untouched. No additions to the existing `IXxxGuard` (game-side
  consume) gateways — those keep checking Game-aggregate
  invariants during `UseHint`/`Undo`/`Reset`.
- **`ItemType.Diamond` reserved for future IAP.** The enum slot
  exists so Apple/Google IAP integration can drop in without an
  enum-reshape sprint, but the v1 buy command rejects it
  explicitly. The future IAP sprint will add a `BuyDiamondBundleCommand`
  with receipt verification — separate code path from `BuyShopItem`.
- **Stock-concurrency race is OK to lose.** `MaxStock = 1` items
  are inherently first-to-commit-wins. EF `RowVersion` makes the
  loser fail loudly rather than silently double-sell. Frontend
  treats the failure as "sold out" and refreshes the catalog.
- **PurchaseOrder is append-only.** No update path, no delete
  path. Refunds (compensating or admin-triggered) are recorded
  as separate `Diamond.GrantBonus` + `IXxxConsume` calls on each
  module's audit trail — Market doesn't model "cancelled" orders.
- **Audit-vs-outbox split.** Admin CRUD on Category/ShopItem flows
  through the same `AdminAuditingCommandHandlerDecorator` template
  the other modules use (10th per-module copy). Individual buys
  do **not** go through admin audit (they're player actions) but
  every `PurchaseOrder` is its own outbox `PurchaseCompletedIntegrationEvent`
  for downstream BI / notifications.
- **Empty PurchaseOrder index is the idempotency contract.** A
  unique index on `(PlayerId, IdempotencyKey)` enforces it at the
  DB level. The handler does a pre-check SELECT for friendly
  "duplicate, here's the existing order" responses; the unique
  index is the actual safety net under concurrency.

### Slice plan (8 slices — mirrors Sprint UR / Sprint D cadence)

| Slice | Content |
| --- | --- |
| **M1 ✅ delivered** | Market module foundation. 5 csproj (`Domain/Application/Infrastructure/Tests/IntegrationTests`). 3 aggregates: `Category` (`Id, Name, SortOrder, Icon?, IsActive, VisibilityWindow?`), `ShopItem` (`Id, CategoryId, ItemType, Quantity, Price, Promotion?, MaxStock?, SoldCount, PerPlayerLimit?, PerPlayerLimitWindow, IsActive, RowVersion`), `PurchaseOrder` (`Id, PlayerId, ShopItemId, ItemType, Quantity, DiamondsPaid, PurchasedAt, IdempotencyKey`). VOs: `VisibilityWindow`, `Promotion`. Enums: `ItemType {Energy, Hint, Undo, Reset, Diamond}`, `PerPlayerLimitWindow {Lifetime, Daily, PerPromo}`. Domain rules: `PromotionPriceMustBeLessThanPriceRule`, `WindowMustBeOrderedRule` (reused for Promotion + VisibilityWindow), `MaxStockMustBePositiveRule`, `ShopItemMustBeActiveRule`, `CategoryMustBeVisibleNowRule`, `ShopItemMustHaveStockRemainingRule`, `PlayerMustNotExceedShopItemLimitRule`, `DiamondSkusNotPurchasableInV1Rule`, `IdempotencyKeyMustBeUniqueForPlayerRule`. Aggregate events. EF mappings (RowVersion concurrency token on `ShopItem`). DbUp `market/Schema/001_CreateSchema.sql`, `Tables/010_Categories.sql`, `Tables/020_ShopItems.sql`, `Tables/030_PurchaseOrders.sql` (unique index on `(PlayerId, IdempotencyKey)`), `Tables/070_OutboxMessages.sql`. Autofac module + Startup + UoW + DomainEventsDispatcher + decorator chain (Logging/Validation/UnitOfWork) + outbox accessor. sln registration. ArchTests widen (Market Domain/Application/Infrastructure rules, aggregate naming, layer dependency, API composition-root boundaries). |
| **M2 ✅ delivered** | Six new cross-module sync gateways. `IDiamondGuard.EnsureDiamondAvailableAsync(playerId, amount)` + `IDiamondGrant.GrantAsync(playerId, amount)` in `Diamond.Application/Configuration/CrossModule/` (first sync-gateway contracts on Diamond — Sprint D deliberately had none). `IEnergyGrant`, `IHintGrant`, `IUndoGrant`, `IResetGrant` in respective modules' `Application/Configuration/CrossModule/`, each wrapping the existing `GrantBonus*` domain method via a module-internal command. API host `CrossModule/` adapters for all 6 — same shape as existing `*GuardAdapter`s. `Market.Application.csproj` → 6 granular ArchTest allows. |
| **M3 ✅ delivered** | `BuyShopItemCommand` + saga orchestration. Player endpoint `POST /market/items/{id}/buy` body `{ idempotencyKey }`. Handler flow: load ShopItem + Category → validate (active + category visible + stock remaining + per-player limit not exceeded + idempotency key unused + not `ItemType.Diamond`) → compute effective price (Promotion if window open else `Price`) → `IDiamondGuard.EnsureDiamondAvailableAsync(playerId, effectivePrice)` (debits diamond) → target `IXxxGrant.GrantAsync(playerId, item.Quantity)` inside try → on grant exception: `IDiamondGrant.GrantAsync(playerId, effectivePrice)` compensating refund + rethrow → on success: `ShopItem.RecordPurchase()` (`SoldCount++`, RowVersion bump) + `PurchaseOrder.Create(...)` + emit `PurchaseCompletedIntegrationEvent` → commit. Pre-flight SELECT on `(PlayerId, IdempotencyKey)` for friendly duplicate response; unique index catches the race. Player + admin endpoint test coverage in M7. |
| **M4 ✅ delivered** | Admin Category + ShopItem CRUD + audit. Six commands marked `IAdminCommand`: `CreateCategoryCommand`, `UpdateCategoryCommand`, `DeactivateCategoryCommand` (`AuditTargetType = "Market.Category"`); `CreateShopItemCommand`, `UpdateShopItemCommand`, `DeactivateShopItemCommand` (`AuditTargetType = "Market.ShopItem"`). 10th per-module copy of `AdminAuditingCommandHandlerDecorator`. `MarketAdminActionPerformedNotification` + handler publishes `AdminActionPerformedIntegrationEvent` through Market outbox. `Market.Infrastructure.csproj` → `Administration.IntegrationEvents` granular ArchTest allow. Promotion / stock / per-player limit are all mutable via `UpdateShopItemCommand` (single command updates the whole SKU). |
| **M5 ✅ delivered** | Player + admin GET endpoints. Player: `GET /market/categories` (currently-visible categories sorted by `SortOrder`, each with their currently-visible items including effective price + remaining stock + per-player remaining for caller), `GET /market/items/{id}` (single-item detail), `GET /market/orders/me` (caller's purchase history newest first, paged). Admin: `GET /admin/market/categories`, `GET /admin/market/items` (filter by category / item type / active), `GET /admin/market/items/{id}`, `GET /admin/market/orders/{playerId}` (per-player history for support). Program.cs wires `MapMarketEndpoints` + `MapAdminMarketEndpoints`. |
| **M6 ✅ delivered** | Frontend. `lib/features/market/` player shop feature with category tabs, item tiles showing promo badge + stock counter + per-player remaining + diamond price, buy confirmation modal, success snackbar, and post-buy inventory + diamond badge refresh when parent cubits are present. HomeScreen now has a "Market" entry. `lib/features/admin_market/` console ships category list + create/edit form with `VisibilityWindow` ISO fields; item list + create/edit form with Promotion price/window, MaxStock, PerPlayerLimit, and LimitWindow; plus per-player orders viewer. Routes wired: `/market`, `/admin/market/categories`, `/admin/market/items`, `/admin/market/orders`, `/admin/market/orders/:playerId`. |
| **M7 ✅ delivered** | Tests + manual verification. Market unit tests (per-aggregate + each rule). Market integration tests cover the active smoke path; operator manual verification exercised the Market/admin flows before close. One manual-test usability revision was folded into the frontend admin console: ShopItem creation now separates Normal vs Promotion setup, Normal hides campaign-only fields, Promotion Start/End use calendar pickers, and Save is disabled until required/valid fields are complete. Quality gate at close: Market unit tests 6/6, Market integration smoke 1/1, ArchitectureTests 61/61, Flutter tests 107/107. `flutter analyze` has no new Market-specific findings; only pre-existing info-level frontend warnings remain. |
| **M8 ✅ delivered** | Docs polish + sprint close. Updates: `activeContext.md > Active Sprint` pivots to "Sprint M closed", `progress.md` Sprint M entry with per-slice delivery notes, `GLOSSARY.md` (Market aggregates / VOs / rules, `BuyShopItem` saga flow, 6 new sync gateway contracts, compensating refund pattern, idempotency contract), `ROADMAP.md > Sprint M ✅ closed`, `frontendActiveContext.md` + `frontendProgress.md > Slice M6`. |

### Deliberate non-actions

- **No IAP / real-money integration.** Apple/Google receipt
  verification + platform sleeves are a separate future sprint.
  `ItemType.Diamond` is the forward-compat hook.
- **No bundle SKUs.** Each ShopItem is one ItemType × one
  Quantity. Composite bundles spanning multiple inventory types
  (e.g., "Starter Pack: 50 Energy + 10 Hint") are a future
  sprint — likely a new `BundleShopItem` aggregate sharing the
  buy command surface.
- **No admin "cancel order" / refund tool.** Manual refund =
  admin Grant on the affected inventory module + admin Grant
  on Diamond. Each side is audited; Market doesn't model
  cancellation.
- **No wishlist / favorites / shopping cart.** Single-item buy
  only. Cart UX would require a separate aggregate.
- **No multi-currency, no localized pricing, no regional SKUs.**
  Diamond is the universal price.
- **No promo stacking.** Each ShopItem carries at most one
  `Promotion`. Overlapping campaigns on the same item are an
  admin-policy problem, not a domain feature.
- **No purchase rate-limiting beyond `PerPlayerLimit`.** Global
  per-minute caps / abuse detection live in the API gateway
  layer if needed.
- **No analytics / BI reports.** Outbox `PurchaseCompletedIntegrationEvent`
  is the raw stream; downstream reporting is out of scope.
- **No frontend changes to Game screen.** Market is reachable
  from HomeScreen only; gameplay is uninterrupted.

### Slice ordering rationale

Foundation (M1) defines every domain shape the rest of the
sprint touches. Sync gateways (M2) precede the buy orchestration
(M3) because the handler depends on the 6 new contracts existing.
Admin CRUD (M4) before GET endpoints (M5) because GETs return
data the admin must create first. Frontend (M6) once backend is
feature-complete. Tests + manual verification (M7) gate the
sprint close (M8). Unlike Sprint D, there is **no destructive
cross-module reshape** in Sprint M — every existing module
gains a new sync gateway contract but no existing domain shape
changes. This keeps the merge surface narrow and the rollback
story simple (revert M1-M8 in any order without breaking other
sprints' invariants).

---

## Sprint D ✅ closed — Diamond Module + Quest 5-Reward

Goal: ship the 5th inventory module — **Diamond**, the in-game
currency that the future shop will charge against. Mechanically
identical to the existing inventory shape (Hint/Undo/Reset template),
but semantically different: Diamond is earned (quest rewards, future
game-completion bonus, future IAP) and spent (future shop checkout),
never invoked during gameplay invariants. Therefore **no sync
gateway** — Game module remains untouched.

This sprint extends the QuestReward shape from 4 fields to 5,
mirroring the Sprint H (2-reward) → Sprint UR (4-reward) progression.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Module name | **Diamond** — separate bounded context, schema `diamond`, microservice extraction candidate. |
| Sync gateway | **None.** Diamond is currency, not a gameplay invariant. Shop integration (future sprint) will be event-driven via outbox/inbox. |
| InitialBalance | **0** (configurable via `Diamond:InitialBalance`). Player earns Diamond through quest rewards, admin grants, and eventually game-completion bonus + IAP. |
| Max cap | **None.** Currency accumulates freely. `GrantBonus` permits any balance — same semantics as the Hint inventory. |
| Earn paths (phase 1) | Quest reward (5th reward field) + admin grant. Same reverse-event-dep template Hint/Undo/Reset use. |
| Earn paths (phase 2, deferred) | Game completion bonus (new reverse dep: `Diamond.Application → Games.IntegrationEvents.GameCompletedIntegrationEvent`), IAP. |
| Spend path | **None in this sprint.** Admin Set/Reset for testing only. Shop module is a separate sprint. |
| Quest reshape | **Destructive ALTER ADD COLUMN** with `DEFAULT 0` backfill — same pattern as Sprint H (2-reward) and Sprint UR (4-reward). No production data yet; local DB existing quest definitions keep their other rewards and inherit `DiamondReward=0`. |

### Architecture notes

- **5th inventory module.** Follows the Hint/Undo/Reset template
  exactly (lazy init via `PlayerRegisteredIntegrationEvent`, per-module
  Application contracts, per-module Autofac module + UnitOfWork +
  decorator chain + outbox).
- **5th outbox consumer for QuestClaimed.** Each reward type has its
  own consumer guarded on `reward > 0`. A Diamond outage doesn't block
  Energy/Hint/Undo/Reset reward delivery and vice versa.
- **Reverse cross-module event dep.** `Diamond.Application →
  Quests.IntegrationEvents.QuestClaimedIntegrationEvent`. Granular
  ArchTest allow — `Diamond.Domain` and `Diamond.Infrastructure`
  stay forbidden from any Quests namespace.
- **`PlayerDiamondInventory.GrantBonus` permits over-balance** — same
  rule-bypass shape as Hint and Energy bonus paths. Currency
  accumulation is intentional, not a defensive invariant.
- **No Game module changes.** Unlike Hint/Undo/Reset (which integrate
  via `IHintGuard`/`IUndoGuard`/`IResetGuard` sync gateways at
  `UseHint`/`Undo`/`Reset` time), Diamond never participates in
  gameplay path execution. The Game module is not aware Diamond
  exists.
- **Frontend HomeScreen 5 badges.** Existing `Wrap` from Sprint UR
  handles overflow on narrow screens.

### Slice plan (8 slices — mirrors Sprint UR cadence)

| Slice | Content |
| --- | --- |
| **D1 ✅** | Diamond module foundation. 5 csproj (Domain/Application/Infrastructure/Tests/IntegrationTests). `PlayerDiamondInventory` aggregate (Id == PlayerId), single `_balance`, no cap, no refill. 3 rules (`DiamondAmountMustBePositiveRule`, `DiamondAmountMustBeNonNegativeRule`, `DiamondBalanceMustBeSufficientRule`). 5 domain events (`Initialized`, `Consumed`, `Granted`, `AdminSet`, `AdminReset`). EF mapping, DbUp scripts (`diamond/Schema/001_CreateSchema.sql`, `Tables/010_PlayerDiamondInventories.sql`, `Tables/070_OutboxMessages.sql`). Autofac module + Startup + UoW + DomainEventsDispatcher + decorator chain (Logging/Validation/UnitOfWork) + outbox accessor. sln registration. ArchTests genişletilir (Diamond Domain/Application/Infrastructure rules, aggregate naming, per-layer dependency, API composition-root boundaries). |
| **D2 ✅** | Lazy init from `PlayerRegisteredIntegrationEvent`. `EnsurePlayerDiamondInventoryExistsCommand` + handler + validator (idempotent: existing row short-circuits). `IDiamondConfigurationService.InitialBalance` (default 0, configurable via `Diamond:InitialBalance`). `PlayerRegisteredIntegrationEventHandler` consumer. `Diamond.Application.csproj` → `Players.IntegrationEvents` granular ArchTest allow. |
| **D3 ✅** | **Destructive: Quest 5-reward.** `QuestDefinition._diamondReward` field. `Create(name, description, trigger, threshold, energyReward, hintReward, undoReward, resetReward, diamondReward, prereqId, progressBaseline)` 9-arg signature (was 8). `Update` mirror. `QuestRewardMustHaveAtLeastOnePositiveRule` widens to 5 fields (each ≥ 0, at least one > 0). `PlayerQuestClaimedDomainEvent` + `QuestClaimedIntegrationEvent` carry 5 reward fields. `PlayerQuest.Claim(now, ready, energy, hint, undo, reset, diamond)` 7-arg. New `GrantDiamondCommand` in Diamond.Application. New `QuestClaimedIntegrationEventHandler` in Diamond.Application (5th outbox consumer; guards on `DiamondReward > 0`). DbUp `quests/060_ExpandQuestRewardsWithDiamond.sql` — idempotent `ALTER TABLE ... ADD COLUMN "DiamondReward" int NOT NULL DEFAULT 0` with `information_schema` guard. Canonical `020_QuestDefinitions.sql` + `021_SeedQuestDefinitions.sql` updated for cold-start. Admin Quest commands (Create/Update) + validators + DTOs + endpoint requests reshape to 5 rewards. |
| **D4 ✅** | Admin operations + GET endpoints + audit. 3 admin commands marked `IAdminCommand` with `AuditTargetType = "Diamond.PlayerDiamondInventory"`: `SetPlayerDiamondCommand`, `GrantBonusDiamondCommand` (wraps internal `GrantDiamondCommand`), `ResetPlayerDiamondCommand`. `GetPlayerDiamondQuery` + handler + `PlayerDiamondSnapshotDto(PlayerId, Balance)`. 9th per-module copy of `AdminAuditingCommandHandlerDecorator`. `DiamondAdminActionPerformedNotification` + handler publishes `AdminActionPerformedIntegrationEvent` through Diamond outbox. `Diamond.Infrastructure.csproj` → `Administration.IntegrationEvents` granular ArchTest allow. API endpoints: `GET /diamond/me` (player), `GET /admin/players/{id}/diamond` + `POST .../set\|grant\|reset` (admin). Program.cs wires `MapDiamondEndpoints` + `MapAdminDiamondEndpoints`. |
| **D5 ✅** | Frontend reshape. `lib/features/diamond/` (player feature: `PlayerDiamond` DTO + `DiamondRepository` + `DiamondCubit` + `DiamondBadge`). HomeScreen 5th badge in the top-right row. `lib/features/admin_diamond/` console (lookup + set/grant/reset, mirroring `admin_hint`/`admin_undo`/`admin_reset`). `/admin/diamond` route + nav destination. Admin quest form 5 reward inputs (Enerji ⚡ + İpucu 💡 + Geri al ↶ + Sıfırla ↻ + Elmas 💎); form-level at-least-one-positive rule covers all 5. Player quest tile `Wrap` already handles 5 badges; just add the 5th. Game screen unchanged (Diamond not used in gameplay). |
| **D6 ✅** | Tests + quality gate. Diamond unit tests (19: aggregate Consume / GrantBonus / AdminSet / AdminReset + rule violations + Initialize). Diamond integration tests (12: lifecycle from PlayerRegistered, idempotency, consume success + empty rejection, QuestClaimed → bonus delivery, admin Set/Grant/Reset with audit row assertions). **Updated 4-reward fixtures to 5-reward:** Energy.IT, Hint.IT, Undo.IT, Reset.IT `QuestRewardDeliveryTests`; Quests.IT admin Quest tests; API.Tests `GetQuestsMe_FreshPlayer_LazilyReturnsSeededDaily`. `scripts/test.sh` registers Diamond.Tests + Diamond.IntegrationTests and passes the full local .NET gate. Games.IT free-hint expectations were updated to the post-UR1 model: `ResolveHints() == 0`, so every hint goes through `IHintGuard`. |
| **D7 ✅** | Flutter test updates + manual verification. 5 quest-area test fixtures reshape (add `diamondReward` field). 5 golden flows manually verified by operator: (1) single-reward quest claim × 5 (one per reward type), (2) mixed 5-reward quest (all 5 deliver), (3) admin Set/Grant/Reset on the new console with audit log assertion, (4) `Diamond:InitialBalance` config override produces non-zero starting balance for new guest, (5) Diamond inventory persists across reload + JWT refresh. |
| **D8 ✅** | Docs polish + sprint close. Updates: `activeContext.md > Active Sprint` pivots to "Sprint D closed", `progress.md` Sprint D entry with per-slice delivery notes, `GLOSSARY.md` (Diamond aggregate / events / rules, widened `QuestRewardMustHaveAtLeastOnePositiveRule`, `PlayerQuest.Claim` 7-arg, `QuestClaimedIntegrationEvent` 5-consumer fan-out), `ROADMAP.md > Sprint D ✅ closed`, `frontendActiveContext.md` + `frontendProgress.md > Slice D5`. |

### Deliberate non-actions

- **No shop module.** Diamond spend path is a separate bounded
  context (likely `Shop` module) — its own sprint after Sprint D
  closes.
- **No game-completion → Diamond bonus.** Adds a new reverse event
  dep (`Diamond.Application → Games.IntegrationEvents`) and a scoring
  formula coupling. Deferred to a small follow-up slice once Diamond
  baseline is stable.
- **No IAP / real-money integration.** Mobile-only platform sleeve,
  needs Apple/Google IAP receipt verification — out of scope for the
  inventory module itself.
- **No `IDiamondGuard` sync gateway.** Diamond is not consumed during
  any Game/Players/Stats invariant check. If a future feature
  requires synchronous spend (e.g., immediate unlock check at content
  load), revisit then.
- **No max balance cap.** Currency accumulates freely. Re-evaluation
  trigger: real product requirement to cap balance (e.g., anti-cheat
  rule).
- **No frontend changes to Game screen.** Diamond is invisible during
  gameplay; only HomeScreen badge + admin console + quest form
  surface it.

### Slice ordering rationale

Same ordering as Sprint UR: foundation → lazy init → destructive
quest reshape → admin + audit → frontend → tests → manual verify →
docs. The destructive Quest 5-reward must come before admin/audit
slice because the admin Quest endpoints already expose
`diamondReward` in their request shape and the audit decorator needs
the new domain event shape.

---

## Administration Module

Goal: ship the sixth backend module — a back-office bounded context that
owns admin users, their role, and a cross-module audit trail. The
module is the foundation for the admin frontend (quest catalog CRUD,
per-player energy edits, content management, etc.). Designed to be
extractable as a microservice without code changes to other modules.

This sprint lifts the previous **non-action: broad permission/UserAccess
module** rule (`activeContext.md`) because a real permission model is
now needed. The lift is bounded: a single `Admin` role, no permission
matrix, no UserAccess-style multi-tenant ceremony.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Module name | **Administration** — separate bounded context. Schema `administration`. Microservice extraction candidate. |
| Admin identity | **Separate `AdminUser` aggregate** (not a flag on `Player`). Admins do not need to be players; the two contexts (back-office user vs. game participant) are distinct. |
| Role model | **Single role: `Admin`** — `AdminUser.Role` is a value object but only one value exists today. Permission matrix is explicitly out of scope; revisit when a real granular permission requirement appears. |
| Quest catalog | **Data-driven** — hardcoded 4-quest catalog migrated to `quests.QuestDefinitions`. Admin CRUD requires it; hardcoded constants are tech debt under DDD when business reality says they are editable. |
| Audit log | **Cross-module via integration events** — every admin command in any target module raises `AdminActionPerformedIntegrationEvent`; Administration's inbox projects to `administration.AdminActionAudit`. Module independence preserved; microservice-extractable. |
| Authorization | **Sync gateway pattern** (same as `IEnergyGuard`) — each consumer module's Application declares `IAdminAuthorizationContext`; API host adapter calls Administration to resolve the current admin's role/identity. |
| Auth scheme | **JWT-issued admin tokens** — Production `ProductionJwt` with `role=Admin` claim; Development `DevelopmentBearer` recognizes admin GUIDs from the `administration.AdminUsers` table. Admin sign-in path is separate from player sign-in. |
| Audit content | **Before/after snapshot JSON + actor + target type/id + occurredOn** — granular enough for support work; not an event-sourced reconstruction. |

### Architecture notes

- **Strict module isolation preserved.** No new module references another
  module's Domain/Application/Infrastructure. Cross-module calls go
  through:
  - **Sync gateway** (`IAdminAuthorizationContext`) when an authorization
    decision must be made before mutating state.
  - **Integration events** (`AdminActionPerformedIntegrationEvent`,
    `AdminUserRegisteredIntegrationEvent`) for audit and downstream
    awareness.
- **Reverse dependency direction.** Players/Energy/Quests/Games consumer
  modules MAY reference `LexiLink.Modules.Administration.IntegrationEvents`
  (granular ArchTest allow), analogous to how Energy already references
  `Quests.IntegrationEvents`. Administration Domain/Application/Infrastructure
  remain forbidden in those modules.
- **Microservice extraction readiness.** Administration owns its own
  schema, outbox, inbox, repository, and EF context. Audit aggregation
  happens through the public integration event contract, not direct DB
  reads. The module can be lifted to a separate process with no
  changes to other modules.
- **Each target module owns its admin endpoints.** `/admin/quests/...`
  lives in `LexiLink.API.Modules.Quests` (or a sibling
  `AdminQuestEndpoints.cs`), guarded by the new `AuthenticatedAdmin`
  policy. Admin commands live in the target module's Application
  (`Quests.Application.Admin.*`). Administration does not orchestrate;
  it authorizes and audits.
- **Decorator-based audit emission.** Admin commands implement a marker
  interface `IAdminCommand`. Each target module's
  `Infrastructure/Configuration/Processing/` chain gets a new
  `AdminAuditingCommandHandlerDecorator` that captures actor + before/after
  + outcome and writes an outbox message for
  `AdminActionPerformedIntegrationEvent`. Same pattern as the existing
  `LoggingCommandHandlerDecorator`.
- **Why not a single shared decorator in Common?** Kamil rule:
  decorators are per-module to avoid command-handler bypass risk.
  Each module owns its admin auditing decorator; duplication is
  intentional.

### Slice plan (10 backend + 6 frontend slices)

Backend slices ship one-at-a-time, each behind its own quality gate
(`dotnet build LexiLink.sln`, `./scripts/test.sh`, DbUp apply).
Frontend slices follow once the relevant backend slice is green.

**B1 — Administration module foundation**

- `src/Modules/Administration/{Domain,Application,Infrastructure,IntegrationEvents,Tests,IntegrationTests}/` skeletons.
- `AdminUser` aggregate (`AdminUserId`, `Email` VO, `Role` VO,
  `AdminUserStatus` enum, registration event, base rules).
- Application contracts per-module (`IAdministrationModule`,
  `ICommand`, `IQuery`, `CommandBase`, `QueryBase`, `ICommandHandler`,
  `IQueryHandler`).
- Infrastructure: `AdministrationContext` (schema `administration`),
  `AdministrationStartup`, `AdministrationModule` (Autofac),
  `AdministrationAutofacModule`, outbox accessor, SQL connection
  factory, decorator chain (logging/validation/unit-of-work), domain
  events dispatcher.
- DbUp: `administration/Schema/001_CreateSchema.sql`,
  `administration/Tables/010_AdminUsers.sql`,
  `administration/Tables/070_OutboxMessages.sql`,
  `administration/Tables/080_InboxMessages.sql` (for B5 audit ingestion).
- Composition root: API host wires `AdministrationStartup`.
- ArchTests: new module covered by isolation, naming, and convention
  fixtures (matches Energy/Quests scope).

**B2 — Admin registration + bootstrap**

- `RegisterAdminUserCommand` (idempotent on email), email validator,
  rules (`AdminUserEmailMustBeUniqueRule`,
  `AdminUserEmailMustBeValidFormatRule`).
- Local dev seed mechanism (config-driven list of bootstrap admin
  emails inserted on startup if missing; production-safe by being
  driven by an environment-supplied list, not hardcoded).
- `LexiLink.Modules.Administration.IntegrationEvents` —
  `AdminUserRegisteredIntegrationEvent` (public contract).
- Unit + integration tests.

**B3 — Admin authentication**

- `POST /auth/admin/token` — external admin sign-in (initially
  `DevelopmentExternalToken`-style verifier; real provider deferred
  alongside the existing player Apple/Google deferral).
- `AuthenticatedAdmin` policy in API host, requires JWT `role=Admin`
  claim and a resolved active `AdminUser`.
- `DevelopmentBearer` extension: a bearer GUID matching an active
  `administration.AdminUsers.Id` resolves as an admin principal.
- `ExecutionContext`/`IExecutionContextAccessor` extension to carry
  `IsAdmin` and `AdminUserId` alongside the existing `PlayerId`.
- API auth smoke tests (admin token accepted, player token rejected
  for admin endpoints, missing token returns 401).

**B4 — Authorization sync gateway (`IAdminAuthorizationContext`)**

- Contract pattern: each consumer module's Application declares its
  own `IAdminAuthorizationContext` interface (per-module, like
  `IEnergyGuard`).
- API host adapter in `LexiLink.API/CrossModule/AdminAuthorizationContext.cs`
  resolves the interface by calling Administration's Application.
- ArchTests prevent cross-module Application/Domain leakage.
- First wiring exercised by a smoke admin endpoint:
  `GET /admin/whoami` returns the current admin's email + role.

**B5 — Audit infrastructure**

- `LexiLink.Modules.Administration.IntegrationEvents.AdminActionPerformedIntegrationEvent`
  (actor admin user id, action type, target type/id, payload JSON,
  occurred on).
- `IAdminCommand` marker interface in
  `Common.Application` (cross-cutting marker; the decorator stays
  per-module).
- `AdminAuditingCommandHandlerDecorator<TCommand>` template
  added per-module under
  `Modules/{X}/Infrastructure/Configuration/Processing/`.
  Each captures before/after via a module-specific projection.
- Administration inbox (`administration.InboxMessages`) +
  `AdminActionAuditProcessor` Quartz job that projects raw inbox
  rows into `administration.AdminActionAudit`.
- `GET /admin/audit` query endpoint (paged, filtered by actor and
  target).
- Integration test: an admin command in any target module produces an
  audit entry visible via the query.

**B6 — Quest catalog data-driven (Quests module)**

- New `QuestDefinition` aggregate in `Quests.Domain` (id, type,
  cadence, goal, reward amount, prerequisite id, active flag).
- `quests.QuestDefinitions` table; DbUp seed migrating the existing 4
  hardcoded definitions.
- `QuestCatalog` replaced by `IQuestDefinitionRepository`-backed
  service.
- All existing Quests behavior preserved by tests; integration tests
  reseed the same 4 definitions and re-run the existing flows.

**B7 — Quest admin operations (Quests module)**

- Admin commands (all `IAdminCommand`):
  `CreateQuestDefinitionCommand`,
  `UpdateQuestDefinitionCommand`,
  `DeactivateQuestDefinitionCommand`,
  `IssueQuestToPlayerCommand` (test/support tool),
  `ResetPlayerQuestCommand`.
- Endpoints under `/admin/quests/definitions` and
  `/admin/players/{playerId}/quests` (both `AuthenticatedAdmin`).
- Each command audited via the B5 decorator chain.

**B8 — Energy admin operations (Energy module)**

- Admin commands: `SetPlayerEnergyCommand` (override current to a
  specific amount), `GrantBonusEnergyCommand` (admin variant of the
  existing internal `GrantEnergyCommand`), `ResetPlayerEnergyCommand`
  (back to default).
- Endpoints: `POST /admin/players/{playerId}/energy/set`,
  `POST /admin/players/{playerId}/energy/grant`,
  `POST /admin/players/{playerId}/energy/reset`.
- Audited via the Energy admin decorator.

**B9 — Player admin operations (Players module)**

- `Player.Ban(reason, now)` / `Player.Unban(now)` domain methods +
  `Player.IsBanned` state + ban event.
- `BanPlayerCommand`, `UnbanPlayerCommand`,
  `GetPlayerAdminDetailQuery` (rich admin view).
- Endpoints: `GET /admin/players/search`,
  `GET /admin/players/{playerId}`,
  `POST /admin/players/{playerId}/ban`,
  `POST /admin/players/{playerId}/unban`.
- Banned players are rejected at the auth boundary
  (`AuthenticatedPlayer` policy denies tokens that map to banned
  players).

**B10 — Content admin operations (Games module)**

- The current unauthenticated `POST /categories`,
  `POST /links`, etc., move to `/admin/...` routes behind
  `AuthenticatedAdmin`. Their previous unprotected routes are removed.
- Admin commands for `Category.Update`, `Link.Update`,
  `Link.Activate`, `Link.Deactivate` (Domain methods exist for the
  Link lifecycle; new ones added where needed).
- Audited via the Games admin decorator.

**F1 — Admin login + session segregation (frontend)**

- Separate `/admin/login` route; admin session stored separately from
  player session to avoid accidental privilege carry-over.
- `AdminSessionCubit`, `AdminApiClient`.

**F2 — Admin shell + navigation**

- `AppAdminShell` with side nav (Quests, Players, Energy, Content,
  Audit) and `AuthenticatedAdmin`-gated routes.

**F3 — Quest catalog UI**

- List + create + edit + deactivate quest definitions.

**F4 — Player search + admin detail UI**

- Player search by handle/email, admin detail view with profile +
  stats + energy + active quests + ban controls.

**F5 — Energy admin UI**

- Set/grant/reset energy on a specific player.

**F6 — Audit view UI**

- Paged audit log with actor + target filters.

### Deliberate non-actions

- **No permission matrix.** Single `Admin` role until a concrete
  granular need surfaces. Re-evaluation trigger: a second admin
  responsibility (e.g., "support-only, no content edit") becomes a
  real ask.
- **No multi-tenant model.** LexiLink is single-tenant.
- **No event-sourced admin history.** Audit is a projection table,
  not a reconstructable event stream.
- **No admin domain in `Common`.** Module-owned Application/Domain
  per Kamil; `IAdminCommand` is the only `Common` symbol because it
  is a cross-cutting marker, not behavior.
- **No new sync gateway beyond `IAdminAuthorizationContext`.** Audit
  is event-driven; admin write commands are direct.
- **No frontend slice until its backend slice is green.** Audit and
  player ban UIs wait on B5 and B9.

---

## Game Options Selection ✅ closed 2026-05-17

Goal: oyun ekranı her zaman **tam 6 outgoing link** göstersin. Veritabanında
bir kelimenin 6'dan fazla outlink'i olabilir; backend deterministik bir 6'lı
alt küme seçsin.

### Decisions (locked 2026-05-16)

| Decision | Choice |
| --- | --- |
| Selection metric | **Pairwise common-neighbor sum** — seçilen 6 outlink'in kendi outlink kümeleri arasındaki ikili ortak-komşu sayılarının toplamı maksimize edilir |
| Previous link | **Always locked into the 6** — oyuncunun geri dönebilmesi için son adım her zaman dahil; simetri DB'de zaten var ama frontend tarafında "ne olursa olsun göster" garantisi şart |
| Algorithm | **Greedy** — densest-k-subgraph yaklaşımı; previousLinkId ile (varsa) tohumla, her iterasyonda mevcut sete olan toplam common-neighbor skoru en yüksek adayı ekle |
| Tie-break | **Fully deterministic** — skor DESC → degree DESC → LinkId ASC; aynı game state için her zaman aynı 6 |
| API shape | **New endpoint `GET /games/{id}/options`** — backend game state'i kendi içinde tarar (currentLinkId + path history → previousLinkId), frontend hesap yapmaz. Existing `GET /links/{id}/outgoing` admin/dataset araçları için olduğu gibi kalır |

### Files to touch

- `LexiLink.Modules.Games.Application/Games/GetGameOptions/` —
  `GetGameOptionsQuery`, `GetGameOptionsQueryHandler`, `OutgoingLinkSelector`
  (saf algoritma, internal static, kolay unit test).
- `LexiLink.Modules.Games.Infrastructure/...` — handler için Dapper SQL'leri
  (candidates + degree, pairwise common-neighbor matrix).
- `LexiLink.API.Modules.Games.GameEndpoints` — yeni endpoint
  (`AuthenticatedPlayer` policy).
- `src/Modules/Games/Tests/` — `OutgoingLinkSelector` unit testleri.
- `src/Modules/Games/IntegrationTests/` — `GetGameOptionsQueryHandler`
  + endpoint smoke (mini seed grafı).

### Edge cases

- `|candidates| ≤ 6` → hepsini dön (seçim atla).
- `previousLinkId = null` (ilk adım) → en yüksek pairwise skorlu çift ile
  tohumla, sonra greedy ile 6'ya tamamla.
- previousLinkId aday setinde değil (defansif; aktif olmayan link olabilir)
  → previousLinkId yokmuş gibi davran ve greedy ile 6 dön.
- Tüm pairwise skorlar 0 (seyrek graf) → degree DESC + LinkId ASC fallback.

### Non-goals

- `GameDetails`'a `Options: List<OutgoingLinkDto>` embed etmek (chatty
  round-trip için sonraki optimizasyon kararı).
- Tam 6-way intersection (NP-hard tam çözüm).
- Hint scoring veya hedefe yakınlık tabanlı seçim.
- Random çeşitlendirme (deterministik tie-break tercih edildi).

### Acceptance

- `GET /games/{id}/options` 6 outlink döner (veya tüm adaylar 6'dan azsa
  hepsini).
- previousLinkId her zaman dönen sette mevcut.
- Aynı game state için aynı 6, aynı sırada (test ile kilitli).
- `flutter analyze` + `flutter test` + backend test suite hâlâ yeşil
  (frontend dokunulmaz bu slice'ta).

Frontend Slice 11 (Game Screen Polish) bu endpoint geldikten sonra
`GameRepository.getOptions(gameId)` ile mevcut `getOutgoing(...)` çağrısını
değiştirir.

### Delivery summary (2026-05-17)

- **Application** — `GetGameOptionsQuery` (`QueryBase<List<OutgoingLinkDto>>`,
  `GameId`), `GetGameOptionsQueryHandler` (Dapper; aday + degree fetch,
  pairwise common-neighbor matrix, kategori-scoped BFS adjacency pull),
  `OutgoingLinkSelector` (saf algoritma, internal static, deterministik
  tie-break: score DESC → degree DESC → id ASC).
- **Previous link resolution** — `_history` start adımını tutmaz; handler
  history count'u sayar: 0 → previous yok, 1 → `Game.StartLinkId`, ≥2 →
  history[count-2]. Bu ayrım test (`GetGameOptions_AfterStep_PreviousLinkIsAlwaysIncluded`)
  ile kilitli.
- **Target reachability lock (post-ship revision)** — Selector ikinci bir
  lock alır: `pathToTargetLinkId`. Handler kategori-scoped adjacency
  üzerinde in-memory BFS ile `currentLinkId → targetLinkId` yolunun ilk
  hop'unu çözer ve selector'a iletir. Density heuristic'i artık target'a
  giden tek outlink'i sessizce drop edemez. Edge case'ler: `target ==
  current` → lock yok; `target` zaten candidate → kendisi lock; yol yoksa
  null. Test (`GetGameOptions_ReachabilityIsolatedLeaf_IsAlwaysIncluded`)
  ile kilitli.
- **API** — `GET /games/{id:guid}/options`, `AuthenticatedPlayer` policy,
  mevcut `GamesEndpoints` grubunun içinde.
- **Tests** — 13 yeni `OutgoingLinkSelectorTests` (greedy seed, previous
  lock, target lock, dual lock, locks-collide edge case, target-id not in
  candidates, tie-break determinism, degree fallback, pre-limit ordering,
  determinism), 5 yeni `GetGameOptionsIntegrationTests` (≤6 outlink
  pass-through, 8-leaf star → tam 6, step sonrası previous her zaman
  dahil, deterministic repeat, target reachability isolated leaf lock).
- **Quality gate** — `./scripts/test.sh`: 285/285 pass (önceki 267 + 18),
  0 warning.

### Verification

- `dotnet build LexiLink.sln` → 0 error, 0 warning.
- `./scripts/test.sh` → 11 test projects, **285 tests pass**.

---

## Quests Module ✅ closed 2026-05-15

Goal: ship the fifth module — daily and play-driven quests that grant rewards
(initially Energy). First module with reverse cross-module event dependency
(Energy listens to Quests' integration event).

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Module name | **Quests** |
| Reward delivery | **Event-driven async** — `QuestClaimedIntegrationEvent` (Quests outbox) → Energy module listener → `GrantEnergyCommand` |
| Claim model | **Explicit** — state machine `Active → ReadyToClaim → Claimed`; player must tap claim |
| Energy over-max | **Allow over-max temporarily** — bonus path does not cap; recharge timer stays idle while `current > max`; consume drains over-max first |

### Architecture notes

- **Reverse dependency direction.** Energy.Application will reference
  `Quests.IntegrationEvents` (public contract assembly) — analogous to how
  Stats.Application references Games/Players IntegrationEvents. Cross-module
  Domain/Application/Infrastructure stay forbidden via ArchTests.
- **No new synchronous gateway.** Energy stays the only sync cross-module
  gateway (`IEnergyGuard`, invariant check). Rewards are intent, not
  invariant — event-driven is the right pattern.
- **Energy domain change.** New `PlayerEnergy.GrantBonus(amount, now)` method
  that does NOT check `EnergyAmountCannotExceedMaximumRule`. Existing recharge
  math already no-ops when `current >= max`, so over-max naturally stays put.
  New unit tests cover the over-max consume edge cases: `10/5 → consume 1 →
  9/5 (timer not set)`, `6/5 → consume 1 → 5/5 (timer not set)`, `5/5 →
  consume 1 → 4/5 (timer set)`.

### MVP quest catalog (Slice 3, hardcoded)

| QuestType | Cadence | Goal | Trigger | Reward | Prerequisite |
| --- | --- | --- | --- | --- | --- |
| `FirstGameCompleted` | One-time | 1 game | `GameCompletedIntegrationEvent` | +3⚡ | — |
| `ThreeGamesCompleted` | One-time | 3 games | `GameCompletedIntegrationEvent` | +5⚡ | — |
| `AccountLinked` | One-time | 1 link | `AuthProviderLinkedIntegrationEvent` | +5⚡ | `ThreeGamesCompleted` claimed |
| `DailyThreeGames` | Daily (UTC midnight) | 3 games | `GameCompletedIntegrationEvent` | +5⚡ | — |

Daily reset is lazy: `_expiresAt = nextUtcMidnight` set at issue;
`ExpireIfPast(now)` is called when the quest is queried or progressed; the
catalog re-issues a fresh daily quest after expiry on the next relevant event.

### Slice plan (8 slices)

1. **Quests.Domain** — `PlayerQuest` aggregate, `QuestType` enum, `QuestState`
   enum, 5 rules
   (`QuestProgressDeltaMustBePositiveRule`,
   `QuestProgressCannotExceedGoalRule`,
   `QuestMustBeActiveToProgressRule`,
   `QuestMustBeReadyToBeClaimedRule`,
   `QuestGoalMustBePositiveRule`),
   3 events (`PlayerQuestIssuedDomainEvent`,
   `PlayerQuestCompletedDomainEvent`,
   `PlayerQuestClaimedDomainEvent`), `IQuestCatalog`,
   `IPlayerQuestRepository`. Unit tests.
2. **Quests.Application** — per-module CQRS contracts;
   `IssueQuestCommand` (idempotent),
   `RecordQuestProgressCommand` (no-op when no active quest),
   `ClaimQuestCommand`,
   `GetActiveQuestsQuery`,
   DTOs, validators.
3. **Quests.Infrastructure** — `QuestsContext`, repository, hardcoded
   `QuestCatalog`, decorators, outbox, Autofac module + composition root.
4. **Database** — DbUp scripts: `quests/Schema/001_CreateSchema.sql`,
   `quests/Tables/010_PlayerQuests.sql` (PK `Id`, indexes on
   `(PlayerId, State)` and `(PlayerId, QuestType)`),
   `quests/Tables/070_OutboxMessages.sql`,
   `quests/Views/110_v_PlayerQuests.sql`.
5. **Integration event handlers in Quests** —
   `IIntegrationEventHandler<GameCompletedIntegrationEvent>` and
   `IIntegrationEventHandler<AuthProviderLinkedIntegrationEvent>` issue and
   progress quests; prerequisites enforced; daily quest issuance is lazy.
   Integration tests with Players + Games + Quests modules booted together.
6. **Energy reward delivery** —
   `Quests.IntegrationEvents/QuestClaimedIntegrationEvent` (public contract);
   Energy.Application adds
   `IIntegrationEventHandler<QuestClaimedIntegrationEvent>`,
   `GrantEnergyCommand`, `PlayerEnergy.GrantBonus(amount, now)` domain method,
   `BonusAmountMustBePositiveRule`, over-max consume edge-case unit tests.
   ArchTest rule: Energy.Application MAY reference
   `LexiLink.Modules.Quests.IntegrationEvents` (granular allow).
7. **API endpoints** — `GET /quests/me` (authenticated; active + ready + last
   N closed quests, list DTO with progress/goal/state/reward/expiresAt),
   `POST /quests/{id}/claim` (transitions current player's quest to claimed
   if `ReadyToClaim`). API tests + live smoke.
8. **Documentation** — `GLOSSARY.md`, `CLAUDE.md`,
   `kamil-modular-monolith-comparison.md`, `ROADMAP.md`, `activeContext.md`,
   `progress.md`, `OPERATIONS.md` updates.

### Deliberate non-actions

- **No quest UI redesign per slice 7.** API contract sets the shape; frontend
  consumption is a separate (later) slice on the Flutter side.
- **No raw inbox in Quests.** Stays inline like Energy; revisit if a real
  duplicate/retry need surfaces.
- **No quest reward outbound integration events** beyond the single
  `QuestClaimedIntegrationEvent`. Issuance/completion stays internal to
  Quests.

### Delivery summary (2026-05-15)

All 8 slices delivered:

- **Domain** — `PlayerQuest` aggregate (keyed by its own `PlayerQuestId`), 5
  rules
  (`QuestGoalMustBePositiveRule`,
  `QuestRewardAmountMustBePositiveRule`,
  `QuestProgressDeltaMustBePositiveRule`,
  `QuestMustBeActiveToProgressRule`,
  `QuestMustBeReadyToBeClaimedRule`),
  3 events (`PlayerQuestIssuedDomainEvent`,
  `PlayerQuestCompletedDomainEvent`,
  `PlayerQuestClaimedDomainEvent`), `QuestType`/`QuestState`/`QuestCadence`
  enums, `QuestDefinition` record, `IQuestCatalog`, `IPlayerQuestRepository`.
- **Application** — per-module CQRS contracts; `IssueQuestCommand`
  (idempotent, prereq + cadence aware), `RecordQuestProgressCommand` (no-op
  when no active quest, clamps delta), `ClaimQuestCommand`,
  `GetActiveQuestsQuery` (Dapper + lazy expiry projection at read time).
- **Infrastructure** — `QuestsContext` (schema `quests`), repository,
  hardcoded `QuestCatalog` (4 MVP definitions), full module-owned UoW +
  domain event dispatcher + outbox accessor + decorator chain, Autofac
  module + composition root, `PlayerQuestClaimedDomainEventNotification`
  + publisher that emits `QuestClaimedIntegrationEvent` via `IEventsBus`.
- **Database** — DbUp scripts: `quests/Schema/001_CreateSchema.sql`,
  `quests/Tables/010_PlayerQuests.sql` (PK `Id`, indexes on
  `(PlayerId, State)` and `(PlayerId, QuestType)`),
  `quests/Tables/070_OutboxMessages.sql`,
  `quests/Views/110_v_PlayerQuests.sql`.
- **Integration event handlers (consumer side)** —
  `GameCompletedIntegrationEventHandler` issues + progresses
  FirstGameCompleted, ThreeGamesCompleted, and DailyThreeGames;
  `AuthProviderLinkedIntegrationEventHandler` issues + progresses
  AccountLinked (prereq enforcement inside `IssueQuestCommandHandler`).
- **IntegrationEvents assembly** —
  `LexiLink.Modules.Quests.IntegrationEvents` published `QuestClaimedIntegrationEvent`.
- **Energy reward delivery (reverse cross-module event dep)** —
  `PlayerEnergy.GrantBonus(amount, now)` permits over-max balance;
  `BonusAmountMustBePositiveRule`; `Consume` timer fix (only arms when
  consume crosses from at/above max to below max); `GrantEnergyCommand`
  + validator + handler in Energy.Application; Energy.Application's
  `QuestClaimedIntegrationEventHandler` defensively runs
  `EnsurePlayerEnergyExistsCommand` then `GrantEnergyCommand`. ArchTests
  added granular allow: Energy.Application MAY reference
  `LexiLink.Modules.Quests.IntegrationEvents` only.
- **API** — `GET /quests/me` and `POST /quests/{id:guid}/claim`
  (both require `AuthenticatedPlayer` policy); claiming another
  player's quest returns 404. API host wired through `QuestsStartup`.
- **Tests** — 23 Quests.Tests (unit), 5 Quests.IT (issue/progress/claim
  state machine + outbox processing), +7 Energy.Tests
  (GrantBonus + over-max consume edge cases), +2 Energy.IT
  (QuestClaimed → bonus delivery, lazy aggregate init under race),
  +5 API.Tests (Quests endpoints auth + happy/error paths),
  +3 ArchTests (Quests Domain/Application/Infrastructure rules,
  plus Quests added to other modules' forbidden lists, plus
  Quests.IntegrationEvents in the integration-events scan). Full
  quality gate: 267/267 green, 0 warnings.

### Verification

- `dotnet build LexiLink.sln` → 0 error, 0 warning.
- `./scripts/test.sh` → 11 test projects, **267 tests pass**.
- Local API smoke: `POST /players/guest` → `GET /quests/me` returns `[]`;
  `POST /quests/{random-id}/claim` returns 404 ProblemDetails;
  unauthenticated calls return 401.
- DbUp re-run idempotent (0 pending after first apply).

---

## Energy Module ✅ closed 2026-05-14

Goal: ship the first business module with a synchronous cross-module dependency
on Games. Players unlock energy on registration; starting a game consumes one
unit; energy refills lazily over time.

Delivered:

- **Domain** — `PlayerEnergy` aggregate (keyed by `PlayerEnergyId` = player
  Guid), 4 rules
  (`EnergyConfigurationMustBeValidRule`,
  `EnergyAmountCannotBeNegativeRule`,
  `EnergyAmountCannotExceedMaximumRule`,
  `EnergyMustBeSufficientToConsumeRule`),
  2 events (`PlayerEnergyConsumedDomainEvent`,
  `PlayerEnergyRefilledDomainEvent`), `IEnergyConfigurationService`,
  `IPlayerEnergyRepository`, `EnergyRefillCalculator` (pure-math projection,
  internal, reused by read and write paths). 16 unit tests.
- **Application** — per-module CQRS contracts; `EnsurePlayerEnergyExistsCommand`
  (idempotent init), `ConsumePlayerEnergyCommand`, `GetPlayerEnergyQuery`
  (lazy refill applied at read time via the shared math).
- **Infrastructure** — `EnergyContext` (schema `energy`), repository, config
  service backed by `IConfiguration` (`Energy:MaxAmount` /
  `Energy:RechargeIntervalSeconds` / `Energy:GameStartCost`; defaults 5 / 900
  / 1), full module-owned UoW + domain event dispatcher + outbox accessor +
  decorator chain, Autofac module + composition root.
- **Database** — DbUp scripts: `energy/Schema/001_CreateSchema.sql`,
  `energy/Tables/010_PlayerEnergies.sql`, `energy/Tables/070_OutboxMessages.sql`,
  `energy/Views/110_v_PlayerEnergies.sql`.
- **Cross-module wiring (Games → Energy)** — `IEnergyGuard` interface in
  `Modules/Games/Application/Configuration/CrossModule/`; adapter in
  `LexiLink.API/CrossModule/EnergyGuard.cs`; `StartGameCommandHandler` calls
  `_energyGuard.EnsureCanStartGameAsync(game.PlayerId)` before `game.Start()`;
  insufficient energy raises `BusinessRuleValidationException`. Architecture
  tests forbid Games.Application from referencing any Energy namespace.
- **PlayerRegistered consumer** —
  `IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>` in
  `Energy.Application` dispatches `EnsurePlayerEnergyExistsCommand` through
  `IEnergyModule`; the in-memory event bus delivers it after the Players
  outbox processor publishes.
- **API endpoint** — `GET /energy/me` (authenticated). Returns
  `PlayerEnergySnapshotDto` with current/max/`isFull`/recharge interval/last
  refill plus `secondsUntilNextRefill` and `fullyRefilledAt` projections.
- **Tests** — Energy unit tests (16), Energy integration tests (2: lifecycle +
  same-device idempotency), Games and Stats integration test bases gained an
  `AlwaysAllowingEnergyGuard` stub so they continue to exercise Games in
  isolation, ArchitectureTests gained Energy layer-dependency rules, API tests
  gained 3 `/energy/me` smoke tests.

Deliberate non-actions for this sprint:

- Skipped publishing public `PlayerEnergyConsumed` / `PlayerEnergyRefilled`
  integration events — no consumer needs them yet. The notification-wrapper
  scaffold is unused and can be added when a real consumer appears.
- Skipped the raw inbox pattern in Energy (the Stats pattern). Energy listens
  inline; revisit if duplicate-event or retry needs surface.
- Residual dual-write risk between Energy commit and `game.Start()` is
  documented in `StartGameCommandHandler` and the comparison doc, not fixed
  with a compensating action.

---

## Production Readiness Backlog ✅ baseline closed

Kamil alignment tamamlandıktan sonraki yürütülebilir sıra. Amaç artık mimari
benzerlik değil; LexiLink'i gerçek kullanıcı, gerçek identity, izlenebilir
background processing ve daha net API contract'ları için hazırlamak.

Closure status:

- Slices 16-21 are complete as the current production-readiness baseline.
- Deferred: real Apple/Google external token verifiers. This needs provider
  credentials/client configuration and should be added before public mobile
  release, not guessed locally.
- Deferred: warnings-as-errors/analyzer policy. Turn it on only after existing
  known EF materialization warnings are either removed or intentionally
  suppressed.
- Non-action: full schema diff tooling. The current DbUp journal readiness guard
  is enough unless schema drift becomes a recurring operational problem.
- Non-action: broad role/permission matrix and UserAccess-style module. The
  current `AuthenticatedPlayer` policy is enough until a real permission model
  appears.

Recommended next product phase:

1. **Game content/admin tooling** — category/link dataset import, validation,
   and repeatable content seeding.
2. **Gameplay product polish** — puzzle generation quality, difficulty tuning,
   hints, scoring, and session feel.
3. **Deployment packaging** — Docker/compose/env templates and release workflow,
   if hosting is the next concrete goal.

### Slice 16 — Production Auth / Identity

Goal: temporary bearer baseline'ını production path'ten ayırıp gerçek identity
doğrulama kararını ve guest-to-auth geçiş akışını kapatmak.

- [x] Current `LexiLinkBearer` scheme'i Development/Test-only baseline olarak
  sınırla veya açıkça non-production guard ile koru.
- [x] Production auth provider stratejisini seç: Apple token validation,
  Google token validation, first-party JWT issuing, veya bunların aşamalı
  kombinasyonu.
- [x] Players login/link flow'unun guest player -> authenticated player
  dönüşümünü kırmadan çalıştığını integration testlerle kilitle.
- [x] Command-level authenticated execution testleri ekle; sadece endpoint 401
  smoke testleriyle yetinme.
- [x] API config/env requirements dokümante et.

Current status:

- `Authentication:Mode=DevelopmentBearer` is now guarded: API startup fails in
  `Production` if this temporary mode is active.
- First production strategy is first-party signed JWT validation:
  `Authentication:Mode=ProductionJwt` validates issuer, audience, lifetime,
  HMAC signature, and GUID `sub`.
- API tests cover development bearer, production startup guard, valid
  production JWT, and wrong-signature JWT rejection.
- `POST /auth/token` issues first-party JWTs after external identity
  verification. The current verifier is `DevelopmentExternalToken` and is
  blocked in Production.
- Guest -> social identity transition is covered in Players integration tests.
- Command-level execution context propagation is covered by Players integration
  tests through the real command decorator pipeline.
- Slice 16 baseline is complete. Real Apple/Google verifier implementation is
  deferred until those provider credentials are available.

Non-goals:

- Geniş role/permission matrisi ekleme; gerçek permission ihtiyacı yoksa
  `AuthenticatedPlayer` policy baseline'ı yeterli.
- UserAccess benzeri ayrı module yaratma; identity behavior büyümeden ceremony
  ekleme.

### Slice 17 — Stats Feature Depth

Goal: Stats'i sadece projection plumbing göstergesi olmaktan çıkarıp product
değeri olan metriklerle güçlendirmek.

- [x] İlk feature'ı seç: daily/weekly leaderboard.
- [x] Period leaderboard read model'i ekle: daily/weekly aggregates, all-time
  leaderboard ile geriye uyumlu query contract.
- [x] Producer event payload'ları yeterli değilse minimal contract genişletmesi
  yap.
- [x] Stats projection/replay davranışını yeni metrik için test et.
- [x] Stats Domain layer ekleme kararını sadece gerçek invariant varsa ver.

Current plan:

- Existing `PlayerStats` remains the all-time read model.
- Added a Stats-owned period leaderboard read model keyed by period type, period
  start date, and player id.
- Extended `GET /stats/leaderboard` with `period=allTime|daily|weekly` and kept
  `orderBy`/`limit` behavior.
- Uses UTC calendar days and Monday-start UTC weeks. Optional `periodStart`
  is supported for deterministic historical period queries.
- No producer payload change was needed; `GameCompletedIntegrationEvent`
  already carries `OccurredOn`, `PlayerId`, and `Score`.
- Stats remains a projection/read-model module; no Domain layer was added.

Non-goals:

- Sırf modül yapısı tam görünsün diye Stats Domain project'i açma.

### Slice 18 — API Contract Hardening

Goal: public API davranışını daha deterministik, testli ve tüketilebilir hale
getirmek.

- [x] Request validation response shape'ini standardize et.
- [x] ProblemDetails/error response contract'ını endpointlerde tutarlı yap.
- [x] OpenAPI/Scalar output'unu auth ve error contract'ları açısından gözden
  geçir.
- [x] Kritik endpointler için smoke/integration coverage'ı genişlet.

Current status:

- Command validation failures now return `application/problem+json` with
  RFC7807 fields and an `errors` dictionary keyed by request/command property.
- API smoke coverage locks the validation problem shape for a protected command
  endpoint.
- Endpoint-level not-found responses now use a shared ProblemDetails helper.
- Business-rule failures are covered as 400 ProblemDetails with rule metadata.
- Conflict-style domain failures remain classified as 400 business-rule
  violations until a concrete domain rule needs a separate 409 API contract.
- OpenAPI now exposes the `LexiLinkBearer` bearer security scheme and applies
  it to protected operations while keeping anonymous operations unsecured.
- Protected endpoint groups advertise ProblemDetails responses for common
  400/401/404 paths. Auth token exchange advertises 401/404 ProblemDetails.
- API smoke coverage verifies the generated `/openapi/v1.json` auth and
  ProblemDetails metadata.
- Slice 18 baseline is complete.

### Slice 19 — Operational Readiness

Goal: API ve async processors production'da izlenebilir, teşhis edilebilir ve
sağlık kontrolü yapılabilir hale gelsin.

- [x] Health checks: API liveness/readiness, database connectivity.
- [x] Outbox/inbox/internal-command backlog ve poison/error visibility için
  admin query veya structured log standardı.
- [x] Processor job failure logging ve correlation bilgisini gözden geçir.
- [x] Configuration defaults ve required env vars dokümante et.

Current status:

- API now exposes anonymous `/health/live` and `/health/ready` endpoints.
  Readiness includes PostgreSQL connectivity through a direct `SELECT 1`
  check, and both endpoints return a small JSON health report.
- API now exposes protected `/operations/processors` visibility for
  `games-outbox`, `players-outbox`, `stats-inbox`, and
  `stats-internal-commands`. The response includes unprocessed, ready,
  scheduled retry, poisoned, failed counts, oldest unprocessed timestamp, and a
  capped recent error sample.
- Quartz jobs now create a background `CorrelationId` and log structured
  `BackgroundJob`, `ProcessorQueue`, `ProcessorType`, Quartz fire/trigger
  metadata around start, completion, and failure paths.
- `docs/OPERATIONS.md` now documents required production env vars, development
  defaults, auth modes, background processor settings, health/operations
  endpoints, and DbUp migration execution.
- Slice 19 baseline is complete.

### Slice 20 — Database Hygiene

Goal: PostgreSQL + DbUp tercihini koruyarak query/index ve migration operasyonu
tarafını güçlendirmek.

- [x] Critical query/index review: Players auth lookup, Games puzzle/link
  traversal, Stats leaderboard.
- [x] DbUp migration runbook: fresh database, existing database, rollback/manual
  recovery yaklaşımı.
- [x] Lightweight migration drift validation: API artifact'teki DbUp script
  manifest'i `public.MigrationsJournal` ile readiness içinde karşılaştırılır.

Current status:

- Players auth lookup already has a unique `(Provider, ExternalId)` index on
  `players.PlayerAuthIdentities`; no query/index change needed there.
- Games completed-pair lookup already uses
  `IX_Games_PlayerId_CategoryId_State_StartLinkId_TargetLinkId`.
- Added `IX_Links_CategoryId_IsActive_Id` for active category link selection
  during puzzle creation.
- Added all-time and period leaderboard indexes for `BestScore`, `TotalScore`,
  and `GamesCompleted` ordering paths.
- DbUp applied the 3 new index scripts locally; Games integration, Stats
  integration, and Architecture test suites pass.
- `docs/OPERATIONS.md` now covers fresh database migration, existing database
  migration, failure recovery, and forward-only rollback policy.
- `/health/ready` now includes a lightweight DbUp journal validation check.
  LexiLink intentionally avoids a full schema diff unless drift becomes a
  recurring operational problem.
- Slice 20 baseline is complete.

### Slice 21 — Release Smoke Gate

Goal: deployment öncesi en küçük production-mode doğrulama komutunu eklemek.

- [x] Local smoke script: API build, DbUp migration, production-mode API start.
- [x] HTTP health verification: `/health/live` and `/health/ready`.
- [x] Operations doc'a smoke command ve override env vars ekle.

Current status:

- `scripts/smoke.sh` local PostgreSQL'e karşı migration'ı uygular, API'yi
  `ASPNETCORE_ENVIRONMENT=Production` ve `Authentication:Mode=ProductionJwt`
  ile başlatır, sonra live/ready endpointlerini gerçek HTTP üzerinden kontrol
  eder.
- Production Readiness Pass baseline is complete.

---

## Kamil Alignment Backlog ✅ closed

Bu bölüm, `docs/kamil-modular-monolith-comparison.md` karşılaştırmasından çıkan
uygulama planıdır. API endpoint dağıtım stili ve PostgreSQL/DbUp tercihi
bilinçli sapma olarak kapsam dışıdır; bu iki başlık için refactor
planlanmıyor.

### Already Done

- [x] Architecture tests baseline: layer dependency, module boundaries, domain
  conventions.
- [x] API -> module facade dispatch: endpoint'ler doğrudan `ISender` kullanmaz.
- [x] Module startup APIs: host, module wiring detaylarını doğrudan bilmez.
- [x] Integration event contracts: Games/Players public `IntegrationEvents`
  assemblies.
- [x] Outbox -> Stats projection path: producer domain notifications,
  integration events, Stats projection, idempotency.
- [x] Stats read surface: player stats and leaderboard.
- [x] Shared-container hardening: module-owned UnitOfWork/domain dispatcher.
- [x] Serial integration test runner: `scripts/test.sh`.
- [x] Central build/package baseline: `Directory.Build.props`,
  `Directory.Packages.props`.
- [x] Application convention ArchTests: internal handlers/validators,
  immutable requests, module handler contracts.
- [x] CI quality gate: GitHub Actions workflow runs restore, build, DbUp
  migrations, then `scripts/test.sh`.
- [x] Auth/authorization baseline: API auth middleware, authenticated-player
  policy, protected endpoint groups, anonymous register/login lookup exceptions.
- [x] Outbox scheduling hardening: Quartz hosted scheduler, retry metadata,
  persisted errors, delayed retry eligibility.

### Slice 8 — CI Quality Gate ✅ done

Goal: local quality gate ile CI aynı şeyi çalıştırsın.

- [x] Add CI workflow.
- [x] Restore once, then run build/test without accidental parallel integration
  DB cleanup races.
- [x] Use `scripts/test.sh --no-restore -v minimal` or an equivalent serialized
  integration-test sequence.
- [x] Document required services/secrets/env vars, especially the integration
  Postgres connection string.
- [x] CI status should fail on build, ArchTests, unit tests, or integration
  tests.

Non-goals:

- Do not introduce Nuke yet.
- Do not turn warnings-as-errors on until known EF materialization warnings are
  either fixed or intentionally suppressed.

### Slice 9 — Authentication / Authorization Baseline ✅ baseline done

Goal: public API exposure before real auth should not happen.

- [x] Define auth boundary: current baseline accepts `Authorization: Bearer <player-guid>`.
- [x] Populate `IExecutionContextAccessor.UserId` from authenticated requests.
- [x] Add API authentication middleware and route protection.
- [x] Introduce permission/policy model inspired by Kamil's UserAccess module,
  but keep it minimal for current Players/Games needs.
- [x] Add API tests for anonymous root access and protected endpoint rejection.
- [x] Add command-level integration tests for authenticated execution once token
  issuing/real external auth is introduced.
- [x] Decide how guest players transition into authenticated players without
  breaking existing `Players` invariants.

Non-goals:

- Do not add broad role/permission tables before there is a real permission
  matrix.
- Do not move player context into Domain unless a domain invariant needs current
  user state.
- Do not treat the temporary `Bearer <player-guid>` scheme as production-grade
  identity verification.

### Slice 10 — Outbox Scheduling And Retry Hardening ✅ done

Goal: replace the simple hosted polling loop with a module-owned durable
processing model.

- [x] Introduce scheduled outbox processing per producer module.
- [x] Add retry/backoff metadata or retry policy.
- [x] Persist processing errors in a queryable form.
- [x] Keep per-message failure isolation; one bad message must not stop the
  batch.
- [x] Add tests for successful batch, partial failure, retryable failure, and
  poison-message behavior.

Options:

- Quartz-style scheduler, close to Kamil.
- A lighter scheduler is acceptable only if it provides clear retry/error
  semantics and can run safely in one API process.

Current status:

- Retry/error tracking is implemented.
- Hosted polling loop is replaced by Quartz hosted scheduling.

### Slice 11 — Raw Inbox Pattern For Consumers ✅ done

Goal: Stats should be able to store incoming integration events before
processing, instead of relying only on inline projection idempotency.

- [x] Add serialized `InboxMessages` shape with event type, payload, occurred
  date, processed date, and error state.
- [x] Change integration-event consumption path to append inbox messages first.
- [x] Add inbox processing command/job for Stats.
- [x] Preserve idempotency by integration event id.
- [x] Add replay-oriented tests: duplicate event, failed processing then retry,
  and projection rebuild from inbox.

Non-goal:

- Do not introduce this for every module before there is at least one real
  consumer path beyond Stats.

Current status:

- Stats integration handlers now append raw serialized inbox messages.
- API Quartz hosts a Stats inbox processing job next to outbox processing.
- Duplicate publish and failed-message continuation/retry metadata are covered
  in Stats integration tests.

### Slice 12 — Internal Commands ✅ done

Goal: support delayed/retried side effects without doing them inside request
transactions.

- [x] Add module-owned `InternalCommands` storage.
- [x] Add command scheduler abstraction.
- [x] Add processing job with retry/error persistence.
- [x] Wire a small real use case only when one exists, such as notification,
  token cleanup, or scheduled projection maintenance.
- [x] Add architecture tests for internal command visibility and constructor
  conventions once the pattern exists.

Non-goal:

- Do not add internal commands as ceremony without a delayed/retried side effect
  to host.

Current status:

- Stats owns `stats.InternalCommands`, `IStatsInternalCommandScheduler`, and
  `IStatsInternalCommandProcessor`.
- The API Stats inbox Quartz job now schedules `ProcessStatsInboxCommand` and
  runs the internal command processor instead of calling the inbox processor
  directly.
- Stats integration tests cover failed internal command retry metadata and
  valid command continuation.

### Slice 13 — Event Bus Abstraction ✅ done

Goal: decouple public integration contracts from MediatR so module extraction or
external broker adoption is possible later.

- [x] Introduce `IEventsBus` or equivalent abstraction.
- [x] Move integration event publication behind that abstraction.
- [x] Keep in-process implementation initially.
- [x] Remove or isolate direct `INotification` coupling from public integration
  event contracts if external bus readiness becomes a real goal.
- [x] Add architecture test that IntegrationEvents projects do not depend on
  in-process transport implementation details.

Non-goal:

- Do not add RabbitMQ/Kafka/Azure Service Bus before process separation is a
  real requirement.

Current status:

- `IIntegrationEvent` is a transport-neutral contract and no longer inherits
  MediatR `INotification`.
- `IEventsBus` + `IIntegrationEventHandler<T>` exist in Common Application, with
  an in-process `InMemoryEventsBus` implementation in Common Infrastructure.
- Games/Players integration-event publication now goes through `IEventsBus`;
  Stats consumers are registered as `IIntegrationEventHandler<T>`.

### Slice 14 — Module Composition Isolation ✅ done

Goal: reduce shared-container coupling if runtime registration conflicts return.

- [x] First document current shared-container tradeoffs and known protected
  registrations.
- [x] Prototype per-module composition root or per-execution lifetime scope for
  one module.
- [x] Verify command decorators, notification handlers, outbox mapping, and
  module facade dispatch still behave correctly.
- [x] Migrate other modules only if the prototype removes real complexity or
  prevents concrete bugs.

Non-goal:

- Do not rewrite the whole host composition root just to match Kamil
  aesthetically.

Current status:

- No new runtime registration collision justified a per-module container rewrite.
- The in-process event bus was changed from singleton to scoped/lifetime-scope
  so integration-event handlers resolve from the current execution scope.
- Architecture composition tests now guard that shared container does not expose
  module-owned `DbContext`, `IUnitOfWork`, domain dispatcher, or outbox services
  as ambiguous common services.
- Per-module container extraction remains deferred until a concrete runtime
  collision returns.

### Slice 15 — Time Abstraction ✅ done

Goal: remove direct `DateTime.UtcNow` from code paths where time is a domain
decision, not just persistence metadata.

- [x] Inventory `DateTime.UtcNow` usages.
- [x] Classify each usage as domain decision, application orchestration, or
  infrastructure timestamp.
- [x] Introduce a clock abstraction only where tests or domain policy need it.
- [x] Update tests for time-dependent rules.

Non-goal:

- Do not mechanically replace every timestamp call if it does not improve
  testability or domain clarity.

Current status:

- `IClock` lives in Common Application and `SystemClock` in Common
  Infrastructure.
- Players registration/link command handlers use `IClock` for domain-visible
  timestamps.
- Outbox, Stats inbox, and Stats internal-command processors use `IClock` for
  processing/retry timestamps.
- Direct production `DateTime.UtcNow` remains only in `SystemClock` and
  `DomainEvent` occurrence metadata; integration tests still use direct dates
  to generate test events.

### Planned Kamil Alignment Complete

The remaining differences are deliberate non-actions or need-triggered items,
not active alignment slices.

### Deliberate Non-Actions

- API endpoint style: keep current Minimal API/module endpoint organization.
- Database provider/tooling: keep PostgreSQL + DbUp + first-class SQL scripts.
- Kamil decorator split: do not reintroduce unless Autofac/MediatR resolution
  path is proven safe in this repo.
- Event sourcing: not planned unless audit/replay requirements become real.
- Implicit project references via `Directory.Build.targets`: defer; explicit
  project references are still easier to audit at current repo size.

---

## Sprint 1 — Domain Hardening ✅ closed 2026-05-02

Goal: bring the Games module's Domain layer to production-ready quality.

Outcome: 3 aggregates (Category, Link, Game) with explicit state machines, 18 business rules, 17 domain events, immutable allowance VOs, four domain service interfaces (one with a `StandardScoreCalculator` default impl in Domain), zero `InvalidOperationException` calls in domain code.

Detailed delivery in `progress.md`.

---

## Sprint 2 — Application Layer ✅ closed 2026-05-03

Goal: make the Domain operable. Wire MediatR, write commands/queries, hook cross-cutting decorators (Kamil-style, Autofac-registered), fill in the read side with Dapper.

### Common.Application

- [x] `NotFoundException` with `EntityName` + `Id` properties.
- [x] `InvalidCommandException` with `List<string> Errors` (thrown by `ValidationCommandHandlerDecorator`).
- [x] `ISqlConnectionFactory` interface (Dapper read side).
- [x] `IUnitOfWork` placement resolved in Sprint 3 — moved Common.Domain → Common.Infrastructure (Kamil-faithful).

### Common.Infrastructure — Domain event dispatch

- [x] `DomainEventsAccessor` — pulls domain events from EF `ChangeTracker.Entries<Entity>()`.
- [x] `DomainEventsDispatcher` — resolves `IDomainEventNotification<>` per event type, publishes via MediatR, serializes notifications into `OutboxMessage`s.
- [x] `DomainEventsDispatcherNotificationHandlerDecorator<T>` — runs `DispatchEventsAsync` after each notification handler so cascade events also dispatch.
- [x] `DomainNotificationsMapper` + `BiDictionary<string, Type>` — bidirectional map for outbox `Type` ↔ runtime type.
- [x] `Serialization/AllPropertiesContractResolver` — Newtonsoft contract resolver that includes private/internal members (so notification payloads serialize fully).

### Modules/Games/Infrastructure/Configuration/Processing/ — Command-handler decorators (Kamil-style, per-module)

Cross-cutting concerns are `ICommandHandler<T>` / `ICommandHandler<T, TResult>` decorators, **not** MediatR `IPipelineBehavior<,>` and **not** in Common. Rationale: `INotificationHandler<T>` (used for in-process domain event dispatch) has no pipeline equivalent, so the notification side must be decorator-based — keeping commands on the same pattern means one mental model. The decorator's generic constraint (`where T : ICommand`) targets Games' own per-module `ICommand`, which forces the decorator to live per-module — exactly Kamil's `Modules/{X}/Infrastructure/Configuration/Processing/` layout.

For each concern, two generics are needed: one for `ICommand` (void return) and one for `ICommand<TResult>` (result-returning commands like `CreateGameCommand : CommandBase<Guid>`, `UseHintCommand : CommandBase<HintResultDto>`).

- [x] `UnitOfWorkCommandHandlerDecorator<T>` — `ICommand` (void) variant; calls `IUnitOfWork.CommitAsync` after the inner handler.
- [x] `UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>` — `ICommand<TResult>` variant.
- [x] `LoggingCommandHandlerDecorator<T>` + `LoggingCommandHandlerWithResultDecorator<T, TResult>` — Serilog-based, Kamil-faithful (LogContext.Push + CommandLogEnricher with `Context = "Command:{Id}"`, try/catch around inner handler). **Deferred:** `RequestLogEnricher` + `IExecutionContextAccessor` (needs Sprint 4 HTTP host) and `IRecurringCommand` short-circuit (needs InternalCommands infra, Beyond Sprint 5). Slot left open in the structure.
- [x] `ValidationCommandHandlerDecorator<T>` + `ValidationCommandHandlerWithResultDecorator<T, TResult>` — FluentValidation, Kamil-faithful: collects `IList<IValidator<T>>`, runs `.Validate(command)` per validator, throws `InvalidCommandException` (in `Common.Application/Exceptions/`) carrying `List<string> Errors` if any rule fails.
- [ ] Autofac registration: `RegisterGenericDecorator(...)` with explicit ordering (logging → validation → unit-of-work → inner handler).
- [ ] Per-aggregate FluentValidation `AbstractValidator<T>` classes — written alongside each command (per Kamil's per-module-per-command pattern).

### Games.Application — Configuration ✅ done

- [x] `Configuration/Commands/`: `ICommand`, `ICommand<TResult>`, `ICommandHandler<>`, `ICommandHandler<,>`, `CommandBase`, `CommandBase<TResult>`. Per-module — not in Common.
- [x] `Configuration/Queries/`: `IQuery<TResult>`, `IQueryHandler<,>`, `QueryBase<TResult>` (carries `Id` for log correlation).

### Games.Application — Links ✅ done

- [x] Commands: `CreateLink`, `AddOutgoingLink`, `RemoveOutgoingLink`, `ActivateLink`, `DeactivateLink`.
- [x] Queries: `GetLinkDetails` (with `LinkDetailsDto`), `GetLinksByCategory` (with `LinkListItemDto`), `GetLinkOutgoingLinks` (with `OutgoingLinkDto`).

### Games.Application — Categories (Create/Edit/List/Details done)

- [x] Commands: `CreateCategory`, `EditCategory`.
- [x] Queries: `GetCategoryDetails` (with `CategoryDetailsDto` including `LinkCount`), `GetCategories` (with `CategoryListItemDto`).
- [ ] Decide: `Activate`/`Deactivate` on Category? (Currently no — Category is owner of Links; deactivating would cascade. Open question.)

### Games.Application — Games ✅ done

Resolved decisions:
- `Game.UseHint()` changed from `void` → `HintResult`; handler projects to `HintResultDto`.
- `IGameConfigurationService` left as 5 separate `Resolve*` calls (no consolidation needed — `ResolveMaxSteps` depends on `puzzle.Depth` which isn't known until after `Puzzle.Create`).
- `ILinkRepository.GetActiveIdsByCategoryAsync` added for puzzle generation.

Commands (7):
- [x] `CreateGameCommand` (returns `Guid`)
- [x] `StartGameCommand`
- [x] `MakeStepCommand`
- [x] `UseHintCommand` (returns `HintResultDto`)
- [x] `UndoCommand`
- [x] `ResetCommand`
- [x] `AbandonGameCommand`

Queries (1):
- [x] `GetGameByIdQuery` — `QueryMultipleAsync` against `[Games].[v_Games]` (JOIN `v_Links` ×3 for Start/Target/Current word denormalization) + `[Games].[v_GameHistory]` (JOIN `v_Links`) → `GameDetailsDto`.

DTOs:
- [x] `GameDetailsDto` — flat fields + `IReadOnlyList<GameHistoryStepDto> History` (positional record + one `init` slot for the nested collection; handler does `dto with { History = history }`).
- [x] `HintResultDto(HintType Type, Guid RecommendedLinkId)`.
- [x] `GameHistoryStepDto(int StepNumber, Guid LinkId, string LinkValue)`.

---

## Sprint 3 — Games.Infrastructure ✅ closed 2026-05-04

Goal: persist the write model with EF Core (PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`), implement the domain services that need I/O. Pattern: birebir Kamil-faithful — Postgres is the only deviation.

### Common.Infrastructure (Kamil's BuildingBlocks/Infrastructure equivalents)

- [x] `IUnitOfWork` moved from `Common.Domain` → `Common.Infrastructure` (Kamil-faithful). Decorator imports updated.
- [x] `UnitOfWork : IUnitOfWork` — injects `DbContext` + `IDomainEventsDispatcher`; `CommitAsync` calls `DispatchEventsAsync()` then `_context.SaveChangesAsync(ct)`. Birebir Kamil.
- [x] `TypedIdValueConverter<TTypedIdValue>` — `TypedIdValueBase` ↔ `Guid` value converter (Andrew Lock pattern).
- [x] `StronglyTypedIdValueConverterSelector` — auto-applies `TypedIdValueConverter` to any property whose type derives from `TypedIdValueBase`.
- [ ] `internalCommandId` parameter on `IUnitOfWork.CommitAsync` — deferred until InternalCommands infrastructure (Beyond Sprint 5).

### DbContext

- [x] `GamesContext : DbContext` at `Modules/Games/Infrastructure/`, with `DbSet<Category>`, `DbSet<Link>`, `DbSet<Game>`. Ctor `(DbContextOptions, ILoggerFactory)`. `OnModelCreating` → `ApplyConfigurationsFromAssembly`.
- [x] Schema: `games` (Postgres lowercase convention; same name as the module).
- [ ] Composition root wires up `UseNpgsql(...)`, `ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>()`, `UseSnakeCaseNamingConvention()` decision (Sprint 4).

### Fluent API configuration

- [x] `CategoryEntityTypeConfiguration` — `_name`, `_description` backing field access; table `Categories` in schema `games`.
- [ ] `LinkEntityTypeConfiguration` — `_outgoingLinks` collection mapping (JSON column or owned collection — decide); `_isActive` backing field.
- [ ] `GameEntityTypeConfiguration` — owned-type mapping for `Puzzle`, `HintAllowance`, `UndoAllowance`, `ResetAllowance`, `Score`, `StepBudget`; `_history` collection mapping.
- [ ] `UsePropertyAccessMode(PropertyAccessMode.Field)` on every aggregate — resolves all CS8618 warnings.

### Repositories

- [x] `CategoryRepository : ICategoryRepository` — `internal`, injects `GamesContext`, methods `GetByIdAsync`, `AddAsync`. No `Commit()` on the repository — UoW decorator handles `SaveChanges`.
- [ ] `LinkRepository : ILinkRepository`
- [ ] `GameRepository : IGameRepository`

### Domain service implementations

- [ ] `PathFinderService : IPathFinderService` — BFS for `FindOptimalPath`; weighted variant for `FindTarget` at depth.
- [ ] `LinkNeighborResolver : ILinkNeighborResolver` — wraps `ILinkRepository`.
- [ ] `GameConfigurationService : IGameConfigurationService` — config-driven (Easy/Medium/Hard → depth range, step budget, allowance counts).

### Read-side views

- [ ] `[Games].[v_Categories]`, `[Games].[v_Links]`, `[Games].[v_Games]`, `[Games].[v_GameHistory]`.
- [ ] `LinkOutgoingLinks` join table view for `GetLinkOutgoingLinks`.

### Migrations

- [ ] Initial migration: tables, views, indices.
- [ ] CS8618 warning count: 0.

---

## Sprint 4 — API Host ✅ closed 2026-05-05

Goal: boot the system end-to-end with HTTP endpoints.

- [ ] `LexiLink.API` ASP.NET Core project.
- [ ] `IModule` / `ModuleStartup` pattern — each module registers its own DI, MediatR scan, FluentValidation.
- [ ] `GamesModule` startup wiring all of Games.Application + Games.Infrastructure.
- [ ] Endpoints (Minimal API or Controllers — TBD): Categories, Links, Games CRUDs and game actions.
- [ ] Exception middleware:
  - `BusinessRuleValidationException` → 400 Bad Request with `BrokenRule` + `Details`.
  - `NotFoundException` → 404 Not Found with `EntityName` + `Id`.
  - `ValidationException` (FluentValidation) → 422 Unprocessable Entity.
- [ ] OpenAPI / Swagger.
- [ ] `appsettings.json` + environment-specific configs.
- [ ] First end-to-end smoke test: create a Category → add Links → wire outgoing topology → create Game → make a step → complete.

---

## Sprint 5 — Tests ✅ closed 2026-05-09

Goal: lock down the contract surface with three flavors of test.

### Domain unit tests (`Games.Tests`)

- [ ] `BusinessRuleTestExtensions.AssertBrokenRule<TRule>(Action)` helper.
- [ ] One test per aggregate state transition (Game state machine; Link Activate/Deactivate; Category EditGeneralInfo).
- [ ] One test per business rule (positive + negative).
- [ ] Allowance VO tests: `Of(0).Consume()` throws; `Consume()` returns new instance with `Used+1`.
- [ ] `StandardScoreCalculator`: formula correctness across all three difficulty multipliers.

### Architecture tests (NetArchTest or ArchUnitNET)

- [ ] Aggregates implement `IAggregateRoot`.
- [ ] Domain layer has no `Application` / `Infrastructure` references.
- [ ] No `public` constructor on `Entity<TId>` subclasses.
- [ ] No `InvalidOperationException` thrown from Domain (except `TypedIdValueBase`).
- [ ] All `*DomainEvent` types end with `DomainEvent`.
- [ ] All `*Rule` types implement `IBusinessRule`.

### Integration tests

- [ ] Command → handler → repository → real (test) DB → DbContext SaveChanges path for one command per aggregate.
- [ ] Domain event handler dispatched after `IUnitOfWork.CommitAsync`.
- [ ] Decorator chain fires in expected order (validation → logging → unit-of-work → handler).

---

## Sprint 7 — Players Module ✅ closed 2026-05-11

Goal: ship the second module to validate the modular monolith pattern. Scope: minimum-viable Player identity (Guest + Apple + Google), profile (DisplayName + Discriminator), no stats (Stats is a future module per the Sprint 8 plan). Tek aggregate, tek schema (`players`), kendi Outbox tablosu, kendi decorator stack'i — `Modules/Games` layout'una birebir.

### Slice 1 — Domain layer ✅ done

- [x] `LexiLink.Modules.Players.Domain.csproj` + `InternalsVisibleTo` Application/Infrastructure/Tests.
- [x] `Player` aggregate root (`RegisterGuest` factory + `LinkAuthProvider` + `UpdateProfile`).
- [x] VOs: `PlayerId`, `Discriminator` (1-9999, `D4` format), `AuthIdentity` (owned).
- [x] Enum: `AuthProvider` (Guest, Apple, Google).
- [x] 9 rules (`DisplayName*`, `Locale*`, `DeviceId*`, `ExternalAuthId*`, `PlayerMustNotAlreadyHaveAuthProvider`, `SocialAuthProviderRequired`, `Discriminator*`, `AvatarUrl*`).
- [x] 3 events: `PlayerRegistered`, `AuthProviderLinked`, `PlayerProfileUpdated`.
- [x] Repository contract `IPlayerRepository` with `GetByIdAsync` + `GetByAuthProviderAsync` (login flow) + `AddAsync`.
- [x] Domain service interface `IDiscriminatorGenerator` (handler calls; Infrastructure implementation queries DB).

### Slice 2 — Application layer ✅ done

- [x] **`Players.Application/Configuration/IPlayerContext.cs`** + **`PlayerContext : IPlayerContext`** in the same folder. Kamil-faithful per-module context — impl `IExecutionContextAccessor.UserId`'yi `new PlayerId(...)` ile sarar. Kamil bunu Application katmanında tutuyor (Infrastructure'da değil) çünkü HTTP'den okumuyor, sadece bir interface'i daraltıyor.
- [x] Per-module CQRS contracts: `Contracts/{ICommand, ICommand<TResult>, IQuery<TResult>, CommandBase, CommandBase<TResult>, QueryBase<TResult>}` + `Configuration/Commands/ICommandHandler` + `Configuration/Queries/IQueryHandler`. Games modülünün birebir kopyası, Kamil pattern'iyle birebir.
- [x] Commands (3): `RegisterGuestPlayerCommand` (`CommandBase<Guid>`), `LinkAuthProviderCommand` (`CommandBase`), `UpdatePlayerProfileCommand` (`CommandBase`). `internal` handler + `internal` ctor; Domain factory metotları çağrılır.
- [x] Queries (2): `GetPlayerByIdQuery` (`QueryBase<PlayerDetailsDto>`) → NotFoundException atar; `GetPlayerByAuthProviderQuery` (`QueryBase<PlayerDetailsDto?>`) → null döner (login flow için gerekli — Apple/Google sub claim'i sorgusunda "yok" geçerli yanıt).
- [x] FluentValidation `AbstractValidator<T>` per command — yüzeysel kontroller (`NotEmpty`, `MaximumLength`/`MinimumLength` referansları domain `MaxLength` sabitlerinden), default mesajlarla (Games konvansiyonu — Kamil `.WithMessage(...)` kullanıyor ama biz proje içinde tutarlı kalıyoruz).
- [x] DTOs: `PlayerDetailsDto` (init `AuthIdentities` collection, Game pattern'i) + `AuthIdentityDto` (positional record).

### Slice 3 — Infrastructure layer ✅ done

- [x] `PlayersContext : DbContext` + `players` schema; `DbSet<Player>`, `DbSet<OutboxMessage>`. `ConfigureConventions` typed-ID reflection (Games pattern'i).
- [x] `PlayerEntityTypeConfiguration` — `OwnsMany<AuthIdentity>` mapping (composite PK `(PlayerId, Provider)`, `Provider` `varchar(32)` HasConversion<string>); `Discriminator` owned single (`DiscriminatorValue` column); field-access mode everywhere.
- [x] `PlayerRepository : IPlayerRepository` — `GetByIdAsync` (EF), **`GetByAuthProviderAsync` two-step (Dapper lookup PlayerId → EF load aggregate)** çünkü owned collection EF query'leri kırılgan; SqlConnectionFactory zaten injekte ediliyor.
- [x] `RandomDiscriminatorGenerator : IDiscriminatorGenerator` — Dapper ile `SELECT "DiscriminatorValue" WHERE "DisplayName"`; 10 random attempt + sequential scan fallback. Race condition DB unique constraint'i tetikler — retry handler-level future work.
- [x] `OutboxAccessor`, `OutboxMessageEntityTypeConfiguration` (table `players.OutboxMessages`) — Games'in birebir kopyası (Kamil pattern: per-module).
- [x] `PlayersAutofacModule` + `OutboxModule` (Games kopyası, namespace farkı dışında bire bir). `PlayerContext : IPlayerContext` da burada `InstancePerLifetimeScope` registered.
- [x] Decorator kopyaları (UoW × 2, Logging × 2, Validation × 2) `Configuration/Processing/` altında, Players'ın kendi `ICommandHandler<>` / `ICommandHandler<,>` / `ICommand` / `ICommand<T>` constraint'leriyle.

### Slice 4 — Database scripts ✅ done (2026-05-11)

- [x] `src/Database/LexiLink.Database/Structure/players/Schema/001_CreateSchema.sql`.
- [x] `players/Tables/010_Players.sql` + **unique index** `UX_Players_DisplayName_DiscriminatorValue`.
- [x] `players/Tables/020_PlayerAuthIdentities.sql` — composite PK `(PlayerId, Provider)` + **unique index** `UX_PlayerAuthIdentities_Provider_ExternalId` + FK→Players ON DELETE CASCADE.
- [x] `players/Tables/070_OutboxMessages.sql` — Games tablosuyla birebir, PK adı `PK_Players_OutboxMessages` (cross-schema name uniqueness için prefix).
- [x] `players/Views/110_v_Players.sql` — denormalized `Handle = DisplayName || '#' || lpad(DiscriminatorValue::text, 4, '0')`.
- [x] DbUp ile deploy: 5 script applied (first run), idempotent re-run "0 pending scripts" — schema temiz.

### Slice 5 — API host wiring ✅ done (2026-05-11)

- [x] `src/API/LexiLink.API/Modules/Players/PlayerEndpoints.cs` — 5 routes: `POST /players/guest` → 201 with `{id}`, `POST /players/{id}/auth-providers` → 204, `PATCH /players/{id}/profile` → 204, `GET /players/{id}` → 200 `PlayerDetailsDto` (NotFound → 404 via middleware), `GET /players/by-auth?provider=Apple&externalId=...` → 200 or 404 (null DTO → 404 — login flow).
- [x] `Program.cs` — `PlayersContext` DbContext registration, `PlayersAutofacModule`, separate `playersDomainNotificationsMap`, `PlayersOutboxModule` registered alongside Games equivalents. Type aliases (`GamesOutboxModule` / `PlayersOutboxModule`) ile same-name class clash çözüldü. Her iki `CheckMappings` build'den sonra çağrılır.
- [x] Smoke verified end-to-end: register guest → `Yasin#5879` handle → link Apple → patch profile → `isGuest` flips false → `/by-auth?provider=Apple` returns same player → unknown sub returns 404 → validator catches empty deviceId (422). Cross-Player Apple-sub collision DB-level unique constraint fires (500 — should map to 409 in a future polish; data integrity preserved).
- **Slice-içi fix**: EF auto-discovered public `Player.AuthIdentities` getter as a second navigation (alongside the OwnsMany-mapped `_authIdentities` private field) and failed model validation with `Unable to determine the relationship represented by navigation 'Player.AuthIdentities'`. Resolved by `builder.Ignore(p => p.AuthIdentities)` in `PlayerEntityTypeConfiguration`. Aggregate public collection getter over owned VO collection olan her durumda aynı tuzak — gelecek modüller için not.

### Slice 6 — Tests + end-to-end smoke ✅ done (2026-05-11)

- [x] `LexiLink.Modules.Players.Tests` (NUnit 4) — 25 domain unit tests (RegisterGuest, LinkAuthProvider, UpdateProfile, all Player rules, Discriminator VO). Added missing avatar URL max-length branch coverage.
- [x] `LexiLink.Modules.Players.IntegrationTests` — Kamil-style real Postgres composition root (Autofac + MediatR + `PlayersAutofacModule` + `OutboxModule`), `[Category("Integration")]`, per-test cleanup of `players` schema tables.
- [x] Integration coverage: register guest, link Apple + query by auth provider, update profile, unknown auth provider returns null. Verified: 4/4 passing.
- [x] Migrator + API smoke covered in Slice 5: yeni player kaydet, Apple bağla, profili güncelle, by-auth ile bul.

---

## Sprint UR — Undo + Reset Modules (active)

Goal: extract per-player **Undo** and **Reset** charges out of the
`Game` aggregate's allowance VOs into two new Kamil-faithful modules
(`Modules/Undo/` + `Modules/Reset/`), mirroring the Sprint H Hint
pattern. The per-game free quota is **eliminated** — every
`Game.UseUndo()` and `Game.ResetToStart()` call now consumes one
charge from the player's persistent inventory via a sync gateway
(`IUndoGuard` / `IResetGuard`). Empty inventory blocks the action.
The same sprint expands `QuestReward` from `(Energy, Hint)` to four
fields `(Energy, Hint, Undo, Reset)` so admin-defined quests can
deliver any combination.

### Why now

Sprint H ended with two product gaps surfaced during manual testing:
(1) Undo and Reset feel "free" — Easy/Medium/Hard giveaways make
puzzles too forgiving and remove the tension that creates moment-
to-moment decisions; (2) the reward catalog only delivers Energy and
Hint, so quest design can't motivate skillful play. Treating Undo
and Reset as scarce, earnable resources mirrors the design
philosophy proven by the Hint inventory in Sprint H — players value
what they pay for, and operators get a fourth knob (Undo/Reset
grants from quests, admin compensation, future shop sales).

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Module structure | **Two separate modules** (`Modules/Undo/` + `Modules/Reset/`). Hint scaffolding cloned in both. ~10 new csproj + 2 new schemas + 2 new outbox tables (boilerplate accepted in exchange for full module isolation). |
| Free quota (per-game) | **Eliminated entirely.** `UndoAllowance` + `ResetAllowance` VOs and their rules are deleted; `Game.UseUndo()` and `Game.ResetToStart()` always invoke the sync gateway (no `HasFreeXRemaining` branching, **unlike** Hint). |
| Counter retention | `Game._undosUsed` + `_resetsUsed` survive as **plain int counters** (statistics + scoring). Increment + read only; no rule, no branching. Frontend reads them through `GameDetailsDto`. |
| Quest reward shape | `QuestReward` expands to 4 fields: `(EnergyReward, HintReward, UndoReward, ResetReward)`. `QuestRewardMustHaveAtLeastOnePositiveRule` widens to cover all four. Destructive ALTER on `quests.QuestDefinitions`. |
| Reward delivery | Four independent outbox consumers, each guarding on its reward's positivity. Hint pattern symmetric: each consumer no-ops when its reward is 0. Adds two new reverse cross-module event dependencies (`Undo.Application → Quests.IntegrationEvents` and `Reset.Application → Quests.IntegrationEvents`) — granular ArchTest allows. |
| Player inventory shape | `PlayerUndoInventory` + `PlayerResetInventory` each hold a single `int _balance`. No cap, no refill timer (twins of `PlayerHintInventory`). Over-cap `GrantBonus` permitted. |
| Initial balance | 0 for both inventories. Configurable via `Undo:InitialBalance` + `Reset:InitialBalance` (defaults 0). Operator can flip in `appsettings.json` pre-launch if onboarding policy needs softer ramp. |
| Sync gateway pattern | Two new gateways (`IUndoGuard.EnsureUndoAvailableAsync`, `IResetGuard.EnsureResetAvailableAsync`) — exact `IHintGuard` template. Contracts in Games.Application; adapters in API host. |
| Admin operations | `AdminSet` + `AdminReset` domain methods on each aggregate; 3 admin commands per module (Set / GrantBonus / Reset) + 1 player query (`GET /{undo,reset}/me`). 7th and 8th per-module copies of `AdminAuditingCommandHandlerDecorator`. |
| Migration | Destructive but idempotent: `Game._undoAllowance` / `_resetAllowance` EF mapping removed; `Games.UndosTotal` / `ResetsTotal` columns dropped if they exist; `quests.QuestDefinitions` ALTER ADD `UndoReward`, `ResetReward` NOT NULL DEFAULT 0. Information_schema guards so fresh DBs short-circuit. |
| Slice ordering | **Game.cs reshape first**, then modules. Allowance VOs + rules deleted in S1 while modules don't yet exist; API host wires no-op `AlwaysAllowing*Guard` adapters temporarily so the stack stays runnable. Real adapters land in S4. |

### Architecture notes

- **Undo + Reset are Hint's twins**, both stripped further. Each
  aggregate is a single `int _balance`; the codebase will have three
  near-identical inventory modules. The repetition is intentional —
  Kamil's per-module decorator + outbox + EF mapping pattern
  forbids `Common` shortcuts.
- **No per-game free quota** is the load-bearing difference from
  Hint. `Game.UseUndo()` and `Game.ResetToStart()` always invoke the
  sync gateway; there is no `game.HasFreeUndoRemaining` branch. This
  simplifies `UseUndoCommandHandler` / `ResetCommandHandler` to a
  single call shape and removes the dual-source-of-truth problem
  Hint accepted.
- **Counters survive but lose semantics.** `_undosUsed` /
  `_resetsUsed` no longer "remaining quota" — they are usage stats
  for scoring + UI ("you undid 3 times this game"). If
  `IScoreCalculator` consumes them today, the formula stays; if it
  was reading "remaining as a bonus", that codepath needs adjustment.
  S1 audit will surface the actual coupling.
- **Quest reward 4-fields > generic reward bag** chosen over a
  `(RewardType, Amount)[]` table for the same reason H4 was: simpler
  EF mapping, simpler admin form, simpler outbox consumer (one int
  field per consumer). The cost is another schema reshape if a
  fifth reward type is ever added.
- **Two more reverse event dependencies** land in S5. After this
  sprint, four modules (`Energy.Application`, `Hint.Application`,
  `Undo.Application`, `Reset.Application`) all carry a granular
  ArchTest allow on `Quests.IntegrationEvents`. The pattern is now
  the LexiLink norm for reward delivery.

### Slice plan

#### UR1 — Game.cs destructive reshape

- Delete `Modules/Games/Domain/Games/Allowances/UndoAllowance.cs` +
  `ResetAllowance.cs` and the matching
  `UndoAllowanceMustHaveRemainingRule` +
  `ResetAllowanceMustHaveRemainingRule`.
- Remove `_undoAllowance` + `_resetAllowance` fields from
  `Game.cs`; remove their EF `OwnsOne` mappings from
  `GameEntityTypeConfiguration`. DbUp script
  `games/Tables/050_DropUndoResetAllowanceColumns.sql`
  (idempotent ALTER DROP COLUMN).
- Add `_undosUsed` + `_resetsUsed` plain int counters on Game
  (default 0). Map as plain columns in EF
  (`Games.UndosUsed` / `Games.ResetsUsed` — keep current column
  names if present, otherwise add).
- Reshape `Game.UseUndo()` to take no allowance check; emit
  `UndoUsedDomainEvent` and increment `_undosUsed`. Same for
  `Game.ResetToStart()`.
- Add `Game.UseUndoWithExternalInventory()` +
  `Game.ResetWithExternalInventory()` (no-op stubs that increment
  counter + emit event; the per-game branch will not be reached
  post-reshape but the method exists so the API host adapter
  pattern matches Hint's `UseHintWithExternalInventory`).
- New contracts in `Games.Application/Configuration/CrossModule/`:
  `IUndoGuard.EnsureUndoAvailableAsync(playerId, ct)` and
  `IResetGuard.EnsureResetAvailableAsync(playerId, ct)`.
- Reshape `UseUndoCommandHandler` and `ResetCommandHandler` to
  always invoke the gateway then call the external-inventory
  method on Game (no `HasFree*Remaining` branching).
- API host: temporary no-op `AlwaysAllowingUndoGuard` +
  `AlwaysAllowingResetGuard` adapters in
  `LexiLink.API/CrossModule/`. Replaced in UR4.
- Games.IT: stub `AlwaysAllowing{Undo,Reset}Guard` in TestBase
  (Hint pattern). Recording variant lands in UR4.
- Delete `UndoAllowanceTests` + `ResetAllowanceTests`. Reshape
  Game lifecycle tests that asserted the allowance count
  (probably 4–6 cases in GameUndoTests / GameResetTests). Keep
  tests for the increment counter + event emission shape.
- `IGameConfigurationService.ResolveUndos(d)` /
  `ResolveResets(d)` removed if no remaining callers; otherwise
  flattened or kept as informational. Audit the surface area.
- Stats / scoring audit: confirm `IScoreCalculator` impact and
  fix if necessary.

**Acceptance:** `dotnet test LexiLink.sln` green; Game.UseUndo
and Game.ResetToStart always succeed in dev because the no-op
adapter accepts everything. Frontend Undo / Reset buttons still
work in the dev stack (no inventory yet — that lands UR2/UR3).

#### UR2 — Undo + Reset module foundation

- Two new module directories cloned from `Modules/Hint/`:
  `Modules/Undo/{Domain,Application,Infrastructure,IntegrationEvents,Tests,IntegrationTests}/`
  and `Modules/Reset/...` (same shape).
- Aggregates: `PlayerUndoInventory` + `PlayerResetInventory`
  (each identified by their own typed Id; same value as owning
  `PlayerId`). Single `int _balance` field. Twin of Hint
  aggregate (no max, no refill).
- Rules per module: `{Undo,Reset}AmountMustBePositiveRule`,
  `*AmountMustBeNonNegativeRule`,
  `*BalanceMustBeSufficientRule`. Six rules total across both.
- Events per aggregate: `Initialized`, `Consumed`, `Granted`,
  `AdminSet`, `AdminReset` (5 per aggregate, 10 total).
- Per-module Autofac module + Startup + UoW +
  DomainEventsDispatcher + decorator chain (Logging /
  Validation / UnitOfWork — admin auditing decorator added in
  UR6).
- DbUp scaffolding per module:
  `{undo,reset}/Schema/001_CreateSchema.sql`,
  `{undo,reset}/Tables/010_Player{Undo,Reset}Inventories.sql`,
  `{undo,reset}/Tables/070_OutboxMessages.sql`.

**Acceptance:** both modules compile, ArchTests pass with new
namespaces in the allowed-pair list, no integration tests yet.

#### UR3 — Lazy init from PlayerRegistered

- `EnsurePlayerUndoInventoryExistsCommand` + handler + validator
  in each module.
- `PlayerRegisteredIntegrationEventHandler` in
  `{Undo,Reset}.Application/Player*Inventories/ProcessIntegrationEvents/`
  dispatching the ensure command. Mirrors Hint's H2 lazy-init.
- `I{Undo,Reset}ConfigurationService.InitialBalance` Domain
  interface + Infrastructure implementation reading
  `{Undo,Reset}:InitialBalance` config (default 0).

**Acceptance:** new guest registration creates a row in
`undo.PlayerUndoInventories` and `reset.PlayerResetInventories`
after outbox processing. Idempotency confirmed via replayed
`PlayerRegisteredIntegrationEvent`.

#### UR4 — Sync gateway integration

- `LexiLink.API/CrossModule/UndoGuard.cs` + `ResetGuard.cs`
  replace the no-op stubs from UR1. Each adapter calls
  `I{Undo,Reset}Module.ExecuteCommandAsync(new
  ConsumePlayer{Undo,Reset}Command(playerId, 1))`.
- `ConsumePlayer{Undo,Reset}Command` + handler + validator in
  each module.
- Games.IT: `AlwaysAllowing{Undo,Reset}Guard` upgraded to
  configurable `Recording{Undo,Reset}Guard` (Hint pattern from
  H7 — `CallCount` + `RejectNext` flag).
- New Games.IT tests: `UseUndoFallThroughTests` +
  `ResetFallThroughTests`. Each asserts: every call invokes the
  gateway (no free branching); rejection propagates before Game
  mutation; counter only increments on success.

**Acceptance:** end-to-end Undo and Reset flows go through the
real Hint-style sync gateway; integration tests prove both
modules participate.

#### UR5 — Quest 4-reward destructive

- `QuestDefinition` adds `_undoReward` + `_resetReward` fields;
  `Create` / `Update` signatures expand to 4 reward parameters.
- `QuestRewardMustHaveAtLeastOnePositiveRule` widens: all four
  must be ≥ 0, at least one > 0.
- `PlayerQuestClaimedDomainEvent` and
  `QuestClaimedIntegrationEvent` carry all 4 reward fields.
- `PlayerQuest.Claim(now, ready, energyReward, hintReward,
  undoReward, resetReward)`.
- New `QuestClaimedIntegrationEventHandler` in
  `Undo.Application` + `Reset.Application` (each guards on its
  own reward > 0; dispatches `Grant{Undo,Reset}Command`).
- `GrantUndoCommand` + `GrantResetCommand` (each calls
  `Player*Inventory.GrantBonus`). Granular ArchTest allows for
  `Undo.Application → Quests.IntegrationEvents` and
  `Reset.Application → Quests.IntegrationEvents`.
- DbUp `quests/Tables/050_ExpandQuestRewardsWithUndoReset.sql` —
  idempotent ALTER TABLE ADD COLUMN with
  information_schema guards.
- All admin Quest commands / validators / DTOs / endpoint
  requests + EF mapping reshape to carry 4 reward fields.
- Quests.IT existing reward delivery tests reshape; new tests
  for the two new consumers; existing
  `QuestClaimedIntegrationEvent` shape tests updated.

**Acceptance:** admin can mint a quest with any combination of
the 4 rewards; claiming dispatches exactly the consumers whose
reward > 0; each inventory updates accordingly.

#### UR6 — Admin operations + GET endpoints + audit

- `PlayerUndoInventory.AdminSet(newBalance, now)` +
  `AdminReset(now)` + matching domain events. Same for
  `PlayerResetInventory`.
- 3 `IAdminCommand` commands per module
  (`SetPlayer{Undo,Reset}Command`,
  `GrantBonus{Undo,Reset}Command`,
  `ResetPlayer{Undo,Reset}Command`). `AuditTargetType =>
  "{Undo,Reset}.Player{Undo,Reset}Inventory"`.
- Player query: `GetPlayer{Undo,Reset}Query` + handler
  (Dapper SELECT). Returns
  `Player{Undo,Reset}SnapshotDto(PlayerId, Balance)`.
- 7th + 8th per-module copies of
  `AdminAuditingCommandHandlerDecorator` template.
- `{Undo,Reset}AdminActionPerformedNotification` + handler
  publishing `AdminActionPerformedIntegrationEvent`.
  `{Undo,Reset}.Infrastructure.csproj` adds reference to
  `Administration.IntegrationEvents` (granular ArchTest allow).
- API endpoints:
  - `GET /{undo,reset}/me` (AuthenticatedPlayer).
  - `GET /admin/players/{id}/{undo,reset}` +
    `POST .../set | grant | reset` (AuthenticatedAdmin).

**Acceptance:** admin can lookup any player's Undo + Reset
balance, set / grant / reset each independently; every action
hits the audit log with the right `TargetType`.

#### UR7 — Frontend reshape

- Two new player features: `lib/features/undo/` +
  `lib/features/reset/`. Each has `PlayerX` DTO,
  `XRepository.getMe()`, `XCubit`, `XBadge` (icon + balance).
  Undo badge: `Icons.undo` in `colorScheme.secondary`. Reset
  badge: `Icons.restart_alt` in `colorScheme.error.withOpacity()`
  or a fresh tertiary tone — UI taste call at slice time.
- Two new admin features: `lib/features/admin_undo/` +
  `lib/features/admin_reset/`. Same shape as `admin_hint` /
  `admin_energy`: lookup row + balance card + set / grant /
  reset.
- HomeScreen top bar: four badges in a Row (Energy ⚡ + Hint 💡 +
  Undo ↶ + Reset ↻). If the layout cramps on narrow screens, wrap
  the row or use a `Wrap` widget — UI taste call at slice time.
- `/admin/undo` + `/admin/reset` routes wired in `app_router.dart`;
  nav destinations + icons added to `app_admin_shell.dart`;
  placeholder wrappers added.
- Quest definition form expands to 4 reward inputs (Energy ⚡ +
  Hint 💡 + Undo ↶ + Reset ↻) with the same form-level
  at-least-one-positive rule (`_rewardSumError`).
- Quest tile (admin + player) renders all 4 possible badges
  conditionally (only when > 0). Spacers between only adjacent
  positive rewards.
- Test fixtures reshape: existing payloads switch from
  2-field reward to 4-field. ~7 test files affected
  (admin_quests {cubit, repo, screen}, quests {cubit, repo}, new
  undo + reset feature tests).

**Acceptance:** `flutter analyze` shows no new errors; `flutter
test` green; manual smoke pass on the dev stack.

#### UR8 — Tests + quality gate + manual verification + docs

- Backend Undo.Tests + Undo.IT + Reset.Tests + Reset.IT mirroring
  Hint.Tests/IT (Initialize / Consume / GrantBonus / Admin
  scenarios + lifecycle + admin command tests). ~30 new domain
  tests + ~12 new integration tests expected.
- Games.IT `UseUndoFallThroughTests` + `ResetFallThroughTests`
  finalized.
- `scripts/test.sh` registers `Undo.Tests`, `Undo.IT`,
  `Reset.Tests`, `Reset.IT`.
- `dotnet test LexiLink.sln` + `flutter test` both green.
- Manual verification:
  1. Multi-reward quest claim — admin creates 4 separate quests
     (salt-Energy, salt-Hint, salt-Undo, salt-Reset); player
     completes each and verifies only the matching inventory
     updates.
  2. Mixed-reward quest — single quest with Energy + Hint + Undo
     + Reset all > 0; claim once, verify all 4 outbox consumers
     fire.
  3. Undo / Reset fall-through with empty inventory — drain
     player to 0 via admin reset; in-game Undo or Reset button
     surfaces the rule error and Game state does not advance.
  4. Admin Set / Grant / Reset — exercise each operation in
     both admin consoles (`/admin/undo` + `/admin/reset`);
     confirm audit log shows correct `TargetType` rows.
- Docs: prepend Sprint UR entries to `progress.md`; pivot
  `activeContext.md > Active Sprint` to UR closure; update
  `GLOSSARY.md` (new aggregates / events / rules / gateways /
  4-reward QuestDefinition + PlayerQuest text); update frontend
  docs (UR7 slice details).

**Acceptance:** sprint closes with operator-confirmed manual
verification + all four golden flows above passing; every doc
synced.

---

## Sprint H — Hint Module + Quest Multi-Reward

Goal: ship a **Hint** module that holds a per-player persistent hint
balance, integrated end-to-end. Per-game free hint count stays on
`Game` (1 fixed, see locked decisions). When the player taps "use
hint" and the in-game allowance is exhausted, the request falls
through to the new `IHintGuard` sync gateway which consumes from the
player's hint inventory. The same sprint expands quest rewards from
a single int to a structured `QuestReward` carrying both
`EnergyReward` and `HintReward`, with at-least-one-positive
validation. Two consumers (Energy + Hint) listen to the same
`QuestClaimedIntegrationEvent` and each grants its share
independently.

### Why now

Manual testing of Sprint Q1 exposed a product gap: the only reward
players can earn is Energy. The operator wants hints as a
differentiated reward — used inside a game when the puzzle is
genuinely hard — so quests can offer either, both, or asymmetric
mixes (e.g. "complete 10 games → 5⚡ + 2💡"). The Hint module also
makes per-player inventory inspectable from the admin panel (set /
grant / reset like Energy), so support cases can compensate without
manual SQL.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Per-game free hint location | Stays in `Game.HintAllowance` VO. Game knows nothing about the Hint module. |
| Free hint count | Fixed at **1** per game across all difficulties. If `IGameConfigurationService.ResolveHintAllowance` currently varies by difficulty, H3 collapses it to 1. |
| Hint module aggregate scope | `PlayerHintInventory` holds `_balance` only. No refill, no max cap, no timer. Sımılar to Energy but stripped of mechanics that don't apply. |
| Hint inventory cap | **Unlimited.** Players can hoard. Hints are earned through quest claims, never free time-based refill, so hoarding is rate-limited by quest cadence. |
| Initial balance | 0. Configurable via `Hint:InitialBalance` (default 0); operator can bump in `appsettings` if onboarding policy changes. |
| Quest reward shape | `QuestReward` VO = `(int EnergyReward, int HintReward)`. Both ≥ 0; rule: at least one > 0. Stored as two columns on `quests.QuestDefinitions`. |
| Reward delivery | Each module listens to `QuestClaimedIntegrationEvent` independently. Energy consumer no-ops when `EnergyReward == 0`; Hint consumer no-ops when `HintReward == 0`. Both fire in the same outbox-dispatched event. |
| Hint guard pattern | New sync gateway `IHintGuard.EnsureHintAvailableAsync(playerId, ct)`. Insufficient → `BusinessRuleValidationException` propagates up — same dual-write residual as `IEnergyGuard` (acceptable for MVP, documented in `kamil-modular-monolith-comparison.md`). |
| Admin operations | Set / Grant / Reset endpoints + audit, exact `IAdminCommand` template from Energy B8. |
| Migration | Destructive: rename `quests.QuestDefinitions.Reward` → `EnergyReward`, add `HintReward` NOT NULL DEFAULT 0. Daily seed values migrate losslessly (`Reward=5` → `EnergyReward=5, HintReward=0`). |

### Architecture notes

- **Hint mirrors Energy but smaller.** PlayerHintInventory has no
  refill timer and no max cap; the aggregate file is ~half the size
  of `PlayerEnergy`. Same outbox/inbox/decorator infrastructure
  copied per Kamil's decorator-per-module rule (no Common shortcut).
- **`Game` stays ignorant of Hint.** The `HintAllowance` VO on Game
  continues to track free per-game hint usage. The handler
  (`UseHintCommandHandler`) is the integration point: if
  `game.HasFreeHintRemaining` then `game.UseHint()`, otherwise the
  handler calls `IHintGuard.EnsureHintAvailableAsync` (which throws
  on insufficient balance) and then `game.UseHintWithExternalInventory()`
  to update the puzzle state without touching the in-game allowance.
- **Reverse cross-module event dep, second instance.** Sprint Q1
  established Energy → `Quests.IntegrationEvents` as the pattern.
  Sprint H adds Hint → `Quests.IntegrationEvents` with the exact
  same granular ArchTest allow. Quests.Domain / Application /
  Infrastructure stay forbidden from any consumer.
- **Reward delivery atomicity.** `QuestClaimedIntegrationEvent` is
  outbox-published once. Both Energy and Hint consumers receive it
  inside the same in-process bus dispatch; each is its own scoped
  transaction. If one fails and the other succeeds, the failing
  consumer's outbox/retry path recovers it independently. Operator
  is comfortable with eventual consistency here — these are reward
  grants, not the puzzle state.
- **Hint admin operations reuse Energy B8 template.** `AdminSet` /
  `AdminGrant` / `AdminReset` domain methods on PlayerHintInventory,
  plus a per-module `AdminAuditingCommandHandlerDecorator` and
  `HintAdminActionPerformedNotification` mapping. The audit row
  lands in `administration.AdminActionAudit` via the existing
  cross-module outbox flow.

### Slice plan

#### H1 — Hint module foundation (target: 2 h)

- New module under `src/Modules/Hint/` with six projects (Domain,
  Application, Infrastructure, IntegrationEvents, Tests,
  IntegrationTests) following the Energy template.
- `PlayerHintInventory` aggregate: `Id` (`PlayerHintInventoryId`,
  same Guid as `PlayerId`), `_balance`. Methods: `InitializeFor`,
  `Consume(amount, now)`, `GrantBonus(amount, now)`. Events:
  `PlayerHintInventoryInitializedDomainEvent`,
  `PlayerHintConsumedDomainEvent`, `PlayerHintGrantedDomainEvent`.
  Rules: `HintAmountMustBePositiveRule`,
  `HintBalanceMustBeSufficientRule`.
- `IPlayerHintInventoryRepository`. EF mapping + per-module Autofac
  module + `HintStartup`.
- DbUp: `hint` schema, `hint.PlayerHintInventories` (PlayerId PK,
  Balance int NOT NULL, CreatedAt, UpdatedAt), `hint.OutboxMessages`.
- ArchTests: `Hint*` assemblies added to boundary tests; no
  forbidden references.

#### H2 — Player registration → Hint init (target: 30 min)

- `Hint.Application/EnsurePlayerHintInventoryExistsCommand` +
  handler (idempotent, no-op if already initialized).
- `Hint.Application/PlayerRegisteredIntegrationEventHandler`
  dispatches the command on every `PlayerRegisteredIntegrationEvent`.
- `Hint:InitialBalance` config option (default 0).
  `IHintConfigurationService` exposes it (matches Energy pattern).

#### H3 — IHintGuard sync gateway + Game.UseHint refactor (target: 1 h)

- `Games.Application/Configuration/CrossModule/IHintGuard.cs`:
  `EnsureHintAvailableAsync(playerId, ct)` — throws on insufficient.
- API host adapter `LexiLink.API/CrossModule/HintGuard.cs` →
  dispatches `ConsumePlayerHintCommand` to `IHintModule`.
- Verify (and if needed flatten) `IGameConfigurationService`
  hint allowance to 1 fixed across difficulties.
- `Game` aggregate:
  - Add `HasFreeHintRemaining` public read property
    (delegates to `HintAllowance.Remaining > 0`).
  - Add `UseHintWithExternalInventory()` method — same effect on
    puzzle state as `UseHint()` but doesn't touch the allowance.
- `UseHintCommandHandler` refactor: branch on
  `game.HasFreeHintRemaining`. Free path calls `game.UseHint()`;
  external path calls `IHintGuard.EnsureHintAvailableAsync` first
  then `game.UseHintWithExternalInventory()`. Domain rule violation
  from either path propagates as `BusinessRuleValidationException`.
- ArchTest granular allow: `Games.Application` →
  `Hint.Application` reference (cross-module contract only,
  Hint.Domain/Infrastructure stay forbidden).

#### H4 — Quest multi-reward shape (target: 2 h, destructive)

- `Quests.Domain/PlayerQuests/QuestReward.cs` VO: positional
  record `(int EnergyReward, int HintReward)` with static
  `Of(energy, hint)` factory enforcing both ≥ 0 and at-least-one >
  0.
- `QuestDefinition` field reshape: `_energyReward` + `_hintReward`
  replace `_reward`. Properties exposed. `Create` / `Update`
  signatures take the pair (or the `QuestReward` VO — decide at
  slice start for ergonomic call sites).
- Rules: rename `QuestRewardMustBePositiveRule` →
  `QuestRewardMustHaveAtLeastOnePositiveRule(energyReward,
  hintReward)`. Drop the single-int variant.
- Events: `QuestDefinitionCreated/UpdatedDomainEvent` carry both
  fields. `PlayerQuestClaimedDomainEvent` carries
  `EnergyReward` + `HintReward`. `PlayerQuest.Claim(now,
  isReadyToClaim, energyReward, hintReward)` forwards both into the
  event.
- `IntegrationEvents/QuestClaimedIntegrationEvent`: rename `Reward`
  → `EnergyReward`, add `HintReward`.
- `ClaimQuestCommandHandler`: reads definition, passes both rewards
  into `Claim`.
- DbUp `quests/Tables/040_ReshapeQuestRewardsForSprintH.sql`:
  `ALTER TABLE quests."QuestDefinitions" RENAME COLUMN "Reward" TO
  "EnergyReward"; ALTER TABLE ... ADD COLUMN "HintReward" int NOT
  NULL DEFAULT 0;`. Idempotent (uses `IF EXISTS` / `IF NOT
  EXISTS`-style guards or `ALTER` wrapped in DO block).
- Update `010` / `020` canonical files to the new shape.
- Update daily seed (`021`) — `'Günlük 3 Oyun'` becomes
  `EnergyReward=5, HintReward=0`.
- Energy consumer adapt: `if (event.EnergyReward > 0)` guard around
  the existing `EnsurePlayerEnergyExists + GrantEnergyCommand`
  pipeline.
- New `Hint.Application/QuestClaimedIntegrationEventHandler`:
  symmetric guard `if (event.HintReward > 0)` →
  `EnsurePlayerHintInventoryExists + GrantHintCommand`.
- Admin `CreateQuestDefinitionCommand` /
  `UpdateQuestDefinitionCommand`: replace `Reward` with
  `EnergyReward` + `HintReward`. Validators: each ≥ 0, at least
  one > 0. Admin DTOs: two fields.
- `ArchTest` granular allow: `Hint.Application` →
  `Quests.IntegrationEvents`.

#### H5 — API endpoints + admin operations (target: 45 min)

- `GET /players/me/hint` — `AuthenticatedPlayer` policy, returns
  `{ playerId, balance }`.
- `GET /admin/players/{playerId}/hint` — `AuthenticatedAdmin`
  policy, returns the same DTO.
- `POST /admin/players/{playerId}/hint/set|grant|reset` — three
  `IAdminCommand`s following the Energy B8 template:
  - `SetPlayerHintCommand(playerId, newBalance)` — 0..int.MaxValue.
  - `GrantHintCommand(playerId, amount)` — over-cap allowed
    (unlimited cap).
  - `ResetPlayerHintCommand(playerId)` — balance ← 0.
- `PlayerHintInventory.AdminSet` / `AdminGrant` / `AdminReset`
  domain methods + events (`PlayerHintAdminSetDomainEvent`, etc.).
- `Hint.Infrastructure/Configuration/Processing/AdminAuditingCommandHandlerDecorator`
  (per-module copy, fifth instance after Quests/Energy/Players/
  Games). `HintAdminActionPerformedNotification` mapping.
- `Hint.Infrastructure` → `Administration.IntegrationEvents`
  granular ArchTest allow.
- API host: `LexiLink.API/Modules/Admin/AdminHintEndpoints.cs`.

#### H6 — Frontend reshape (target: 2 h)

- New feature folder `lib/features/hint/`:
  - `data/player_hint.dart` — `{ playerId, balance }`.
  - `data/hint_repository.dart` — `getMe()`.
  - `application/hint_cubit.dart` — load + refresh.
- New feature folder `lib/features/admin_hint/`:
  - Mirror of `admin_energy` (set/grant/reset card).
- Player UI:
  - Header: hint balance badge next to the energy badge.
  - Game screen: hint button unchanged in shape; backend now
    decides free vs inventory automatically. Optional: surface
    `freeHintRemaining` so the UI can show "1 ücretsiz" vs "💡
    {balance}".
- Admin quest form (`quest_definition_form.dart`): split reward
  input into two — "Enerji ödülü" (⚡ icon) and "İpucu ödülü" (💡
  icon). Validation: each ≥ 0, at least one > 0. Row in admin
  catalog renders both badges when present.
- Admin player console: hint card next to the energy card with set
  / grant / reset buttons.

#### H7 — Tests + quality gate (target: 2 h)

- Backend:
  - `Hint.Tests` — domain unit tests for the aggregate, rules,
    events.
  - `Hint.IntegrationTests` — Initialize, Consume happy + rule
    violation, GrantBonus, admin commands + audit roundtrip, quest
    reward delivery via `QuestClaimedIntegrationEvent`.
  - `Games.IntegrationTests` — UseHint with free remaining, with
    free exhausted + hint inventory available, with both
    exhausted (rule violation).
  - `Energy.IntegrationTests` — quest reward consumer skips when
    `EnergyReward == 0`.
  - `Quests.IntegrationTests` — admin Create/Update with new
    reward shape, validator at-least-one rule, audit row.
  - `API.Tests` — `GET /players/me/hint`, admin operations
    401/403/200.
- Frontend:
  - `hint_cubit_test`, `hint_repository_test`.
  - `admin_hint_*_test`.
  - `admin_quests_screen_test` extension for two-reward form.
- Run `scripts/test.sh` + `flutter test`. Sprint closes only when
  both quality gates are green.

#### H8 — Manual verification + docs (target: 30 min)

- Restart stack (API + Flutter web).
- Admin creates a quest with `EnergyReward=5, HintReward=2`.
- Test player completes it; verify Energy and Hint balances both
  grow by the right amounts and the audit shows the admin's create
  action.
- Player plays a game, uses 1 hint (free), tries again — second
  hint should consume from inventory; refresh shows balance -1.
- Once inventory hits 0, a third use-hint attempt returns the
  business rule error.
- Docs:
  - `progress.md` → "Sprint H" entry with per-slice notes.
  - `activeContext.md` → flip Active Sprint to closed; document
    Hint constraints (no refill/cap, free hint stays in Game,
    multi-reward consumer pattern).
  - `frontendActiveContext.md` + `frontendProgress.md` → Slice H6.
  - `GLOSSARY.md` → add `PlayerHintInventory`, `IHintGuard`,
    `QuestReward`, update `QuestClaimedIntegrationEvent` shape and
    business-rule total.

### Risk / open questions

- **`IGameConfigurationService.ResolveHintAllowance` shape today.**
  Slice H3 starts by checking if it varies by difficulty. If yes,
  fixing it to 1 is a small behavior change for the in-game free
  hint count — operator explicitly approved fixed-at-1.
- **Quest reward serialization size.** `QuestClaimedIntegrationEvent`
  grows from 5 fields to 6. Outbox row size impact negligible. No
  concern.
- **Migration safety.** `040_ReshapeQuestRewardsForSprintH.sql` runs
  ALTER COLUMN + ADD COLUMN on a populated `QuestDefinitions` table
  (the daily seed). Both are non-blocking in Postgres. Existing rows
  get `HintReward = 0`.
- **Cross-test order coupling.** Two cross-module event handlers
  (Energy + Hint) for the same integration event. Tests must
  tolerate either order — the in-memory bus dispatches them
  sequentially in registration order, but the assertion should
  check the end state, not the order.

### Acceptance

- Admin operator can create a quest with any mix of Energy and
  Hint rewards (subject to the at-least-one-positive rule), claim
  it as a test player, and observe both inventories update via the
  player UI and the admin player console.
- Player tapping "use hint" inside a game first consumes the free
  in-game hint (1 per game), then falls through to the personal
  hint inventory once free is exhausted. When inventory is 0 the
  request is blocked with a business rule error.
- All backend tests + Flutter tests pass after H7. Manual flow in
  H8 confirms end-to-end behaviour. Sprint commits land one-per-
  slice (H1 → H8 = eight commits) on `main`.

---

## Sprint Q1 — Quests Module Redesign (data-driven, lazy, chain-aware) ✅ Q1.1–Q1.7 closed 2026-05-24

**Status:** Backend slices Q1.1–Q1.5 + Q1.7 and frontend slice Q1.6
shipped on 2026-05-24 as seven sequential commits on `main`. Final
quality gate: **361/361 .NET tests + 103/103 Flutter tests green**.
Only Q1.8 (operator-level manual verification — admin builds a chain
via the UI and a guest player runs through it) remains.

Per-slice delivery detail in `progress.md > Sprint Q1` and
`frontendProgress.md > Slice Q1.6`. The full original plan + locked
decisions are preserved below for reference.

Goal: replace the fixed-enum, hardcoded-behavior quest catalog with a
fully data-driven model where every quest definition carries its own
trigger + threshold + reward + prerequisite. PlayerQuest rows are
**not** materialized at admin-create time; they are issued lazily when
the player opens the app (splash sync) or the quest page. This keeps
DB row growth bounded to actually-engaged players and removes the need
for hardcoded `QuestType` event handlers.

### Why now

Manual testing of the closed Administration backend + admin frontend
F1–F6 surfaced three real product gaps:

1. **Closed catalog.** `QuestType` is a fixed enum (4 known values, all
   seeded). Admin "Create quest" is functionally inert — every
   selectable type already has a definition, so the only useful
   actions are Edit / Deactivate / Reactivate. Adding new quest types
   was a code+seed change, not an admin-runtime action.
2. **Hardcoded behavior.** `GameCompletedIntegrationEventHandler` and
   `AuthProviderLinkedIntegrationEventHandler` enumerate three known
   types each. New types (even if the enum had `Custom1/2/3` slots)
   never fire automatic issuance/progress because no handler knows
   about them.
3. **Eager broadcast cost.** The latest B15 slice (Quests listens to
   `PlayerRegisteredIntegrationEvent` → issue all active definitions)
   correctly auto-issues quests at register time, but every new admin
   `CreateQuestDefinition` call would need an N-player broadcast to be
   useful for existing players. At any non-trivial player count this
   is wasteful for inactive accounts.

### Decisions (locked)

| Decision | Choice |
| --- | --- |
| Quest identity | `QuestDefinition.Id` (Guid). `QuestType` enum **removed**. |
| Definition shape | `Id`, `Name`, `Description`, `Trigger`, `Threshold`, `Reward`, `PrerequisiteQuestDefinitionId?`, `ProgressBaseline`, `IsActive`. |
| Trigger | Enum: `GameCompletedTotal`, `GameCompletedDaily`, `AuthProviderLinked`. Fixed at 3 — extending requires a domain + handler change. |
| Threshold | Positive int. Validated by `CreateQuestDefinitionCommandValidator`. |
| Progress baseline | Enum: `FromSnapshot` (count starts at 0 from the player's counter snapshot at issuance) or `FromExistingTotal` (count from absolute counter, useful for retroactive milestones). Admin picks per definition. Only meaningful for `GameCompletedTotal`; ignored for Daily/AuthLinked. |
| Prerequisite | Single FK to another `QuestDefinition`. Cycles rejected by validator. Deactivated prerequisites break the chain (downstream quests never issue). |
| Issuance strategy | **Lazy / pull-based.** `GET /quests/me` performs a sync pass that issues missing eligible PlayerQuests and deletes expired Daily rows in one round-trip. No broadcast on admin create. No `PlayerRegisteredIntegrationEvent` handler. |
| Progress storage | **Not stored.** `PlayerQuests.Progress` column removed. Progress is computed at read time as `min(player_counter - baseline_snapshot, threshold)` where `player_counter` comes from Stats. |
| Daily expiry | Expired-and-unclaimed Daily PlayerQuest rows are **deleted** at every sync, not preserved. Stats / leaderboard already tracks long-term game-completion data; quest expiry rows have no audit value. |
| Counter source | Cross-module read via `IQuestCounterReader` (Application contract, API host adapter). Reads `stats.PlayerStats.GamesCompleted` (Total), `stats.PlayerPeriodStats` (Daily, `PeriodType='Daily' + PeriodStartDate=today UTC`), and `players.PlayerAuthIdentities` (count > 0 = linked). |
| Admin manual issue | **Removed.** `POST /admin/quests/players/{id}/issue` and `IssueQuestToPlayerCommand` deleted. With lazy issuance + chain prereq, an admin "force-issue" path has no remaining use case. |
| Hardcoded event handlers | **Removed.** `GameCompletedIntegrationEventHandler` and `AuthProviderLinkedIntegrationEventHandler` deleted. Stats module already maintains the counters Quests projects from; Quests no longer needs to participate in real-time event processing. |

### Architecture notes

- **Counter ownership stays in Stats.** Quests becomes a pure consumer:
  it reads from Stats' projection tables on every quest list query.
  Module isolation preserved — Quests holds no real-time counter
  state.
- **No scheduler needed for Daily reset.** Lazy issuance + delete-
  expired-on-sync means a player who returns after a week sees today's
  Daily quest issued on the next `GET /quests/me`, and never sees the
  stale week-old rows.
- **Microservice extraction unaffected.** `IQuestCounterReader` is the
  same sync-gateway pattern as `IEnergyGuard` and `IAdminLookup` — a
  Quests.Application contract with the implementation in the API
  composition root. Quests module remains structurally isolated.
- **Schema migration is destructive.** Existing PlayerQuests and
  QuestDefinitions rows are dropped and recreated. There are no
  production players today, so no data preservation is required.
  Recreated schema seeds zero rows; admin must define quests via the
  admin panel (or DbUp seeds a fresh chain — TBD per Q1.2).

### Slice plan

#### Q1.1 — Domain reshape (target: 30 min)

- Remove `QuestType.cs` enum. Add `QuestTrigger.cs` (3 values) and
  `ProgressBaseline.cs` (2 values).
- `QuestDefinition`: replace `QuestType`, `Goal`, `RewardAmount`,
  `PrerequisiteQuestType` with new fields. `Name` (non-empty, ≤ 64),
  `Description` (≤ 256), `Trigger`, `Threshold` (> 0), `Reward` (> 0),
  `PrerequisiteQuestDefinitionId` (nullable Guid), `ProgressBaseline`.
  `Activate` / `Deactivate` / `Update` mutators stay.
- `PlayerQuest`: identity by `QuestDefinitionId` (FK), drop `QuestType`
  + `Progress` columns from the aggregate, add
  `ProgressBaselineSnapshot` (int) captured at issuance.
- Domain events updated: `PlayerQuestIssuedDomainEvent`,
  `PlayerQuestClaimedDomainEvent`, etc. carry
  `QuestDefinitionId` instead of `QuestType`.
- Domain rules updated: prerequisite chain cycle check (recursive
  pre-check on Create / Update).

#### Q1.2 — Schema migration via DbUp (target: 30 min)

- `quests/Tables/010_PlayerQuests.sql` rewrite: drop `QuestType`,
  `Progress` columns; add `QuestDefinitionId` FK + index +
  `ProgressBaselineSnapshot`. Unique constraint
  `UX_PlayerQuests_PlayerId_QuestDefinitionId` for idempotent
  issuance.
- `quests/Tables/020_QuestDefinitions.sql` rewrite: add `Name`,
  `Description`, `Trigger`, `ProgressBaseline`,
  `PrerequisiteQuestDefinitionId` FK; drop `QuestType` column +
  unique index; drop `Goal`/`RewardAmount` rename to
  `Threshold`/`Reward` (or keep column names — decide at slice time).
- `quests/Tables/021_SeedQuestDefinitions.sql` rewrite: optional —
  either seed zero rows (admin creates everything) or seed a default
  6-step game-completion chain (1 → 3 → 5 → 10 → 50 → 100). Decide
  with user at slice start.
- View `v_PlayerQuests` updated to expose `QuestDefinitionId`.

#### Q1.3 — Application reshape (target: 1.5 h)

- `IssueQuestCommand` takes `QuestDefinitionId`. Handler reads the
  current counter (Total or Daily, depending on definition's
  `Trigger`) from the new `IQuestCounterReader`, captures the
  baseline snapshot, persists PlayerQuest. Prereq check stays.
- `GetActiveQuestsQueryHandler` rewritten as a two-pass operation:
  - **Sync pass:** iterate active definitions; for each eligible
    (prereq null or prereq claimed by this player) definition, insert
    missing PlayerQuest with baseline snapshot. Delete expired
    Daily rows.
  - **Read pass:** join definitions + player counters + PlayerQuests;
    compute `progress = min(counter - baseline, threshold)` and
    `state` (Active / ReadyToClaim / Claimed). Return DTOs.
- Remove `RecordQuestProgressCommand` + handler + validator
  (progress is computed, never written).
- Remove `GameCompletedIntegrationEventHandler`,
  `AuthProviderLinkedIntegrationEventHandler`,
  `PlayerRegisteredIntegrationEventHandler` (all hardcode quest
  types and / or eager-issue; no longer needed).
- Remove `IssueQuestToPlayerCommand` + handler + endpoint.
- New `IQuestCounterReader` contract under
  `Application/Configuration/CrossModule/` with three reads.
- `IQuestCatalog` shrinks to definition lookup by id;
  `ResolveAsync(QuestType)` removed.

#### Q1.4 — Cross-module counter reader (target: 30 min – 1 h)

- API host adapter `QuestCounterReader : IQuestCounterReader` in
  `LexiLink.API/CrossModule/`. Uses `ISqlConnectionFactory` + Dapper
  to query `stats.*` and `players.*` directly (analogous to how
  `EnergyGuard` calls into Energy via the events bus, but here we
  query view tables directly because the read is hot-path and
  trivial).
- Autofac registration in API composition root.
- Quests.Application contract sits in
  `Configuration/CrossModule/IQuestCounterReader.cs`.

#### Q1.5 — API endpoints reshape (target: 20 min)

- `AdminQuestEndpoints`: `Create` body becomes `{ name, description,
  trigger, threshold, reward, prerequisiteQuestDefinitionId,
  progressBaseline }`. `Update` body same minus name/trigger
  (trigger and name immutable post-create per Q1.1 decision; revisit
  if user wants editable names). `Reactivate` / `Deactivate` /
  `Get` paths unchanged in shape.
- Remove `POST /admin/quests/players/{playerId}/issue` and
  `POST /admin/quests/players/{playerId}/{playerQuestId}/reset`
  (admin manual-issue removed entirely per locked decisions).
- `GET /quests/me` unchanged externally; internally now runs the
  lazy sync.

#### Q1.6 — Frontend reshape (target: 1 h)

- Frontend `AdminQuestType` enum **removed**. Quest type is now a
  `QuestTrigger` enum (3 values) shipped with the AdminQuestsRepository.
- Admin create form fields:
  - `Name` text
  - `Description` multi-line text
  - `Trigger` dropdown (3 fixed)
  - `Threshold` numeric input
  - `Reward` numeric input
  - `Progress baseline` dropdown (visible only when
    `Trigger=GameCompletedTotal`)
  - `Prerequisite` dropdown — populated from other active definitions
    (excludes self in Edit mode); "None" option present
- Admin row UI: shows `Name` + `Trigger.Threshold` (e.g. "Bronz —
  3 oyun") + reward badge. Active/Inactive badge. Edit / Deactivate
  / Reactivate icons (existing). No type/cadence column anymore.
- Player `/quests` screen: definition `Name` + `Description` displayed;
  progress bar + threshold; existing Claim button.
- Frontend roadmap doc reference: see `frontendRoadmap.md > Slice
  Q1.6`.

#### Q1.7 — Tests (target: 1.5 h)

- Replace all `QuestType.*` references in tests with
  `QuestDefinitionId` (Guid) or trigger constants.
- Domain unit tests: chain prereq cycle rejection; daily expire
  computation; issue idempotency on unique constraint;
  baseline-snapshot correctness.
- Application unit tests: `IssueQuestCommandHandler` (baseline
  capture, prereq honor), `GetActiveQuestsQueryHandler` (sync pass
  insert + delete-expired; read pass projection math).
- Integration tests: full chain scenario — admin creates chain
  (threshold 1 → 3 → 5), player registers, plays 1 game → quest 1
  ReadyToClaim, claims → quest 2 appears on next sync, plays 3 games
  → quest 2 ReadyToClaim. Daily quest expiry happens at midnight.
- API tests: validation problem details for invalid Create payloads.
- Frontend tests: admin form behaviors; player screen rendering of
  computed progress.

#### Q1.8 — Manual sync + commit (target: 30 min)

- Stack restart with the new schema.
- Admin creates a chain via UI (1, 3, 5, 10 GameCompletedTotal).
- Open guest player; quests page shows only quest 1 active.
- Play 1 game; refresh quests → quest 1 ReadyToClaim; claim;
  refresh → quest 2 appears with progress 0.
- Repeat through chain; verify Daily creation/expiry.
- Commit per-slice (Q1.1 ... Q1.8 — eight commits).
- Update `progress.md`, `activeContext.md`, `frontendActiveContext.md`,
  `frontendProgress.md`, `GLOSSARY.md` (new terms: Trigger, Threshold,
  ProgressBaseline, lazy issuance, counter reader).

### Risk / open questions

- **Default seed?** If we ship zero seeded quests, the player onboarding
  experience starts with an empty quest list — fine for development,
  but production launch should ship a meaningful default chain via
  DbUp. Decide at Q1.2 start.
- **Trigger extensibility.** Adding a new trigger (e.g. "category Spor
  completed 10 times") requires a domain + counter reader extension.
  Not a Q1 concern — design keeps the path open by isolating reads
  behind `IQuestCounterReader`.
- **Editing immutable fields.** Per Q1.5, `Name` and `Trigger` are
  fixed after Create. If the user wants editable names later, the
  decision is reversible — just expand the Update contract.

### Acceptance

- Admin can create a new quest definition via the admin panel with a
  custom name and arbitrary threshold; the definition is visible to
  every eligible player on their next `GET /quests/me` call without
  any background processing.
- Player quest list reflects real-time game-completion progress
  computed from Stats counters; no manual progress recording is
  required anywhere in the codebase.
- All 368+ tests pass after Q1.7 with the new identity model.
- Frontend admin form lets the operator build a 6-step chain
  (1 → 3 → 5 → 10 → 50 → 100) end-to-end and verify the chain
  unlocks step by step via manual play.

---

## Beyond Sprint 7

- **Architecture alignment pass (current)** — Kamil comparison sonrası eksikler sırayla kapatılıyor. Slice 1 ArchTests baseline, Slice 2 API module facade, Slice 3 module startup APIs ve Slice 4 no-repeat completed start-target pairs tamamlandı.
- **Stats module** (next / Sprint 8) — Integration Events + Outbox/Inbox eksikliğini kapatacak sıradaki adım. Games + Players event'lerini dinleyen üçüncü modül olacak; `Stats.Infrastructure` ilk gerçek cross-module consumer olacak. BiDictionary `IDomainEventNotification<T>` mapping'leri, outbox processor ve inbox/idempotency burada anlamlanacak.
- **Authentication middleware** (Sprint 9+) — Apple/Google ID token doğrulama, JWT issue/refresh, API host'ta `JwtBearer` ya da custom auth handler. `IExecutionContextAccessor.UserId` claim'lerden gerçekten dolar.
- **Push notifications** (Sprint 10+) — FCM/APNs token saklama (`Players` aggregate'ine `DeviceInfo` owned VO eklenir) + ayrı `Notifications` modülü.
- **Integration Events / Outbox-Inbox processor (Quartz)** — Stats modülüyle birlikte gelir; her modül kendi `ProcessOutboxJob`'unu işletir.
- **Read-model projection / CQRS denormalization** — leaderboard / streak view'leri Stats modülünde.
- **Event sourcing** — explicitly *not* on the path. Game state is durable; events are a notification mechanism, not the source of truth.

---

## See Also

- `progress.md` — what's been delivered and when.
- `activeContext.md` — current sprint focus.
- `SKILLS.md` — the principles every sprint follows.
