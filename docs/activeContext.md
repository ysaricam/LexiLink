# activeContext.md

Project'in o anki yönü ve en yakın sıra. Geçmiş teslimatlar `progress.md`,
uzun vadeli plan `ROADMAP.md`, mimari karşılaştırma notları
`kamil-modular-monolith-comparison.md` içindedir.

> Last updated: 2026-05-26 (Sprint H — Hint Module + Quest Multi-Reward — closed end-to-end; eight slices delivered with operator-confirmed manual verification).

---

## Active Sprint

**Sprint H — Hint Module + Quest Multi-Reward — closed.** Eight
slices (H1 → H8) shipped on 2026-05-25 → 2026-05-26 with per-slice
operator approval and standalone commits. Manual stack verification
on 2026-05-26 passed all golden flows (multi-reward quest claim,
Game UseHint free→inventory fall-through, admin hint console).
Final quality gate: **348 .NET tests + 103 Flutter tests green**.

Goal (delivered): extracted player hints out of `Game.HintAllowance`
into a dedicated **Hint module** (sixth business module —
`PlayerHintInventory` aggregate) and reshaped `QuestReward` into a
`(EnergyReward, HintReward)` pair so a single quest can deliver
either or both. The per-game free hint quota stays inside Games
(fixed at 1 across all difficulties); when exhausted,
`UseHintCommandHandler` falls through to
`IHintGuard.EnsureHintAvailableAsync(playerId)` (sync gateway). The
adapter (`LexiLink.API/CrossModule/HintGuard.cs`) translates this to
a `ConsumePlayerHintCommand` on the Hint module — empty inventory
breaks `HintBalanceMustBeSufficientRule` and the puzzle does not
advance.

Multi-reward delivery uses **two independent outbox consumers** each
guarding on its reward's positivity:
`Energy.Application/QuestClaimedIntegrationEventHandler` skips when
`EnergyReward == 0`, and the new
`Hint.Application/QuestClaimedIntegrationEventHandler` grants hints
when `HintReward > 0`. This is LexiLink's **second reverse
cross-module event dependency** (after Energy's existing
consumer of `Quests.IntegrationEvents`).

Detailed slice plan in `ROADMAP.md > Sprint H`; per-slice delivery
notes in `progress.md > Sprint H` and `frontendProgress.md > Slice
H6`.

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

- **HintsUsed counts only the free quota by design.** The
  per-game `HintAllowance.Used` counter is unrelated to player
  inventory consumption. `UseHintWithExternalInventory()`
  deliberately does **not** advance the counter. Surfaces in
  `GameDetailsDto.HintsUsed` as the *free* count, not the *total*
  count. Codified in `UseHintFallThroughTests`.
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

## Next Action

**Sprint H — Hint Module + Quest Multi-Reward — kapandı.** Sekiz
slice'ın tamamı commit'lendi; manuel doğrulama 2026-05-26 günü
operator tarafından geçildi. Quality gate 348/348 .NET + 103/103
Flutter.

Sprint H aday listesi tükendi. Sonraki sprint adayları `ROADMAP.md
> Beyond Sprint 7` bölümünde:

- **Power-up / shop ekonomisi.** Enerji + ipucu artık iki ayrı
  para birimi gibi davranıyor — alış-veriş ekranı, IAP veya
  reklam-temelli bonus akışları doğal sonraki adım.
- **Quest yeni trigger türleri.** Şu an `GameCompletedTotal /
  Daily / AuthProviderLinked` üçlüsü var. Operator backlog'unda
  `StreakReached`, `CategoryMastered` gibi adaylar var (ayrı
  sprint olarak değerlendirilecek).
- **Tutorial flow.** Yeni guest player'a 1-2 puzzle'lık rehberli
  giriş; quest sistemine ısıtmak için "Hint öğren" + "Enerji
  öğren" tarzı sıfır-zorluk puzzle'larla.

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
