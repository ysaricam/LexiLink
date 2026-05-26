# progress.md

History log of delivered work. Newest at top. Append entries when significant work lands; don't rewrite the past.

---

## Sprint UR — Undo + Reset Modules + Quest 4-Reward (2026-05-26, closed)

Ninth full-stack sprint. Extracted Undo and Reset into two new
Kamil-faithful inventory modules, **eliminated** their per-game free
quota from Games (unlike Hint, every call goes through the sync
gateway), and expanded quest rewards from `(Energy, Hint)` to
`(Energy, Hint, Undo, Reset)`. Eight slices delivered in four
commits — UR1–UR5 bulk (`ba9e42d`), UR6 admin (`725c7d2`), UR7
frontend (`277fcad`), UR8 admin IT tests + docs (this commit).

Final quality gate: **464 .NET tests + 103 Flutter tests green**.
Detailed plan in `ROADMAP.md > Sprint UR`; per-slice delivery
notes below; frontend slice detail in
`frontendProgress.md > Slice UR7`.

### Slice UR8 — Admin command IT tests + docs (2026-05-26, this commit)

- New `UndoAdminCommandTests` + `ResetAdminCommandTests` mirror
  `HintAdminCommandTests`: NonAdmin reject, Set / GrantBonus /
  Reset with audit row assertions. 8 cases total.
- Undo.IT + Reset.IT `TestBase` extended with
  `AdministrationStartup` boot and
  `administration.AdminActionAudit` cleanup so the outbox-
  published audit row actually lands in the test DB.
- Final backend test count: **464 pass** (Hint 27, Undo 29, Reset
  29).
- Docs polish: this `progress.md` entry; `activeContext.md > Active
  Sprint` pivots from "planning closed" to "Sprint UR closed";
  `GLOSSARY.md` updates for `QuestDefinition` 4-reward,
  `PlayerQuest.Claim` 4-arg, `QuestClaimedIntegrationEvent`
  4-consumer fan-out, widened
  `QuestRewardMustHaveAtLeastOnePositiveRule`;
  `frontendActiveContext.md` + `frontendProgress.md > Slice UR7`
  added.
- Manual verification: operator runs four golden flows — single-
  reward quest claim × 4 (one per reward type), mixed-reward
  quest, empty-inventory fall-through rejection, admin Set / Grant
  / Reset on both new consoles with audit log assertions.

### Slice UR7 — Frontend reshape (2026-05-26, 277fcad)

See `frontendProgress.md > Slice UR7` for the full slice.
Highlights:

- Two new player features (`lib/features/undo/` +
  `lib/features/reset/`) and two new admin consoles
  (`lib/features/admin_undo/` + `lib/features/admin_reset/`).
- HomeScreen now hosts **4** badges in the top-right row (Reset /
  Undo / Hint / Energy). Each uses a distinct M3 color token so
  the four inventories are visually distinguishable at a glance.
- Admin quest form gains 2 new number inputs (Geri al ↶ + Sıfırla
  ↻); form-level at-least-one-positive rule spans all four
  rewards.
- Player quest tile switched from a fixed Row to `Wrap` so all 4
  reward badges flow on narrow screens.
- 5 quest-area test files reshaped to include `undoReward` +
  `resetReward`. 103/103 Flutter tests pass; `flutter analyze`
  shows no new errors.

### Slice UR6 — Admin operations + GET endpoints + audit (2026-05-26, 725c7d2)

- 6 new admin commands (Set / GrantBonus / Reset per module)
  marked `IAdminCommand` with `AuditTargetType =
  "{Undo,Reset}.PlayerXInventory"`. `ResetPlayerResetCommand`'s
  double-Reset naming is intentional (outer verb, inner
  aggregate).
- 2 new player queries (`GetPlayer{Undo,Reset}Query` + handler +
  Dapper SQL + `PlayerXSnapshotDto`).
- 7th + 8th per-module copies of
  `AdminAuditingCommandHandlerDecorator`.
  `{Undo,Reset}AdminActionPerformedNotification` + handler
  publishes `AdminActionPerformedIntegrationEvent` through each
  module's own outbox.
  `{Undo,Reset}.Infrastructure.csproj` add granular
  references to `Administration.IntegrationEvents`; ArchTests
  updated to allow.
- API endpoints: `GET /undo/me`, `GET /reset/me` (player);
  `GET /admin/players/{id}/{undo,reset}` +
  `POST .../{set,grant,reset}` (admin). Program.cs wires all
  four MapXEndpoints calls.

### Slice UR5 — Quest 4-reward (destructive) (2026-05-26, part of ba9e42d)

- `QuestDefinition._undoReward + _resetReward`; `Create` +
  `Update` signatures expand to 4 reward parameters.
- `QuestRewardMustHaveAtLeastOnePositiveRule` widens across all
  4 fields — each ≥ 0, at least one > 0.
- `PlayerQuestClaimedDomainEvent` + `QuestClaimedIntegrationEvent`
  carry all 4 reward fields.
- `PlayerQuest.Claim(now, ready, energyReward, hintReward,
  undoReward, resetReward)`.
- 2 new outbox consumers in
  `Undo.Application/PlayerUndoInventories/ProcessIntegrationEvents/`
  and the Reset twin: each guards on its reward's positivity and
  dispatches `Grant{Undo,Reset}Command` (which calls
  `Player*Inventory.GrantBonus` — same over-cap-allowed bonus
  semantics as Hint).
- `GrantUndoCommand` + `GrantResetCommand` added.
- DbUp `quests/050_ExpandQuestRewardsWithUndoReset.sql` —
  idempotent ALTER ADD COLUMN with `IF NOT EXISTS`. Canonical
  `020_QuestDefinitions.sql` + `021_SeedQuestDefinitions.sql`
  also updated for cold-start DBs.
- All admin Quest commands / validators / DTOs / endpoint
  requests + EF mapping + outbox notification + integration
  events reshape to carry 4 reward fields. Existing
  `QuestRewardDeliveryTests` in Energy.IT + Hint.IT now assert
  the 4-field shape; new `QuestRewardDeliveryTests` in Undo.IT +
  Reset.IT verify the new consumers fire only when their
  reward field is > 0.

### Slice UR4 — Sync gateway integration (2026-05-26, working tree)

- Added `ConsumePlayerUndoCommand` and `ConsumePlayerResetCommand`
  with validators and handlers. Handlers load the player inventory,
  call aggregate `Consume(amount, now)`, and surface empty-balance
  business-rule failures.
- Replaced API host no-op `UndoGuard` and `ResetGuard` stubs with real
  composition-root adapters. Each adapter calls its module facade with
  `ConsumePlayer{Undo,Reset}Command(playerId, 1)`.
- Games integration test composition root now registers configurable
  `RecordingUndoGuard` and `RecordingResetGuard` stubs, matching the
  Hint recording guard pattern.
- Added Games.IT `UseUndoFallThroughTests` and `ResetFallThroughTests`.
  They prove every in-game Undo/Reset call invokes the gateway, guard
  rejection happens before Game mutation/counter increment, and success
  keeps the existing history/counter behavior.
- Undo/Reset integration lifecycle tests now cover consume success and
  consume-from-zero rejection. Raw SQL balance seeding clears the EF
  change tracker before command execution so the test observes database
  state rather than a stale tracked aggregate.
- No DbUp migration was needed for UR4; it uses the UR2 inventory
  tables.
- Quality gate passed: `dotnet build LexiLink.sln --no-restore
  --disable-build-servers -m:1 /clp:ErrorsOnly`, Undo.Tests 19/19,
  Reset.Tests 19/19, Games.IntegrationTests 34/34,
  Undo.IntegrationTests 4/4, Reset.IntegrationTests 4/4,
  ArchitectureTests 55/55, and full
  `./scripts/test.sh --no-restore --no-build /clp:ErrorsOnly` green.

### Slice UR3 — Lazy init from PlayerRegistered (2026-05-26, working tree)

- Added `EnsurePlayerUndoInventoryExistsCommand` and
  `EnsurePlayerResetInventoryExistsCommand` with validators and
  idempotent handlers. Existing inventory rows short-circuit; missing
  rows initialize with the module's configured initial balance.
- Added `IUndoConfigurationService.InitialBalance` and
  `IResetConfigurationService.InitialBalance` in Domain, plus
  Infrastructure implementations reading `Undo:InitialBalance` and
  `Reset:InitialBalance` with default 0.
- Added `PlayerRegisteredIntegrationEventHandler` consumers in Undo
  and Reset Application. Both consume only the public
  `Players.IntegrationEvents` contract and dispatch their own ensure
  command through the module facade.
- Undo/Reset Application projects now reference
  `LexiLink.Modules.Players.IntegrationEvents`; Architecture tests were
  adjusted to allow that granular public-contract dependency while
  keeping Players Domain/Application/Infrastructure forbidden.
- Added Undo and Reset integration test bases and lifecycle tests:
  new guest registration creates `undo.PlayerUndoInventories` and
  `reset.PlayerResetInventories` after outbox processing, and replayed
  `PlayerRegisteredIntegrationEvent` messages do not duplicate rows.
- `scripts/test.sh` now runs Undo and Reset integration test projects.
- Quality gate passed: `dotnet build LexiLink.sln --no-restore
  --disable-build-servers -m:1 /clp:ErrorsOnly`, Undo.Tests 19/19,
  Reset.Tests 19/19, Undo.IntegrationTests 2/2,
  Reset.IntegrationTests 2/2, ArchitectureTests 55/55, and full
  `./scripts/test.sh --no-restore --no-build /clp:ErrorsOnly` green.

### Slice UR2 — Undo + Reset module foundation (2026-05-26, working tree)

- Added two Kamil-faithful module skeletons:
  `src/Modules/Undo/{Domain,Application,Infrastructure,Tests,IntegrationTests}/`
  and
  `src/Modules/Reset/{Domain,Application,Infrastructure,Tests,IntegrationTests}/`.
  Ten new projects were added to `LexiLink.sln`.
- Added `PlayerUndoInventory` and `PlayerResetInventory` aggregates.
  Both use `Id == PlayerId`, a single `_balance`, no maximum cap, and
  no refill timer. Every in-game Undo/Reset call is intended to spend
  persistent inventory through the future real guard adapter.
- Added domain rules/events/repositories for both modules:
  `{Undo,Reset}AmountMustBePositiveRule`,
  `{Undo,Reset}AmountMustBeNonNegativeRule`,
  `{Undo,Reset}BalanceMustBeSufficientRule`, initialized/consumed/
  granted/admin-set/admin-reset domain events, and repository
  contracts.
- Added per-module Application contracts (`IUndoModule`,
  `IResetModule`, command/query base contracts and handler
  interfaces).
- Added per-module Infrastructure foundation: EF context, schema
  startup, Autofac module, module facade, UnitOfWork, domain event
  dispatcher, logging/validation/unit-of-work decorators, outbox
  accessor/table mapping, repository implementation, and aggregate
  mapping. Admin auditing decorators and quest reward consumers are
  intentionally deferred to later UR slices.
- DbUp scripts added and applied locally:
  `{undo,reset}/Schema/001_CreateSchema.sql`,
  `{undo,reset}/Tables/010_Player{Undo,Reset}Inventories.sql`, and
  `{undo,reset}/Tables/070_OutboxMessages.sql`.
- API composition root now initializes Undo and Reset startups, checks
  their mappings, and references both Infrastructure projects. The
  temporary no-op Games guard adapters from UR1 remain in place until
  UR4.
- Architecture tests now include Undo/Reset assemblies, aggregate
  naming checks, API composition-root boundaries, and per-layer
  dependency rules. `scripts/test.sh` now runs Undo and Reset unit test
  projects.
- iCloud duplicate `* 2.sql` resource copies were cleaned from source
  and build output after DbUp reported duplicate/missing embedded
  migration names.
- Quality gate passed: `dotnet build LexiLink.sln --no-restore
  --disable-build-servers -m:1 /clp:ErrorsOnly`,
  Undo.Tests 19/19, Reset.Tests 19/19, ArchitectureTests 55/55, and
  full `./scripts/test.sh --no-restore --no-build /clp:ErrorsOnly`
  green.

### Slice UR1 — Game.cs destructive reshape (2026-05-26, working tree)

- Deleted `UndoAllowance` + `ResetAllowance` VOs, their
  `*MustHaveRemainingRule` classes, and their unit tests.
- `Game` now keeps `_undosUsed` + `_resetsUsed` as plain int counters.
  `Undo()` delegates to `UseUndoWithExternalInventory()` and
  `ResetToStart()` delegates to `ResetWithExternalInventory()`;
  both methods preserve the existing state/history behavior, emit the
  same domain events, and increment only the usage counter.
- `Complete()` scoring now reads the plain counters instead of
  allowance VO `.Used`.
- `GameEntityTypeConfiguration` maps `UndosUsed` + `ResetsUsed` as
  plain columns; `UndosRemaining` + `ResetsRemaining` owned mappings are
  gone.
- Added `IUndoGuard` + `IResetGuard` contracts in Games.Application.
  `UndoCommandHandler` and `ResetCommandHandler` now always call the
  corresponding gateway before mutating Game. API host registers
  temporary no-op `UndoGuard` + `ResetGuard`; real adapters land in UR4.
- Games, Quests, and Stats integration-test composition roots register
  always-allowing Undo/Reset guard stubs so those modules still boot
  without the new inventory modules.
- DbUp `games/Tables/050_DropUndoResetAllowanceColumns.sql` drops the
  old remaining columns idempotently. Canonical `040_Games.sql` and
  `130_v_Games.sql` were updated; `v_Games` keeps `UndosTotal` and
  `ResetsTotal` as temporary compatibility projections for the current
  frontend.
- API quest endpoint test was made resilient to local admin-created
  active quest definitions: it now asserts that the seeded daily quest
  is present, not that it is the only active quest.
- Local `lexilink` DB was upgraded with the UR1 DbUp script. Quality
  gate passed: `dotnet build LexiLink.sln --no-restore
  --disable-build-servers -m:1 /clp:ErrorsOnly` and
  `./scripts/test.sh --no-restore --no-build /clp:ErrorsOnly` green.

## Sprint H — Hint Module + Quest Multi-Reward (2026-05-25 → 2026-05-26)

Eighth full-stack sprint. Extracted Hint out of `Game.HintAllowance`
into its own per-module aggregate (`PlayerHintInventory`) mirroring
the Energy template, and reshaped `QuestReward` into a `(Energy,
Hint)` pair with an at-least-one-positive rule. Eight slices
(H1 → H8), each committed standalone behind operator approval.
Manual stack verification on 2026-05-26 (operator-confirmed). Final
quality gate: **348 .NET tests + 103 Flutter tests green** (Hint
contributes 19 domain + 8 integration; Games.IT adds 3 UseHint
fall-through scenarios via `RecordingHintGuard`).

Per-game free hint quota stays in Games (fixed at 1 across all
difficulties — `IGameConfigurationService.ResolveHints` flattened).
When the free quota is exhausted, `UseHintCommandHandler` falls
through to `IHintGuard.EnsureHintAvailableAsync(playerId)` (sync
gateway), which the API host adapter translates to a
`ConsumePlayerHintCommand` on the Hint module. Insufficient
inventory surfaces `HintBalanceMustBeSufficientRule` and the puzzle
state does not advance.

Multi-reward quests deliver via **two independent outbox consumers**
each guarding on its reward's positivity: Energy's existing
`QuestClaimedIntegrationEventHandler` now skips when
`EnergyReward == 0`, and the new
`Hint.Application/PlayerHintInventories/ProcessIntegrationEvents/QuestClaimedIntegrationEventHandler`
grants hints when `HintReward > 0`. `PlayerHintInventory.GrantBonus`
intentionally permits over-cap balance (parallels Energy's bonus
path). This introduces LexiLink's **second reverse cross-module
event dependency** (`Hint.Application → Quests.IntegrationEvents`,
mirroring Energy's pattern).

Detailed slice plan + decisions live in
`ROADMAP.md > Sprint H — Hint Module + Quest Multi-Reward`. Frontend
slice detail in `frontendProgress.md > Slice H6`.

### Slice H1 — Hint module foundation (2026-05-25, 6a52cf0)

- Five new csproj (Domain / Application / Infrastructure / Tests /
  IntegrationTests) following the Energy module template
  exactly. Per-module Autofac module + Startup +
  UnitOfWork + DomainEventsDispatcher + the standard decorator
  chain (Logging / Validation / UnitOfWork; admin auditing added
  later in H5).
- Aggregate `PlayerHintInventory` (`Domain/PlayerHintInventories/`)
  identified by `PlayerHintInventoryId` (same Guid as owning
  `PlayerId`). Single `int _balance`. Lifecycle: internal
  `InitializeFor(playerId, initialBalance)`. No max cap, no refill
  timer — hints are earned, not regenerated.
- Three rules: `HintAmountMustBePositiveRule`,
  `HintAmountMustBeNonNegativeRule`,
  `HintBalanceMustBeSufficientRule`.
- One domain event so far:
  `PlayerHintInventoryInitializedDomainEvent`.
- DbUp: `hint/Schema/001_CreateSchema.sql`,
  `hint/Tables/010_PlayerHintInventories.sql`,
  `hint/Tables/070_OutboxMessages.sql`.

### Slice H2 — Player registration → Hint init (2026-05-25, ae662ab)

- `EnsurePlayerHintInventoryExistsCommand` + handler + validator.
  Idempotent: handler short-circuits if an inventory already
  exists for the player.
- `PlayerRegisteredIntegrationEventHandler` in
  `Hint.Application/PlayerHintInventories/ProcessIntegrationEvents/`
  dispatches the ensure command. Mirrors Energy's lazy-init
  pattern.
- `IHintConfigurationService.InitialBalance` (Domain interface)
  +`HintConfigurationService` (Infrastructure impl) reads
  `Hint:InitialBalance` from config; default 0.

### Slice H3 — IHintGuard sync gateway + Game.UseHint refactor (2026-05-25, eb13748)

- `Games.Application/Configuration/CrossModule/IHintGuard.cs` —
  `EnsureHintAvailableAsync(playerId, ct)`. Contract lives in
  Games so Games depends only on its own surface.
- `LexiLink.API/CrossModule/HintGuard.cs` — adapter calling
  `IHintModule` with `ConsumePlayerHintCommand`. Composed in the
  API host; same pattern as `EnergyGuard`.
- `ConsumePlayerHintCommand` + handler + validator in
  `Hint.Application/PlayerHintInventories/ConsumePlayerHint/`.
- `Game.HasFreeHintRemaining` property + new
  `Game.UseHintWithExternalInventory()` method (does not touch the
  per-game allowance but emits the same `HintUsedDomainEvent`).
- `UseHintCommandHandler` branches on
  `HasFreeHintRemaining`: free → `game.UseHint()`; otherwise →
  `_hintGuard.EnsureHintAvailableAsync` then
  `game.UseHintWithExternalInventory()`.
- `StandardGameConfigurationService.ResolveHints(difficulty)`
  flattened to a fixed 1 — free quota no longer depends on
  difficulty.
- `Games.IT`, `Quests.IT`, `Stats.IT` TestBases received an
  `AlwaysAllowingHintGuard` stub. Games.IT later upgraded to a
  configurable `RecordingHintGuard` in H7.

### Slice H4 — Quest multi-reward (destructive) (2026-05-25, c74ca87)

- `QuestRewardMustHaveAtLeastOnePositiveRule` replaces
  `QuestRewardMustBePositiveRule`: at least one of
  `(EnergyReward, HintReward)` must be > 0; both ≥ 0.
- `QuestDefinition` now holds `_energyReward` + `_hintReward`.
  `Create` and `Update` both take the pair.
- `PlayerQuestClaimedDomainEvent` and
  `QuestClaimedIntegrationEvent` reshape from `Reward` to
  `(EnergyReward, HintReward)` — wire-level breaking change.
- `PlayerQuest.Claim(now, isReadyToClaim, energyReward,
  hintReward)`.
- `Energy.Application/QuestClaimedIntegrationEventHandler` guards
  on `EnergyReward > 0`.
- New
  `Hint.Application/PlayerHintInventories/ProcessIntegrationEvents/QuestClaimedIntegrationEventHandler`
  guards on `HintReward > 0` and dispatches `GrantHintCommand`.
  `Hint.Application.csproj` adds a reference to
  `Quests.IntegrationEvents` (granular ArchTest allow).
- `GrantHintCommand` + handler + validator. Calls
  `PlayerHintInventory.GrantBonus` (no max cap — hints accumulate
  freely).
- DbUp `quests/Tables/040_ReshapeQuestRewardsForSprintH.sql`:
  idempotent ALTER COLUMN RENAME (`Reward` →
  `EnergyReward`) + ADD COLUMN `HintReward` with
  information_schema guards so fresh DBs short-circuit. Canonical
  `020_QuestDefinitions.sql` + `021_SeedQuestDefinitions.sql`
  also updated for cold-start DBs.
- All admin commands / validators / DTOs / endpoint requests +
  EF mapping + outbox notification handler reshape.

### Slice H5 — Hint admin operations + GET endpoints + audit (2026-05-25, be0b8e1)

- `PlayerHintInventory.AdminSet(newBalance, now)` +
  `AdminReset(now)` domain methods +
  `PlayerHintAdminSetDomainEvent` +
  `PlayerHintAdminResetDomainEvent`.
- Three admin commands marked `IAdminCommand` with
  `AuditTargetType => "Hint.PlayerHintInventory"`:
  `SetPlayerHintCommand`, `GrantBonusHintCommand` (wraps
  `GrantHintCommand`), `ResetPlayerHintCommand`.
- `GetPlayerHintQuery` + handler (Dapper) +
  `PlayerHintSnapshotDto(PlayerId, Balance)` for the player UX
  GET endpoint.
- Per-module copy of `AdminAuditingCommandHandlerDecorator`
  (5th — same template as Quests/Energy/Administration).
- `HintAdminActionPerformedNotification` + handler publishes
  `AdminActionPerformedIntegrationEvent` through the Hint outbox.
  `Hint.Infrastructure.csproj` adds reference to
  `Administration.IntegrationEvents` (granular ArchTest allow).
- API endpoints:
  - `GET /hint/me` (AuthenticatedPlayer) → `PlayerHintSnapshotDto`.
  - `GET /admin/players/{id}/hint` +
    `POST .../set | grant | reset` (AuthenticatedAdmin).

### Slice H6 — Frontend reshape (2026-05-25, 4109a93)

See `frontendProgress.md > Slice H6` for the full slice. Highlights:

- Two new Flutter features: `lib/features/hint/` (player —
  `PlayerHint` DTO + `HintRepository` + `HintCubit` + `HintBadge`)
  and `lib/features/admin_hint/` (admin console — lookup +
  set/grant/reset, mirroring `admin_energy`).
- HintBadge wired into HomeScreen next to EnergyBadge.
- `/admin/hint` route + nav destination.
- `admin_quests` reshape: `QuestDefinition` DTO, repository,
  cubit signatures, and the form now collect **two** reward
  inputs (Enerji ⚡ + İpucu 💡) with a form-level
  at-least-one-positive rule.
- Player `quests_screen` renders both reward badges
  side-by-side when positive (energy primary, hint tertiary).
- 5 quest-area test files reshaped + new claim snackbar text
  ("inventories will update" — generic for both rewards).
- `flutter test` green at 103/103.

### Slice H7 — Tests + quality gate (2026-05-25, 1ee29a7)

- Hint domain tests
  (`Tests/PlayerHintInventories/`): Consume, GrantBonus,
  AdminSet, AdminReset + rule violations. 19 cases total
  (incl. existing Initialize tests from H1).
- Hint integration tests
  (`IntegrationTests/PlayerHintInventories/`):
  `PlayerHintInventoryLifecycleTests` (lazy init on
  PlayerRegistered, idempotent re-registration, Grant→Consume,
  empty-balance reject) + `HintAdminCommandTests` (non-admin
  reject, Set + Grant + Reset with audit row assertions). 8 cases.
- `Hint.IT TestBase` extended with `AdministrationStartup` and the
  Energy-style `TestAdminAuthorizationContext` so
  `IAdminCommand`-decorated commands can be exercised end-to-end.
- `Games.IT`: replaced `AlwaysAllowingHintGuard` with
  `RecordingHintGuard` (`CallCount` + `RejectNext` flag, resolved
  as singleton). New `UseHintFallThroughTests`: free quota
  satisfies first hint without invoking the gateway; second hint
  falls through; gateway rejection propagates without advancing
  the `HintsUsed` counter (the counter only tracks the per-game
  free quota by design).
- API.Tests: `GetQuestsMe_FreshPlayer_LazilyReturnsSeededDaily`
  switched to assert `energyReward` + `hintReward`.
- `scripts/test.sh`: Hint.Tests in the DB-free batch,
  Hint.IntegrationTests in the integration batch.

### Slice H8 — Manual verification + docs (2026-05-26, this commit)

- Operator restarted the stack, created an Energy+Hint reward
  quest via the admin console, completed three games as a test
  player, verified the claim grants both inventories (Energy +1
  visible on the energy badge after refresh; Hint +N visible on
  the new hint badge), exercised the Game UseHint flow (first
  hint consumes the free quota, second hint draws from the
  player inventory, exhausted inventory blocks the hint). All
  flows passed.
- Docs polished: this entry + `activeContext.md > Active Sprint`
  + `GLOSSARY.md` (Hint aggregate / events / rules / gateway /
  multi-reward QuestDefinition + PlayerQuest text) +
  `frontendActiveContext.md` + `frontendProgress.md > Slice H6`.

---

## Sprint Q1 — Quests Module redesign (2026-05-24)

Backend Q1.1–Q1.5 + Q1.7 and frontend Q1.6 shipped in a single
session. The closed-enum `QuestType` catalog is gone; quest
definitions now carry free-text `Name` + `Description`, a fixed
`QuestTrigger`, `Threshold`, `Reward`, optional
`PrerequisiteQuestDefinitionId`, and `ProgressBaseline`. PlayerQuests
are issued lazily on `GET /quests/me` and progress is computed at read
time from Stats counters. Final quality gate: **361/361 .NET tests +
103/103 Flutter tests green**.

Detailed slice plan + decisions live in
`ROADMAP.md > Sprint Q1 — Quests Module Redesign`. Frontend slice
detail in `frontendProgress.md > Slice Q1.6`.

### Slice Q1.1 — Domain reshape (2026-05-24, 9f8face)

- Drop `QuestType`, `QuestCadence`, all four progress-tracking rules
  and the `PlayerQuestCompleted` / `PlayerQuestAdminReset` events.
  `QuestState` shrinks to `Active|Claimed`.
- Add `QuestTrigger` (3 values), `ProgressBaseline` (2 values), six
  new rules (Threshold/Reward/Name length/Description length/Cycle).
- `QuestDefinition.Create` / `Update` take the new field set.
  `Update` excludes Name + Trigger (immutable post-create per Q1.5).
  Cycle check is parametric on a boolean computed by the handler.
- `PlayerQuest.IssueFor(playerId, questDefinitionId, baselineSnapshot,
  issuedAt, expiresAt?)`. `Claim(now, isReadyToClaim, reward)` —
  caller computes readiness from Stats counters; reward flows through
  to the domain event so Energy bonus delivery stays event-driven.

### Slice Q1.2 — DbUp schema rewrite (2026-05-24, a094219)

- Rewrite `quests.PlayerQuests` (QuestDefinitionId FK +
  ProgressBaselineSnapshot; no Progress/Goal/RewardAmount/CompletedAt)
  + `quests.QuestDefinitions` (Name/Description/Trigger/Threshold/
  Reward/PrerequisiteQuestDefinitionId/ProgressBaseline) + view
  `v_PlayerQuests`.
- `UX_PlayerQuests_PlayerId_QuestDefinitionId` for idempotent lazy
  issuance under concurrent calls.
- Seed shrinks to a single `'Günlük 3 Oyun'` daily quest
  (`11111111-0000-0000-0000-000000000010`); the Total chain is left to
  admin tooling so the chain-building UX is exercised end-to-end.
- `030_ReshapeQuestsForSprintQ1.sql` ships as an idempotent
  destructive migration so existing local databases drop the old
  shape and rebuild — no production data yet, so no preservation.

### Slice Q1.3 — Application reshape (2026-05-24, 5087205)

- Delete `GameCompleted` / `AuthProviderLinked` / `PlayerRegistered`
  integration event handlers and `RecordQuestProgressCommand`. All
  three were tied to hardcoded QuestType; lazy issuance makes them
  unnecessary. Admin `IssueQuestToPlayer` and `ResetPlayerQuest` also
  removed — chain-aware lazy issuance leaves no use case for either.
- New `IQuestCounterReader` contract in
  `Application/Configuration/CrossModule/`. Returns
  `QuestCounters(GamesCompletedTotal, GamesCompletedToday,
  AuthProviderLinked)` in a single call.
- `IssueQuestCommandHandler` reads counters via the gateway,
  computes baseline (FromSnapshot → counter, FromExistingTotal → 0
  for Total; 0 for Daily/Auth), and persists PlayerQuest with
  expiresAt = next UTC midnight for daily.
- `ClaimQuestCommandHandler` reads counters, computes
  `isReadyToClaim = (counter - baseline) >= threshold AND not past
  expiry`, passes that + `definition.Reward` into `Claim`. Claiming
  a deactivated definition's row is still allowed — deactivation
  hides future issuance but does not void earned rewards.
- `GetActiveQuestsQueryHandler` is now a two-pass Dapper handler:
  DELETE expired daily rows first (so the slot can be re-issued),
  then INSERT missing eligible PlayerQuests with `ON CONFLICT DO
  NOTHING`, then SELECT-with-join + in-memory projection of progress
  and DisplayState (`Active` / `ReadyToClaim` / `Claimed`).
- `QuestClaimedIntegrationEvent` now carries `QuestDefinitionId` +
  `Reward` (was `QuestType` + `RewardAmount`).
- `Energy.Application/QuestClaimedIntegrationEventHandler` reads the
  new `Reward` field — forced cross-module ripple from the contract
  reshape.

### Slice Q1.4 — Cross-module counter reader (2026-05-24, 318e1b3)

- `LexiLink.API/CrossModule/QuestCounterReader.cs` implements
  `IQuestCounterReader` against `stats.PlayerStats` (Total),
  `stats.PlayerPeriodStats` (Daily for today UTC), and
  `players.PlayerAuthIdentities` (AuthLinked existence) via Dapper.
- Fresh `NpgsqlConnection` per call (`await using`) so the adapter
  doesn't share lifetimes with any module's `SqlConnectionFactory`.
- Three small queries instead of a UNION ALL — readable; profile-
  driven optimization can collapse it later.
- Registered in `Program.cs` next to `EnergyGuard` /
  `AdminLookup` / `PlayerStatusLookup`.

### Slice Q1.5 — API endpoints reshape (2026-05-24, 6f0ecca)

- `AdminQuestEndpoints` request DTOs: `CreateQuestDefinitionRequest`
  (Name + Description + Trigger + Threshold + Reward +
  PrerequisiteQuestDefinitionId + ProgressBaseline) and
  `UpdateQuestDefinitionRequest` (same minus Name/Trigger per Q1.5
  immutability).
- Drop `POST /admin/quests/players/{playerId}/issue` and
  `POST /admin/quests/players/{playerId}/{playerQuestId}/reset` along
  with their request types.
- `GET /quests/me` external shape unchanged; internally now runs the
  Q1.3 two-pass handler.

### Slice Q1.7 — Tests reshape (2026-05-24, 85803d7)

- Delete `PlayerQuestExpireTests` and `PlayerQuestRecordProgressTests`
  (the underlying methods are gone). Reshape `PlayerQuestIssueTests`,
  `PlayerQuestClaimTests`, `QuestDefinitionTests`,
  `PlayerQuestTestsBase` to the new signatures.
- Quests.IT TestBase: seed-reset SQL leaves only the daily definition;
  `MutableQuestCounterReader` stub wired into the container so
  handlers can resolve `IQuestCounterReader` without booting Stats /
  Players. New `QuestAdminCommandTests` covers audit-target wiring
  for the new commands plus the direct self-reference cycle case.
- `QuestIntegrationEventTests` is rewritten end-to-end — no more
  Game/Auth event handlers, so the suite verifies lazy issuance,
  sync idempotency, prereq honoring, baseline-snapshot behaviour,
  delete-then-reissue daily flow, claim outbox roundtrip, and
  threshold-not-met claim rejection.
- `Energy.IT/QuestRewardDeliveryTests` + `API.Tests/QuestEndpointsTests`
  pick up the new `QuestClaimedIntegrationEvent` and PlayerQuests
  column shape.
- Final quality gate: **361/361 .NET tests green** (4 progress/expire
  tests + obsolete IT cases dropped; net -7 from previous 368).

### Bug fixed during Q1.7

- `GetActiveQuestsQueryHandler` originally ran `Sync` before `Delete
  expired`. An expired Daily row looked "existing" to the sync pass
  (no insert), then was deleted, leaving the player with zero rows
  for the day. Order swapped: Delete expired first, then Sync
  missing. Comment added explaining the why.

---

## Administration Module + admin frontend session (2026-05-21 to 2026-05-23)

The sixth backend module (Administration) and the six-slice admin
frontend (F1–F6) shipped together. Mid-session manual testing surfaced
follow-on backend fixes (B11/B12/B15, Npgsql timestamp behavior,
ProductionJwt dev preset). Frontend session details live in
`frontendProgress.md > Admin frontend sprint (F1–F6)`.

### Slice B1 — Administration module foundation (2026-05-21, commit 4164868)

- New module `LexiLink.Modules.Administration` with Domain /
  Application / Infrastructure / IntegrationEvents / Tests /
  IntegrationTests projects. Schema `administration`.
- `AdminUser` aggregate (`Id`, `Email` VO, `Role` VO with single
  `Admin` value, `CreatedAt`, `IsActive`, `DisabledAt?`).
  `RegisterAdminUserCommand` idempotent on email.
- Per-module CQRS contracts + outbox + decorator chain copied from
  Players module template (Kamil-faithful, no shared Common contract).
- DbUp scripts: `administration` schema, `AdminUsers`,
  `OutboxMessages`, `InboxMessages`, `AdminActionAudit`.

### Slice B2 — Admin registration + bootstrap seed + outbox publish (2026-05-21, 91312da)

- `AdminUserRegisteredIntegrationEvent` published from outbox.
- `AdministrationBootstrapHostedService` reads
  `Administration:Bootstrap:AdminEmails` list and issues
  `RegisterAdminUserCommand` per email on every API start. One DI
  scope per email to avoid EF OwnsOne shadow-FK conflicts when
  seeding multiple admins. Failures logged and skipped — API still
  starts; operator fixes config and restarts.

### Slice B3 — Admin authentication (2026-05-21, efadd26)

- `POST /auth/admin/token` endpoint. Body: `{email, externalToken}`.
- `IExternalAdminIdentityVerifier`: dev impl checks
  `externalToken == "dev:admin:{email}"`. Production impl deferred.
- `JwtTokenIssuer.IssueAdmin(adminUserId)` adds `role=Admin` +
  `admin_id` claim alongside the standard subject claim.
- `AuthConstants`: `AdminRoleValue`, `AdminUserIdClaimType`,
  `AuthenticatedAdminPolicy`.
- `LexiLinkBearerAuthenticationHandler` (both modes) checks the
  Administration module for a matching active admin and stamps the
  role claim onto the principal.

### Slice B4 — Admin authorization cross-cut (2026-05-21, 4e9385b)

- `IAdminAuthorizationContext` contract in `Common.Application.Admin`.
- API host adapter `AdminAuthorizationContext` reads the JWT claims
  and exposes `IsAdmin` / `AdminUserId` to consumer modules.
- `IExecutionContextAccessor` extended with `IsAdmin` /
  `AdminUserId`.
- `IAdminCommand` marker interface (`AuditTargetType`,
  `AuditTargetId`).
- `AdminAuthorizationException` for non-admin attempts on admin
  commands.

### Slice B5 — Admin audit projection (2026-05-21, 18baf2f)

- `AdminActionPerformedIntegrationEvent` (cross-module).
- `administration.AdminActionAudit` table + inbox projection handler.
- `GetAdminActionsQuery` with offset/limit + adminUserId/targetType/
  targetId filters. Default limit 50, max 200.
- `GET /admin/audit/` endpoint.

### Slice B6 — Quests catalog data-driven (2026-05-21, 007098d)

- Quests' `QuestDefinition` promoted from a record to an aggregate
  with `Create / Update / Deactivate / Reactivate` and per-action
  domain events.
- `quests.QuestDefinitions` table + DbUp seed (4 rows for the existing
  quest types).
- `QuestCatalog` infra delegates to `IQuestDefinitionRepository`.
- Existing hardcoded `QuestDefinitionEntry` constants deleted.
- Note: predecessor to the broader Q1 redesign — see
  `ROADMAP.md > Sprint Q1 — Quests Module Redesign` for the full
  data-driven shape, which also removes the `QuestType` enum and
  switches issuance to lazy / pull-based.

### Slice B7 — Quest admin operations + first per-module audit decorator (2026-05-21, ba09c55)

- `Application/Admin/QuestDefinitions/{Create, Update, Deactivate}` +
  `Application/Admin/PlayerQuests/{IssueQuestToPlayer, ResetPlayerQuest}`.
- `Application/Admin/QuestDefinitions/GetQuestDefinitions` query
  returning `QuestDefinitionDto`.
- Audit decorator template introduced **per-module** (not in Common):
  `AdminAuditingCommandHandlerDecorator<T>` in
  `Quests.Infrastructure/Configuration/Processing/`. Same chain is
  copied to Energy / Players / Games infrastructures in later slices
  rather than shared (per Kamil "decorator-per-module" pattern saved
  in operator memory).
- `QuestsAdminActionPerformedNotification` +
  `DomainNotificationsMap.Instance` wiring routes the audit through
  the existing outbox.

### Slice B8 — Energy admin operations (2026-05-21, 7ed6a60)

- `Application/Admin/{SetPlayerEnergy, GrantBonusEnergy, ResetPlayerEnergy}`.
- `PlayerEnergy.AdminSet` / `AdminReset` raise dedicated domain
  events; `GrantBonus` is the existing public domain method (over-max
  permitted).
- Energy module gets its own
  `AdminAuditingCommandHandlerDecorator` (4th per-module template
  copy by slice end).

### Slice B9 — Players admin ban/unban + auth boundary refusal (2026-05-21, 33c8cb4)

- `Player` aggregate: `Ban(reason)` / `Unban()` + ban state fields
  (`IsBanned`, `BannedReason`, `BannedAt`).
- DbUp `030_AddPlayerBanColumns.sql`.
- `Application/Admin/{BanPlayer, UnbanPlayer, GetPlayerAdminDetail}`.
- `IPlayerStatusLookup` cross-module contract; auth handler refuses
  banned tokens at the boundary (admin tokens are exempt — a banned
  player who is also an admin can still authenticate as admin).
- `GET /admin/players/{playerId}` + `POST /ban` + `POST /unban`.

### Slice B10 — Content admin guard (2026-05-21, 3bc4dde)

- Games' write commands (`Create/EditCategory`,
  `Create/Activate/Deactivate/AddOutgoingLink/RemoveOutgoingLink Link`)
  promoted to `IAdminCommand`. Player-facing read endpoints stay on
  `AuthenticatedPlayer` policy.
- New `/admin/content/...` endpoint group, behind `AuthenticatedAdmin`.
- Games module gets the per-module audit decorator (final template
  copy of B-series).
- Games.IT TestBase boots Administration and default-logs-in a
  synthetic admin so existing content-seeding tests keep passing.
- `ValidationProblemDetailsTests.CommandValidationFailure` retargeted
  from `/categories` to `/admin/content/categories`.
- **Backend sprint closed:** 368/368 tests pass.

### Mid-test follow-ons (2026-05-22…23)

These shipped after F1–F6 manual testing surfaced gaps. None changes
the closed B1–B10 contract; all are additive or environmental.

**Slice B11 — Admin energy GET endpoint** (commit 45f8ac0, also
covers frontend F5).

- `GET /admin/players/{playerId}/energy` returning
  `PlayerEnergySnapshotDto`. Reuses the existing
  `GetPlayerEnergyQuery` handler under `AuthenticatedAdmin`.
- Tiny passthrough; the query is auth-agnostic (no
  `IExecutionContextAccessor` dependency), so reuse is direct.
- Unblocks the F5 admin energy console UI.

**Slice B12 — Reactivate quest definition** (uncommitted at doc time;
will ship with Q1 commit prep).

- `Application/Admin/QuestDefinitions/ReactivateQuestDefinition/`
  command + handler mirrors Deactivate.
- `POST /admin/quests/definitions/{id}/reactivate` endpoint.
- Domain `QuestDefinition.Reactivate()` already existed; this slice
  exposes it via the Admin pipeline + audit decorator.

**Slice B15 — Quests listens to `PlayerRegisteredIntegrationEvent`**
(uncommitted; **will be deleted in Q1.3**).

- `PlayerRegisteredIntegrationEventHandler` issues all active
  QuestDefinitions to a newly registered player (idempotent,
  prereq-respecting).
- Worked locally for the new-player flow but does not address
  existing players when an admin creates a new definition. Lazy
  issuance (Q1) is the long-term answer.

**Npgsql timestamp behavior fix** (uncommitted).

- `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`
  in `Program.cs` (before `WebApplication.CreateBuilder`).
- Root cause: Npgsql 6+ converts `Kind=Utc` `DateTime` values into
  the session's local timezone when writing to `timestamp without
  time zone`. Reads round-trip the local-shifted value, and consumers
  (e.g. `EnergyRefillCalculator`) call `DateTime.SpecifyKind(..., Utc)`
  → treats the local-time value as UTC → 7-hour drift on the dev
  Mac. Energy projection then computed 24 refill intervals and
  always returned a fully-refilled bucket.
- Legacy switch writes UTC values verbatim into `timestamp` columns
  (the behavior the codebase assumes everywhere). One-liner; column
  type migration not required.

**ProductionJwt dev preset** (env-only, not committed code).

- Local dev now runs API with `Authentication__Mode=ProductionJwt` +
  a 32+ char dev signing key + matching issuer/audience. Reason:
  `DevelopmentBearer` only accepts raw GUIDs as bearer tokens, but
  the admin login flow returns a real JWT (Slice B3). Player flow
  updated correspondingly on the frontend
  (`GuestPlayerRepository` now calls `/auth/token` after
  `/players/guest`).
- Operator-level configuration only; no code change in this
  session. Production already required these env vars per
  `OPERATIONS.md`.

**Backend Player /quests/me filter for deactivated definitions**
(uncommitted).

- `GetActiveQuestsQueryHandler` SQL joins with `quests.QuestDefinitions`
  and filters `qd.IsActive = TRUE`. Deactivating a definition hides
  its rows from the player immediately; reactivating brings them
  back. Rows stay in the DB (claim history preserved).

**Backend QuestType.Custom1/2/3 placeholder slots** (uncommitted;
**will be deleted with the enum in Q1.1**).

- 3 placeholder values added to the enum so the admin Create flow
  can be exercised end-to-end against types that don't already have
  a definition. Behaviorally inert — no event handler triggers
  issuance / progress for these types.

---

## Game Options Selection — target reachability revision (2026-05-17)

İlk teslimat sırasında gözden kaçan bug: density-only seçimde target'a giden
tek outlink "izole" konumdaysa (cluster'a göre common-neighbor sayısı düşük)
sessizce atılıyordu ve oyuncu sıkışabiliyordu. Mevcut
`Select_SeedsByHighestPairwiseScore_WhenNoPrevious` unit testi zaten bu davranışı
"doğru" sayıyordu — testin kendisi bug'ın imzasıydı.

- **Selector** — `OutgoingLinkSelector.Select(..., pathToTargetLinkId, limit)`
  imzasına yeni lock parametresi eklendi. previousLinkId → pathToTargetLinkId
  → density seed → greedy fill sırasıyla doluyor. `previousLinkId ==
  pathToTargetLinkId` durumunda lock tek sefer alınır (geçersiz duplicate
  oluşmaz).
- **Handler** — `GetGameOptionsQueryHandler` Game projection'ına
  `TargetLinkId`/`CategoryId` ekledi. Adaylardan target'a giden ilk hop
  in-memory BFS ile çözülüyor (kategori-scoped adjacency pull + sorted
  neighbor lists for determinism). Edge case: `target == current` (oyun
  bitmiş ya da bitmek üzere) → lock yok; `target` doğrudan candidate ise
  kendisi lock; BFS yol bulamazsa null → eski davranış.
- **Tests** — +4 unit
  (`Select_LocksPathToTargetLinkId_EvenWhenIsolated`,
  `Select_LocksBothPreviousAndPathToTarget`,
  `Select_PreviousAndPathToTargetSameLink_LocksOnce`,
  `Select_PathToTargetIdNotInCandidates_IsIgnored`), +1 integration
  (`GetGameOptions_ReachabilityIsolatedLeaf_IsAlwaysIncluded`: cluster6 +
  izole leaf + target via izole; izole leaf her zaman dönen 6'nın içinde).
- **Quality gate** — `./scripts/test.sh`: 285/285 pass (önceki 280 + 5),
  0 warning.

### Verification

- `dotnet build LexiLink.sln` → 0 error, 0 warning.
- `./scripts/test.sh --no-restore -v minimal` → 11 test projects,
  **285 tests pass**.

---

## Game Options Selection ✅ closed (2026-05-17)

Frontend Slice 11 (Game Screen Polish) için backend pre-step. Oyun ekranı
artık her zaman tam 6 outgoing link göstersin; bir kelimenin 6'dan fazla
outlink'i varsa backend deterministik bir alt küme seçer ve previousLinkId
her zaman kilitli kalır.

- **Algoritma** — pairwise common-neighbor sum maksimize eden greedy
  densest-k-subgraph. Tie-break: score DESC → degree DESC → LinkId ASC.
  previousLinkId verildiğinde önce o tohumlanır; yoksa en yüksek pairwise
  skorlu aday seçilir. Tüm pairwise skorlar 0 ise degree DESC fallback'i
  devreye girer.
- **Application** —
  `LexiLink.Modules.Games.Application/Games/GetGameOptions/GetGameOptionsQuery.cs`
  (`QueryBase<List<OutgoingLinkDto>>`),
  `GetGameOptionsQueryHandler.cs` (Dapper; tek query'de current + start
  link + history count + history[count-2], ardından aday + degree
  fetch, son olarak yalnızca gerektiğinde pairwise matrix),
  `OutgoingLinkSelector.cs` (saf algoritma, internal static).
- **Previous link resolution** — `Game._history` start adımını tutmaz;
  handler history count'u sayar (PostgreSQL `CROSS JOIN LATERAL`):
  0 → previous yok (oyuncu start'ta), 1 → `Game.StartLinkId`,
  ≥2 → history DESC sıralı OFFSET 1 LIMIT 1. Bu ayrım
  `GetGameOptions_AfterStep_PreviousLinkIsAlwaysIncluded` integration
  testiyle kilitli (test ilk yazımda fail etti, asıl bug history'nin
  start adımını içermemesiydi).
- **API** — `GET /games/{id:guid}/options`,
  `AuthenticatedPlayer` policy.
  `GamesEndpoints` grubuna eklendi.
- **Edge cases** — `|candidates| ≤ 6` ise hepsi döner; previousLinkId
  aday setinde değilse yokmuş gibi davranılır; tüm pairwise skorlar 0
  ise degree fallback.
- **Tests** — 9 yeni unit (`OutgoingLinkSelectorTests`), 4 yeni
  integration (`GetGameOptionsIntegrationTests` star graph + Game tablosu
  direct insert + MakeStep adımı + determinism). Quality gate
  280/280 pass (önceki 267 + 13), 0 warning.

### Verification

- `dotnet build LexiLink.sln` → 0 error, 0 warning.
- `./scripts/test.sh --no-restore -v minimal` → 11 test projects,
  **280 tests pass**.

---

## Quests Module ✅ closed (2026-05-15)

LexiLink'in beşinci modülü (Games, Players, Stats, Energy sonrası) teslim
edildi. Quests, daily/play-driven quest sistemi ve event-driven reward
delivery getirir; aynı zamanda LexiLink'in **ilk reverse cross-module event
dependency**'sini canlandırır (Energy.Application,
`Quests.IntegrationEvents.QuestClaimedIntegrationEvent`'i tüketir).

### Slice 1 — Quests.Domain

- **Aggregate** — `PlayerQuest` (kimliği `PlayerQuestId` = kendi Guid'i),
  `_playerId`/`_questType`/`_progress`/`_goal`/`_rewardAmount`/`_state`
  /`_issuedAt`/`_completedAt`/`_claimedAt`/`_expiresAt` shadow field'ları,
  `IssueFor` factory.
- **Davranış** — `RecordProgress(delta, now)` `ExpireIfPast(now)` →
  `QuestMustBeActiveToProgressRule` → progress clamping (geç gelen
  integration event goal'ün üzerine çıkamaz), goal'a ulaştığında
  `Active → ReadyToClaim`. `Claim(now)` `ExpireIfPast(now)` →
  `QuestMustBeReadyToBeClaimedRule` → `ReadyToClaim → Claimed`.
  `ExpireIfPast(now)` lazy `Active`/`ReadyToClaim → Expired`.
- **Enums** — `QuestType` (FirstGameCompleted, ThreeGamesCompleted,
  AccountLinked, DailyThreeGames), `QuestState` (Active, ReadyToClaim,
  Claimed, Expired), `QuestCadence` (OneTime, Daily).
- **Catalog kontratı** — `QuestDefinition` record + `IQuestCatalog`.
- **Kurallar (5)** — `QuestGoalMustBePositiveRule`,
  `QuestRewardAmountMustBePositiveRule`,
  `QuestProgressDeltaMustBePositiveRule`,
  `QuestMustBeActiveToProgressRule`, `QuestMustBeReadyToBeClaimedRule`.
- **Event'ler (3)** — `PlayerQuestIssuedDomainEvent`,
  `PlayerQuestCompletedDomainEvent`, `PlayerQuestClaimedDomainEvent`.
- **Tests** — 23 unit test (issue/progress/claim/expire fixture'ları).

### Slice 2 — Quests.Application

- Per-module CQRS contracts (`ICommand`/`IQuery`/`CommandBase`/`QueryBase`)
  + `IQuestsModule` facade, Players ile birebir aynı pattern.
- `IssueQuestCommand` — idempotent: prereq sağlanmadıysa veya zaten claimed
  bir OneTime quest varsa veya zaten `Active`/`ReadyToClaim` örnek varsa
  no-op. `RecordQuestProgressCommand` — quest yoksa veya `Active` değilse
  no-op. `ClaimQuestCommand` — `(playerQuestId, playerId)` constructor
  sırası; başka oyuncunun questi `NotFoundException` (id leak yok).
  `GetActiveQuestsQuery` — Dapper read + Application'da
  `ProjectState(...)` ile lazy expiry uygulaması.
- `PlayerQuestDto` (record); FluentValidation per command.

### Slice 3 — Quests.Infrastructure

- `QuestsContext` (schema `quests`),
  `PlayerQuestEntityTypeConfiguration` (enum'lar `HasConversion<string>`),
  `OutboxMessageEntityTypeConfiguration` (PK adı
  `PK_Quests_OutboxMessages`), `PlayerQuestRepository`
  (`EF.Property<>` ile shadow erişim), `SqlConnectionFactory`,
  `QuestCatalog` (hardcoded 4 MVP definition).
- Module-owned `QuestsUnitOfWork`, `QuestsDomainEventsDispatcher`,
  `OutboxAccessor` (Energy birebir).
- 6 decorator (UoW × 2, Logging × 2, Validation × 2) Quests'in kendi
  `ICommandHandler<>` constraint'lerine bağlı.
- `QuestsAutofacModule`, `QuestsModule` facade impl, `OutboxModule`,
  `QuestsStartup`. `QuestCatalog` singleton kayıt.

### Slice 4 — DbUp scripts

- `quests/Schema/001_CreateSchema.sql`,
  `quests/Tables/010_PlayerQuests.sql` (PK `Id`, indexes
  `(PlayerId, State)` ve `(PlayerId, QuestType)`),
  `quests/Tables/070_OutboxMessages.sql` (PK `PK_Quests_OutboxMessages`
  + 2 retry index), `quests/Views/110_v_PlayerQuests.sql`.
- DbUp first-run: 4 script applied. Re-run: 0 pending. Idempotent
  doğrulandı.

### Slice 5 — Integration event handlers (Quests consumer side)

- `Quests.Application.csproj` → `Players.IntegrationEvents` +
  `Games.IntegrationEvents` granular ref'leri.
- `GameCompletedIntegrationEventHandler`: 3 quest type için
  `IssueQuestCommand` + `RecordQuestProgressCommand(delta=1)` dispatch
  (FirstGameCompleted, ThreeGamesCompleted, DailyThreeGames).
- `AuthProviderLinkedIntegrationEventHandler`: `IssueQuestCommand` +
  `RecordQuestProgressCommand(delta=1)` for AccountLinked; prereq
  enforcement IssueQuestCommandHandler içinde.
- `Quests.IntegrationTests` projesi + 4 test (sonra Slice 6'da +1
  outbox test eklendi → 5 toplam): single GameCompleted issues 3 quests;
  3× GameCompleted hepsini ReadyToClaim yapar; AuthProviderLinked prereq
  yokken AccountLinked issue olmaz; ThreeGamesCompleted claimed sonra
  AccountLinked issue olur.
- `scripts/test.sh` Quests projelerini içeriyor.

### Slice 6 — Energy reward delivery (reverse cross-module event dep)

- **Yeni assembly** `LexiLink.Modules.Quests.IntegrationEvents` +
  `QuestClaimedIntegrationEvent` (PlayerId, PlayerQuestId, QuestType,
  RewardAmount).
- **Quests outbox publisher chain** —
  `PlayerQuestClaimedDomainEventNotification` (Infrastructure) +
  publisher (`IEventsBus.PublishAsync(QuestClaimedIntegrationEvent)`);
  `QuestsStartup` static ctor'da `DomainNotificationsMap` kaydı.
- **Energy domain extension** — `PlayerEnergy.GrantBonus(amount, now)`
  (max kontrolü yok, refill timer'a dokunmaz),
  `BonusAmountMustBePositiveRule`. `Consume` davranış düzeltmesi:
  timer artık sadece "at/above max → below max" geçişinde set ediliyor
  (10/5 → 9/5 ve 6/5 → 5/5 timer'ı sıfırlamaz; 5/5 → 4/5 sıfırlar).
- **Energy.Application reward delivery** — `GrantEnergyCommand` +
  validator + handler; `QuestClaimedIntegrationEventHandler` defansif
  `EnsurePlayerEnergyExistsCommand` (race-safe) + `GrantEnergyCommand`.
- **Cross-module ref** — `Energy.Application.csproj` granular
  `Quests.IntegrationEvents` ref'i.
- **Energy.Tests** — +7 test (GrantBonus over-max, refill timer
  düzeltmesi edge case'leri, BonusAmountMustBePositiveRule).
- **Energy.IntegrationTests** — +2 test (QuestClaimed → bonus delivery
  mevcut aggregate üzerinden over-max push; QuestClaimed lazy aggregate
  init under race).
- **Quests.IntegrationTests** — +1 test (ClaimQuest outbox row üretir
  → ProcessOutbox sonrası ProcessedDate set).
- **ArchTests** — `QuestsIntegrationEventsAssembly` base'e eklendi;
  `Energy.Application` forbidden listesi `LexiLink.Modules.Quests` →
  granular `Quests.Domain/Application/Infrastructure` (IntegrationEvents
  serbest); `IntegrationEvents_Should_NotDependOnModuleInternals` artık
  3 IntegrationEvents assembly'sini tarıyor.

### Slice 7 — API endpoints

- `GET /quests/me` (`AuthenticatedPlayer` policy):
  `IExecutionContextAccessor.UserId` → `GetActiveQuestsQuery` →
  `IReadOnlyList<PlayerQuestDto>` (200 OK).
- `POST /quests/{id:guid}/claim` (`AuthenticatedPlayer` policy):
  `ClaimQuestCommand(id, userId)` → 204 NoContent; başka oyuncunun
  questi → 404 ProblemDetails (`NotFoundException` mapping).
- `Program.cs` Quests startup + composition + endpoint mapping +
  `CheckMappings` çağrısı eklendi. `LexiLink.API.csproj` Quests.Infrastructure
  ref'i.
- `LexiLink.API.Tests` +5 test: 401 without bearer (GET ve POST), 200
  list response, 204 + DB state Claimed, 404 başka oyuncunun questi.
- Canlı API smoke: `POST /players/guest` → `GET /quests/me` `[]`
  döndürdü; `POST /quests/{random-id}/claim` 404 ProblemDetails;
  unauthenticated 401.

### Slice 8 — Documentation

- `GLOSSARY.md`: `PlayerQuest` aggregate, `QuestType`/`QuestState`/
  `QuestCadence` enums, `QuestDefinition`, `IQuestCatalog`,
  `QuestClaimedIntegrationEvent`, 3 PlayerQuest event ve 5 PlayerQuest
  rule (toplam: 20 domain event, 24 business rule). `PlayerEnergy`
  girişine `GrantBonus` + Consume timer fix; `BonusAmountMustBePositiveRule`
  PlayerEnergy rules'a eklendi.
- `CLAUDE.md`: Quests modülü beşinci modül olarak tanıtıldı; project
  layout box'a eklendi; aggregate listesine `PlayerQuest` eklendi.
- `ROADMAP.md`: Quests Module heading "✅ closed 2026-05-15" yapıldı;
  delivery summary + verification eklendi.
- `activeContext.md`: 2026-05-15 güncelle; "Slice 1 active" satırı
  kaldırıldı; mimari kazanım listesi (reverse cross-module event dep,
  GrantBonus, Consume timer fix, hardcoded catalog, API endpoints);
  Active Constraints'e Quests-specific constraint'ler eklendi
  (event-driven reward, raw inbox YOK, GrantBonus over-max);
  Next Action backlog'a çevrildi (Game Content/Admin Tooling, frontend
  MVP devamı, Apple/Google verifier).
- `kamil-modular-monolith-comparison.md`: reverse cross-module event
  dep notu.
- `OPERATIONS.md`: Quests şeması migration listesinde.
- `progress.md`: bu giriş.

### Verification

- `dotnet build LexiLink.sln` → 0 error, 0 warning.
- Full `scripts/test.sh`: **267/267** test (11 proje: API 29, Games 85,
  Players 27, Energy 23, Quests 23, ArchTests 38, Games.IT 18, Players.IT 7,
  Stats.IT 8, Energy.IT 4, Quests.IT 5).
- Canlı API smoke (DevelopmentBearer): register guest → `GET /quests/me`
  `[]`; bearer-less calls 401; non-existent quest claim 404.

---

## Energy Module ✅ closed (2026-05-14)

LexiLink'in dördüncü modülü (Games, Players, Stats sonrası) teslim edildi.
Energy modülü, oyuncunun oyun başlatabilmesini enerji bütçesine bağlayan ilk
synchronous cross-module dependency'yi getirir.

### Slice 1 — Energy.Domain (2026-05-14)

- **Aggregate** — `PlayerEnergy` (kimliği `PlayerEnergyId` = `Players.PlayerId`
  Guid değeri), `_currentAmount` / `_maximumAmount` /
  `_rechargeIntervalSeconds` / `_lastRefilledOn` backing field'ları, full
  energy ile başlayan `InitializeFor` factory.
- **Davranış** — `Consume(amount, now)` önce `RechargeBasedOnElapsedTime(now)`
  çağırır; from-max tüketim `_lastRefilledOn = now` ile recharge timer'ı yeniden
  başlatır.
- **Kurallar (4)** — `EnergyConfigurationMustBeValidRule`,
  `EnergyAmountCannotBeNegativeRule`, `EnergyAmountCannotExceedMaximumRule`,
  `EnergyMustBeSufficientToConsumeRule`.
- **Event'ler (2)** — `PlayerEnergyConsumedDomainEvent`,
  `PlayerEnergyRefilledDomainEvent`.
- **Pure-math projection** — `EnergyRefillCalculator.Project(...)` aggregate
  ve read query tarafından paylaşılan tek matematik kaynağı; kısmi interval'ı
  korur, max'ta cap eder.
- **Tests** — 16 unit test (initialize/consume/recharge fixture'ları).

### Slice 2 — Energy.Application

- Per-module CQRS contracts (`ICommand`/`IQuery`/`CommandBase`/`QueryBase`)
  + `IEnergyModule` facade interface; Players ile birebir aynı pattern.
- `EnsurePlayerEnergyExistsCommand` (idempotent init), `ConsumePlayerEnergyCommand`
  (rule failure → `BusinessRuleValidationException`), `GetPlayerEnergyQuery`
  (Dapper read + Application'da lazy refill projeksiyonu).
- `PlayerEnergySnapshotDto`: `currentAmount`, `isFull`, `secondsUntilNextRefill`,
  `fullyRefilledAt`. FluentValidation `AbstractValidator<T>` per command.

### Slice 3 — Energy.Infrastructure

- `EnergyContext`, `PlayerEnergyEntityTypeConfiguration`,
  `OutboxMessageEntityTypeConfiguration`, `PlayerEnergyRepository` (EF-only),
  `EnergyConfigurationService` (`Energy:MaxAmount` /
  `Energy:RechargeIntervalSeconds` / `Energy:GameStartCost` from
  `IConfiguration`; defaults `5 / 900 / 1`), `SqlConnectionFactory`.
- Module-owned `EnergyUnitOfWork`, `EnergyDomainEventsDispatcher`,
  `OutboxAccessor` (Players birebir).
- 6 decorator (UoW × 2, Logging × 2, Validation × 2) Energy'nin kendi
  `ICommandHandler<>` constraint'lerine bağlı.
- `EnergyAutofacModule`, `EnergyModule` facade impl, `OutboxModule`,
  `EnergyStartup`. Domain notification wrapper'ları henüz eklenmedi (consumer
  yok).

### Slice 4 — DbUp scripts

- `energy/Schema/001_CreateSchema.sql`,
  `energy/Tables/010_PlayerEnergies.sql` (PK `PlayerId`),
  `energy/Tables/070_OutboxMessages.sql` (PK adı `PK_Energy_OutboxMessages`),
  `energy/Views/110_v_PlayerEnergies.sql`.
- DbUp first-run: 4 script applied. Re-run: 0 pending. Tablo/view doğrulandı.

### Slice 5 — Games cross-module wiring

- `IEnergyGuard` interface `Modules/Games/Application/Configuration/CrossModule/`
  altında; Games Energy contract'larına bağımlı değil.
- Adapter `LexiLink.API/CrossModule/EnergyGuard.cs`: `IEnergyModule` +
  `IEnergyConfigurationService` üzerinden `ConsumePlayerEnergyCommand`.
- `StartGameCommandHandler` `_energyGuard.EnsureCanStartGameAsync(game.PlayerId)`
  → `game.Start()`. Insufficient case state'i `Initial`'dan ilerletmez. Residual
  dual-write riski yorumla belgelendi.
- `Program.cs`'e Energy startup + adapter registration.
- Games/Stats integration test base'lerinde `AlwaysAllowingEnergyGuard` stub.
- ArchTest'lere 3 yeni Energy layer rule + diğer modüllere Energy forbidden
  namespace.

### Slice 6 — PlayerRegistered handler

- Energy.Application içinde
  `IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>` →
  `EnsurePlayerEnergyExistsCommand`.
- `EnergyAutofacModule` `IIntegrationEventHandler<>` scan.
- ArchTest'lerde Energy.Application/Infrastructure forbidden listesi
  Players.{Domain,Application,Infrastructure} olarak granüler;
  `Players.IntegrationEvents` ref'i serbest.
- `Microsoft.Extensions.Configuration 10.0.4` CPM'e eklendi.
- `Energy.IntegrationTests` projesi: 2 test (outbox sonrası full energy ile
  init, aynı device id idempotency). Real Postgres + Players + Energy + outbox
  processor.
- `scripts/test.sh` Energy projelerini içeriyor.

### Slice 7 — API endpoint

- `GET /energy/me` (`AuthenticatedPlayer` policy):
  `IExecutionContextAccessor.UserId` → `GetPlayerEnergyQuery` →
  `PlayerEnergySnapshotDto`.
- API.Tests: 401 without bearer, 404 missing aggregate, 200 full snapshot.
- Canlı API smoke: `POST /players/guest` → ~10s sonra outbox processed →
  `GET /energy/me` `currentAmount:5, isFull:true` döndürdü.

### Slice 8 — Documentation

- `GLOSSARY.md`, `CLAUDE.md`, `kamil-modular-monolith-comparison.md`,
  `ROADMAP.md`, `activeContext.md`, `progress.md`, `OPERATIONS.md`
  güncellendi.

### Verification

- `dotnet build LexiLink.sln` → 0 error.
- Full `scripts/test.sh`: **222/222** test (9 proje: API 24, Games 85, Players
  27, Energy 16, ArchTests 35, Games.IT 18, Players.IT 7, Stats.IT 8, Energy.IT 2).
- Canlı API: `POST /players/guest` → `GET /energy/me` end-to-end başarılı.

---

## Frontend Backend Guest Smoke (2026-05-13)

- **Real guest smoke** — Ran the DbUp migrator against local PostgreSQL; 0
  pending scripts, upgrade succeeded.
- **API dev startup** — Started `LexiLink.API` in Development mode on
  `http://127.0.0.1:5099` with `Authentication__Mode=DevelopmentBearer`.
- **Guest endpoint** — `POST /players/guest` returned a real player id from the
  local API.
- **DevelopmentBearer check** — `GET /players/{id}` with `Authorization:
  Bearer <playerId>` returned the registered guest details.
- **CORS fix** — Added a configured `LexiLinkFrontend` CORS policy and
  Development allowed origins for `http://127.0.0.1:5173` and
  `http://localhost:5173`, so Flutter web preview can call the API from the
  browser. Production remains closed unless origins are configured.
- **Verification** — `dotnet build src/API/LexiLink.API/LexiLink.API.csproj
  --no-restore --disable-build-servers -v minimal` passed with 0 warnings and
  0 errors.

### Guest retry/idempotency fix

- **Issue** — Repeating `POST /players/guest` with the same frontend device id
  returned HTTP 500 because the backend attempted to insert a duplicate guest
  auth identity.
- **Fix** — `RegisterGuestPlayerCommandHandler` now returns the existing guest
  player id when `AuthProvider.Guest + deviceId` already exists.
- **Coverage** — Added Players integration test coverage for same-device guest
  registration idempotency.
- **Smoke** — Two live API calls with `frontend-preview-device` both returned
  the same player id.

### Spor category content import

- **Source file** — Imported `docs/category-spor.json`, containing the Spor
  category, 157 links, and 1234 directed link interactions.
- **Tooling** — Added `LexiLink.Tools.CategoryImporter`, a deterministic,
  repeatable PostgreSQL importer for category JSON files.
- **Local DB result** — Imported Spor as category id
  `f29ec5db-774d-eb3b-9974-6fbecfbecf6d`.
- **API verification** — `/categories` returns Spor, `/categories/{id}` returns
  `linkCount: 157`, and `/links?categoryId=...` returns 157 links.

## Production Readiness Pass ✅ baseline closed (2026-05-12 to 2026-05-13)

Kamil alignment tamamlandıktan sonra yeni sıra production-facing risklere geçti:
auth/identity, Stats product metrics, API contract hardening, operational
readiness ve database hygiene.

### Production readiness closure audit (2026-05-13)

- **Closed baseline** — Slices 16-21 are complete: production auth/JWT baseline,
  Stats daily/weekly product metric, API contract hardening, operational
  readiness, database hygiene, and release smoke gate.
- **Deferred** — Real Apple/Google external token verifiers remain blocked on
  provider credential/client configuration. Warnings-as-errors/analyzer policy
  remains deferred until known warnings are cleaned up or intentionally
  suppressed.
- **Non-actions** — Full schema diff tooling, broad role/permission matrix, and
  UserAccess-style module are intentionally out of scope until concrete product
  needs appear.
- **Next recommendation** — Move to Game Content/Admin Tooling: repeatable
  category/link dataset validation, import, and seed workflow.

### Slice 21a — Release smoke script (2026-05-13)

- **Smoke gate** — Added `scripts/smoke.sh` to build the API, apply DbUp
  migrations, start the API in `Production` mode with `ProductionJwt`, and
  check `/health/live` plus `/health/ready` over HTTP.
- **Config overrides** — The script uses the local PostgreSQL connection string
  by default and supports `ConnectionStrings__LexiLinkDb` and
  `LEXILINK_SMOKE_PORT` overrides.
- **Operations doc** — Documented the smoke command in `docs/OPERATIONS.md`.
- **Verification** — `./scripts/smoke.sh` passed locally: API build succeeded,
  DbUp reported 0 pending scripts, and `/health/live` plus `/health/ready`
  returned healthy over HTTP.

### Slice 20c — Lightweight migration drift validation (2026-05-13)

- **Decision** — Chose a lightweight DbUp journal guard instead of a full schema
  diff. For LexiLink's current size, the highest-value drift risk is deploying
  code without applying its SQL scripts.
- **API artifact manifest** — The API project now carries
  `src/Database/LexiLink.Database/Structure/**/*.sql` into its build output
  under `Database/Structure`.
- **Readiness validation** — `/health/ready` now includes
  `database-migrations`, which compares expected artifact scripts with
  `public.MigrationsJournal` and reports unhealthy when scripts are missing.
- **Diagnostics** — Health check JSON includes health-check `data`, so missing
  migration count and a capped missing-script sample are visible.
- **Verification** — API tests passed: `dotnet test
  src/API/LexiLink.API.Tests/LexiLink.API.Tests.csproj --no-restore
  --disable-build-servers -v minimal` -> 21/21. Architecture tests passed:
  `dotnet test src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj
  --no-restore --disable-build-servers -v minimal` -> 32/32.
- **Carry-over** — Full schema diff remains intentionally out of scope unless
  drift becomes a recurring production issue.

### Slice 20b — DbUp migration runbook (2026-05-13)

- **Fresh database** — Documented the first-run path: provision PostgreSQL, set
  `ConnectionStrings__LexiLinkDb`, run the migrator, verify
  `public.MigrationsJournal`, then check readiness.
- **Existing database** — Documented the release path: take backup/snapshot,
  verify artifact/script version, run pending DbUp scripts, then check health
  and processor visibility.
- **Recovery policy** — Documented failed migration handling, journal checks,
  restore/manual cleanup/forward-only corrective script choices, and the rule
  that previously journaled scripts must not be edited.
- **Rollback stance** — Clarified that DbUp migrations are forward-only; API
  rollback is only safe when the previous API version remains schema-compatible.
- **Verification** — Re-ran the migrator against local PostgreSQL; it reported
  0 pending scripts and completed successfully.
- **Carry-over** — Remaining Database Hygiene item is deciding whether schema
  drift needs lightweight validation.

### Slice 20a — Critical query/index review (2026-05-13)

- **Players auth lookup** — Reviewed `GetByAuthProviderAsync`; existing unique
  `(Provider, ExternalId)` index on `players.PlayerAuthIdentities` already
  supports the lookup.
- **Games traversal** — Reviewed category link selection, outgoing-link
  traversal, and completed-pair filtering. Existing completed-pair index remains
  valid; added `IX_Links_CategoryId_IsActive_Id` for active category link
  selection during puzzle creation.
- **Stats leaderboards** — Added all-time and period leaderboard indexes for
  `BestScore`, `TotalScore`, and `GamesCompleted` ordering paths.
- **Verification** — DbUp applied 3 scripts locally; Games integration tests
  18/18; Stats integration tests 8/8; Architecture tests 32/32.
- **Carry-over** — Next Database Hygiene step is the DbUp migration runbook.

### Slice 19d — Configuration/env operations documentation (2026-05-13)

- **Operations doc** — Added `docs/OPERATIONS.md` as the runtime configuration
  and operational runbook.
- **Required production config** — Documented `ConnectionStrings__LexiLinkDb`,
  `Authentication__Mode=ProductionJwt`, required JWT issuer/audience/signing
  key, token lifetime, and token-exchange mode.
- **Development guardrails** — Documented that `DevelopmentBearer` and
  `DevelopmentExternalToken` are local/test conveniences and are blocked in
  `Production`.
- **Operational surface** — Documented health endpoints, processor visibility,
  structured background log fields, processor defaults, and DbUp migration
  execution.
- **Result** — Slice 19 Operational Readiness baseline is complete. Next
  production-readiness slice is Database Hygiene.

### Slice 19c — Processor job failure logging/correlation (2026-05-13)

- **Background correlation** — Quartz jobs now create a fresh `CorrelationId`
  per execution, so request-independent background failures can be tied together
  in logs.
- **Structured job metadata** — Outbox and Stats inbox/internal-command jobs log
  start, completion, and failure with `BackgroundJob`, `ProcessorQueue`,
  `ProcessorType`, `QuartzFireInstanceId`, and `QuartzTrigger` metadata.
- **Processor scope** — Outbox, Stats inbox, and Stats internal-command
  processors also carry their own `ProcessorQueue`/`ProcessorType` scopes, so
  manual or test-triggered processor runs keep the same searchable fields.
- **Verification** — API tests 17/17; Architecture tests 32/32.
- **Carry-over** — Remaining Operational Readiness work is documenting
  configuration defaults and required env vars.

### Slice 19b — Async processor backlog/error visibility (2026-05-13)

- **Operations endpoint** — Added protected `GET /operations/processors` for
  operational visibility into `games-outbox`, `players-outbox`, `stats-inbox`,
  and `stats-internal-commands`.
- **Backlog summary** — The endpoint reports total unprocessed, ready to
  process, scheduled retry, poisoned, failed counts, max retry count, and oldest
  unprocessed timestamp per queue.
- **Error sample** — Each queue includes a capped error sample with message id,
  type, retry count, next retry date, and a trimmed error string.
- **Verification** — API tests 17/17; Architecture tests 32/32. New API smoke
  coverage locks auth protection and response shape.
- **Carry-over** — Remaining Operational Readiness work continues with
  processor job failure logging/correlation review.

### Slice 19a — Health checks baseline (2026-05-13)

- **Liveness/readiness** — Added anonymous `/health/live` and `/health/ready`
  endpoints using ASP.NET Core health checks.
- **Database connectivity** — Readiness now includes a PostgreSQL `SELECT 1`
  health check against `ConnectionStrings:LexiLinkDb`.
- **JSON report** — Health endpoints return a compact JSON payload with overall
  status and individual check status.
- **Verification** — API tests 15/15; Architecture tests 32/32. New API smoke
  coverage locks live health as anonymous/healthy and ready health as
  PostgreSQL-backed.
- **Carry-over** — Remaining Operational Readiness work starts with async
  processor backlog/error visibility for outbox, inbox, and internal commands.

### Slice 18c — OpenAPI auth/error contract visibility (2026-05-12)

- **Bearer scheme** — Added OpenAPI document/operation transformers. The
  generated document now exposes `LexiLinkBearer` as an HTTP bearer scheme and
  applies it to protected operations.
- **Anonymous exception** — Operations marked with `AllowAnonymous`, such as
  guest registration, are not marked as secured in OpenAPI.
- **ProblemDetails metadata** — Protected endpoint groups now advertise
  ProblemDetails responses for common 400/401/404 paths. Auth token exchange
  advertises 401/404 ProblemDetails.
- **Scalar impact** — Scalar reads the same `/openapi/v1.json`, so auth and
  error response visibility are available in the interactive docs without a
  separate Scalar-specific configuration.
- **Verification** — API tests 13/13; Architecture tests 32/32. New API smoke
  coverage checks `/openapi/v1.json` for bearer security, secured protected
  operations, unsecured anonymous operations, and ProblemDetails response
  metadata.
- **Result** — Slice 18 API Contract Hardening baseline is complete. Next
  production-readiness slice is Operational Readiness.

### Slice 18b — Endpoint error ProblemDetails consistency (2026-05-12)

- **Endpoint NotFound helper** — Added `ApiProblemResults.NotFound(...)` and
  replaced explicit empty `Results.NotFound()` responses in Auth, Players, and
  Stats endpoints with `application/problem+json` payloads.
- **Business-rule coverage** — Added API smoke coverage for a domain
  `BusinessRuleValidationException` path. It now locks 400 ProblemDetails with
  `rule`, `detail`, and `traceId`.
- **Conflict decision** — No separate 409 mapping was added yet. Current
  business-rule failures stay 400 until a specific rule has a stable conflict
  contract and client-facing recovery semantics.
- **Verification** — API tests 12/12; Architecture tests 32/32; Players
  integration tests 6/6.
- **Carry-over** — Remaining Slice 18 work is OpenAPI/Scalar review for auth and
  error response visibility.

### Slice 18a — Validation ProblemDetails baseline (2026-05-12)

- **Problem shape** — API exception middleware now emits
  `application/problem+json` for handled failures. Command validation failures
  use `HttpValidationProblemDetails` with RFC7807 fields, `traceId`, and an
  `errors` dictionary.
- **Validation detail** — Games/Players command validation decorators now pass
  validation failures grouped by property name instead of a flat message list.
  `InvalidCommandException` keeps the old flat list for compatibility and adds
  `ErrorsByProperty` for API contract output.
- **Other handled exceptions** — Business-rule, not-found, and unhandled
  exception paths now also write ProblemDetails-shaped payloads through the same
  middleware helper.
- **API coverage** — Added an API smoke test proving a protected command
  validation failure returns 400 `application/problem+json` with `type`,
  `title`, `status`, `detail`, `traceId`, and field-level `errors`.
- **Verification** — API tests 10/10; Architecture tests 32/32.
- **Carry-over** — Next API hardening sub-step is to standardize explicit
  endpoint-level `NotFound`/domain conflict-style responses and document the
  OpenAPI/Scalar error contract.

### Slice 17a — Daily/weekly leaderboard depth (2026-05-12)

- **Feature choice** — Stats Feature Depth için daily/weekly leaderboard seçildi;
  oyun deneyimi açısından per-category stats'ten daha görünür rekabet/motivasyon
  sağladığı için ilk product-facing Stats metriği oldu.
- **Read model** — Added `stats.PlayerPeriodStats`, keyed by `PeriodType`,
  `PeriodStartDate`, and `PlayerId`. It stores period `GamesCompleted`,
  `BestScore`, `TotalScore`, and `LastGameCompletedOn`.
- **Projection** — `GameCompletedIntegrationEvent` projection now updates both
  existing all-time `PlayerStats` and daily/weekly period aggregates. No event
  payload change was needed because `OccurredOn`, `PlayerId`, and `Score` were
  already present.
- **Query/API contract** — Added `LeaderboardPeriod` and extended
  `GetLeaderboardQuery` plus `GET /stats/leaderboard` with
  `period=allTime|daily|weekly` and optional `periodStart`. Existing
  `orderBy`/`limit` behavior remains intact.
- **Scope decision** — Stats remains a projection/read-model module; no Domain
  layer was added because the feature has no Stats-owned invariant yet.
- **Verification** — DbUp applied
  `stats.Tables.050_PlayerPeriodStats.sql` and
  `stats.Tables.051_PlayerPeriodStats_PeriodStartDate_Timestamp.sql`; Stats
  integration tests 8/8; API tests 9/9; Architecture tests 32/32.

### Slice 16d — Command-level authenticated execution coverage (2026-05-12)

- **Command context coverage** — Players integration tests now execute a real
  command through the module command decorator pipeline and assert the
  `IExecutionContextAccessor.CorrelationId` reaches command-level structured
  logs.
- **Logging context hardening** — Games and Players command decorators now emit
  request correlation as a dedicated `CorrelationId` property instead of
  competing with command context on the generic `Context` property.
- **Test infrastructure** — Players integration tests use a small collecting
  Serilog sink with `FromLogContext()` so decorator-enriched events can be
  asserted without external log infrastructure.
- **Verification** — Players integration tests 6/6; Games unit tests 85/85.
- **Result** — Slice 16 production-auth baseline is complete. Real Apple/Google
  verifier implementation remains deferred until provider credentials and
  client token contract are defined.

### Slice 16c — JWT issuing boundary and guest-to-auth coverage (2026-05-12)

- **Token issuer** — Added `IJwtTokenIssuer` and `JwtTokenIssuer`. Issued tokens
  use configured issuer/audience/signing key and access-token lifetime.
- **Token exchange endpoint** — Added `POST /auth/token`. It issues a
  first-party JWT only after `IExternalIdentityVerifier` verifies the submitted
  provider identity and Players resolves the linked auth identity.
- **Verifier boundary** — Added `IExternalIdentityVerifier`. Current
  `DevelopmentExternalToken` verifier is deterministic and non-production only;
  production startup rejects it. Disabled verifier is the default.
- **Auth surface** — Moved `GET /auth/me` into Auth endpoints and kept it
  protected for resource-token checks.
- **Guest-to-auth coverage** — Players integration tests now explicitly verify a
  guest player becomes non-guest after linking a social identity and remains
  resolvable by that provider identity.
- **Package governance** — Added direct IdentityModel package references for
  JWT creation/validation instead of relying on transitive assets.
- **Verification** — API tests 9/9; Players integration tests 5/5.
- **Carry-over** — Real Apple/Google token verification is not implemented yet;
  add provider-specific verifiers when client/provider credentials are defined.
  Next immediate auth hardening is command-level authenticated execution tests.

### Slice 16b — Production JWT validation (2026-05-12)

- **Strategy** — First production auth strategy is first-party signed JWT
  validation. Apple/Google token validation remains part of a future token
  exchange/login boundary; resource endpoints now validate LexiLink-issued JWTs.
- **Auth mode** — Added `ProductionJwt` alongside `DevelopmentBearer`.
- **Validation** — `ProductionJwt` validates issuer, audience, lifetime,
  required expiration, HMAC signature, and GUID `sub`. The authenticated
  principal still exposes `sub` and `NameIdentifier` for
  `IExecutionContextAccessor`.
- **Protected identity endpoint** — Added `GET /auth/me` to verify the current
  authenticated player without hitting module databases.
- **Tests** — API tests cover valid development bearer, production guard,
  valid production JWT, and wrong-signature JWT rejection.
- **Verification** — API tests 7/7.
- **Carry-over** — Token issuing/token exchange is not implemented yet. Next
  sub-step is to issue first-party JWTs after a verified login/link flow and
  lock guest-to-auth transition with integration tests.

### Slice 16a — Development bearer production guard (2026-05-12)

- **Problem** — `LexiLinkBearer` baseline'ı `Authorization: Bearer <player-guid>`
  kabul ediyordu. Bu test/development için yeterli, fakat production'da gerçek
  token doğrulama yerine geçmemeli.
- **Config contract** — Added `Authentication:Mode` with current value
  `DevelopmentBearer`.
- **Production guard** — API startup now fails in `Production` when
  `Authentication:Mode=DevelopmentBearer`.
- **Tests** — API auth tests now cover the production startup guard in addition
  to anonymous root access and protected endpoint 401 behavior.
- **Verification** — API tests 4/4.
- **Carry-over** — Real external token validation is not implemented yet. Next
  sub-step is choosing Apple/Google/JWT strategy and wiring real validation,
  not a fake parser.

### Planning reset — production readiness roadmap (2026-05-12)

- **Decision** — Kamil alignment artık aktif backlog değil; bilinçli sapmalar ve
  ihtiyaç oluşunca açılacak mimari işler dokümanlarda korunuyor.
- **New order** — `ROADMAP.md` üstüne Production Readiness Backlog eklendi:
  Production Auth / Identity, Stats Feature Depth, API Contract Hardening,
  Operational Readiness, Database Hygiene.
- **Active context** — `activeContext.md` artık sıradaki implementation slice'ı
  Production Auth / Identity olarak gösteriyor.
- **No production code changes** — Bu güncelleme planlama/dokümantasyon işidir.

## Architecture Alignment Pass ✅ closed (2026-05-11 to 2026-05-12)

Kamil Grzybek reference comparison sonrası eksikler sırayla kapatılıyor. Her adımda önce "Kamil bunu böyle mi yapıyor?" kontrol edilecek; bilinçli sapmalar ayrıca notlanacak.

### Slice 15 — Time abstraction baseline (2026-05-12)

- **Kamil reference check** — Kamil time-sensitive domain policy'lerde clock abstraction kullanıyor. LexiLink'te domain-visible zaman kararı olarak Players registration/link timestamp'leri; processing metadata olarak outbox, inbox ve internal-command retry/processed timestamp'leri vardı.
- **Clock contracts** — Added Common Application `IClock` and Common Infrastructure `SystemClock`.
- **Domain-visible decisions** — Players `RegisterGuestPlayerCommandHandler` and `LinkAuthProviderCommandHandler` now use `IClock.UtcNow` instead of direct `DateTime.UtcNow` when passing `registeredAt`/`linkedAt` into the domain model.
- **Processing metadata** — Outbox processor, Stats inbox processor, and Stats internal-command scheduler/processor now use `IClock` for eligibility, processed date, enqueue date, due date, and retry date calculations.
- **Tests** — Added Players command handler tests with a fixed clock to prove registration/link timestamps are controlled by the abstraction.
- **Conscious scope** — `DomainEvent` occurrence metadata still uses direct `DateTime.UtcNow`; injecting services into domain event constructors would add more coupling than value here. Integration tests still create event timestamps directly.
- **Verification** — Players unit tests 27/27; Architecture tests 32/32; API tests 3/3; Stats integration tests 7/7.
- **Result** — Slice 15 complete. Planned Kamil alignment backlog is now closed except for deliberate non-actions and need-triggered future items.

### Slice 14 — Module composition isolation review (2026-05-12)

- **Kamil reference check** — Kamil her module için ayrı composition root/container kullanıyor. LexiLink shared host container kullanıyor; önceki slice'larda gerçek çakışmalar module-owned UoW/domain dispatcher/outbox accessor ile kapatılmıştı.
- **Decision** — Full per-module container rewrite yapılmadı. Bu noktada yeni bir runtime registration collision yok; büyük rewrite gerçek problemi azaltmıyor, sadece karmaşıklık ekliyor.
- **Scope hardening** — `IEventsBus` registration singleton'dan scoped/lifetime-scope'a alındı. Böylece in-process event bus, integration-event handler'larını root provider yerine mevcut execution scope üzerinden çözer.
- **Architecture guard** — Added `CompositionIsolationTests`. Testler shared container'da common `DbContext`, `IUnitOfWork`, `IDomainEventsDispatcher`, `IOutbox` servislerinin global resolve edilemediğini, outbox processor'ların çoğul kaldığını ve `IEventsBus`'ın scope içinde aynı ama farklı scope'larda farklı instance olduğunu doğrular.
- **Verification** — Architecture tests 32/32; API tests 3/3; Stats integration tests 7/7.
- **Result** — Slice 14 complete. Shared host container korunuyor; per-module container/per-execution module scope ancak yeni somut collision oluşursa açılacak. Next alignment slice is Time Abstraction.

### Slice 13 — Event bus abstraction baseline (2026-05-12)

- **Kamil reference check** — Kamil integration events'i `IEventsBus` abstraction'ı üzerinden publish/subscribe ediyor; LexiLink public integration event contract'ları doğrudan MediatR `INotification`'a bağlıydı.
- **Contracts** — `IIntegrationEvent` artık MediatR'dan bağımsız. `IEventsBus` ve `IIntegrationEventHandler<T>` Common Application'a eklendi.
- **In-process implementation** — Added Common Infrastructure `InMemoryEventsBus`; current implementation resolves all `IIntegrationEventHandler<T>` handlers from DI and invokes them in-process. External broker bilinçli olarak eklenmedi.
- **Producer path** — Games/Players domain notification handlers now publish public integration events through `IEventsBus.PublishAsync(...)` instead of `IPublisher.Publish(...)`.
- **Consumer path** — Stats integration-event handlers now implement `IIntegrationEventHandler<T>` and are registered in `StatsAutofacModule` through that contract. Stats direct publish tests now use `IEventsBus`.
- **Architecture guard** — `IntegrationEvents_Should_NotDependOnModuleInternals` now also forbids MediatR dependency for public IntegrationEvents assemblies.
- **Verification** — `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0/0; Stats integration tests 7/7; API tests 3/3; Architecture tests 30/30.
- **Result** — Slice 13 is complete with an in-process bus baseline. Next alignment slice is Module Composition Isolation.

### Slice 12 — Stats internal commands baseline (2026-05-12)

- **Kamil reference check** — Kamil delayed/retried side effects için module-owned `InternalCommands`, command scheduler ve processor kullanıyor. LexiLink'te email/billing gibi ayrı side effect yoktu; boş altyapı yerine Stats inbox processing scheduled projection maintenance use case olarak bağlandı.
- **Contracts** — Stats Application'a `ICommand`, `CommandBase`, `ICommandHandler`, `IInternalCommand`, `IStatsInternalCommandScheduler` ve `IStatsInternalCommandProcessor` eklendi.
- **Storage** — Added `stats.InternalCommands` with `Id`, `EnqueueDate`, `DueDate`, `Type`, serialized `Data`, nullable `ProcessedDate`, `RetryCount`, `NextRetryDate`, and persisted `Error`.
- **Processor** — Added Stats internal command type map, scheduler, and processor. Processor due/unprocessed commands'ı okur, mapped command'ı deserialize edip MediatR üzerinden gönderir, başarıda processed işaretler, hatada retry metadata ve error yazar, batch'e devam eder.
- **Real use case** — Added `ProcessStatsInboxCommand` and handler. API `ProcessStatsInboxMessagesJob` now schedules this internal command and runs the internal command processor; Stats inbox processing is no longer called directly by the Quartz job.
- **Tests** — Stats integration suite now covers unknown internal command failure metadata while a valid `ProcessStatsInboxCommand` in the same batch still processes a Stats inbox message. Architecture tests now require internal commands to have public parameterless constructors.
- **Verification** — DbUp applied `stats.Tables.040_InternalCommands.sql`; `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0/0; Stats integration tests 7/7; API tests 3/3; Architecture tests 30/30.
- **Result** — Slice 12 is complete for the first real delayed/retried side-effect path. Next alignment slice is Event Bus abstraction.

### Slice 11 — Stats raw inbox pattern (2026-05-12)

- **Kamil reference check** — Kamil consuming module'larda incoming integration event'leri önce raw inbox'a kaydediyor, sonra ayrı processing path ile işliyor. LexiLink Stats daha önce projection SQL'iyle aynı anda idempotency row yazıyordu; bu replay/failure isolation için zayıftı.
- **Inbox contract** — Added `IStatsInbox` and `IStatsInboxProcessor` under Stats application processing contracts. Existing Stats integration-event handlers now append serialized inbox messages instead of projecting inline.
- **Raw storage** — `stats.InboxMessages` now stores `Type`, serialized `Data`, nullable `ProcessedDate`, `RetryCount`, `NextRetryDate`, and persisted `Error`. Fresh schema and existing database migration were both updated.
- **Processor** — Added Stats infrastructure inbox type map, append-only inbox writer, and processor. The processor deserializes known integration events, updates `PlayerStats`, marks success, persists retry/error metadata on failure, and continues the batch.
- **Scheduling** — Added API `ProcessStatsInboxMessagesJob : IJob` and registered it in Quartz next to outbox processing, using the same polling interval baseline.
- **Tests** — Stats integration tests now run `ProcessOutboxAsync()` then `ProcessStatsInboxAsync()` for real producer flows. Added coverage for duplicate integration event idempotency and inbox failure metadata while a valid message still projects.
- **Verification** — DbUp applied `stats.Tables.021_InboxMessages_RawProcessing.sql`; `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0/0; Stats integration tests 6/6; API tests 3/3; Architecture tests 27/27.
- **Result** — Slice 11 is complete for Stats. The next alignment slice is Internal Commands, but it should be tied to a real delayed/retried side effect instead of added as empty ceremony.

### Slice 10a — Outbox retry/error tracking baseline (2026-05-12)

- **Kamil reference check** — Kamil's outbox processing is scheduled and records processing failures. LexiLink already had per-message exception isolation, but failed messages only logged errors and stayed immediately eligible for reprocessing.
- **Retry metadata** — Added `RetryCount`, `NextRetryDate`, and `Error` to `Common.Application.Outbox.OutboxMessage`, EF mappings, and both producer outbox tables (`games.OutboxMessages`, `players.OutboxMessages`). Added DbUp scripts `071_OutboxMessages_RetryMetadata.sql` for existing databases and updated `070_OutboxMessages.sql` for fresh databases.
- **Processor behavior** — `OutboxProcessor` now selects only unprocessed messages whose retry delay has elapsed and whose retry count is below `MaxRetryCount`. On success it sets `ProcessedDate` and clears retry metadata. On failure it increments `RetryCount`, persists a truncated error string, sets `NextRetryDate`, logs the retry count, and continues the batch.
- **Options** — Added `OutboxProcessingOptions` with `PollingInterval`, `MaxRetryCount`, and `RetryBackoff`. The hosted service now reads the polling interval from options instead of hard-coded `5s`.
- **Tests** — Added Stats integration coverage that inserts an unmapped Games outbox message, runs the processors, verifies retry metadata is persisted, and verifies a valid Players outbox message in the same run still projects into Stats.
- **Verification** — DbUp applied 2 scripts locally; `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0 warning/0 error; `dotnet test src/Modules/Stats/IntegrationTests/LexiLink.Modules.Stats.IntegrationTests.csproj --no-restore --disable-build-servers -v minimal` → 5/5.
- **Carry-over** — This closes retry/error persistence, not the full Kamil Quartz-style scheduler. Replacing the hosted polling service with a real scheduled job remains the next outbox-alignment step if we choose to continue hardening before Raw Inbox.

### Slice 10b — Quartz scheduled outbox processing (2026-05-12)

- **Kamil reference check** — Kamil runs outbox processing as scheduled jobs, not a hand-written infinite hosted loop. LexiLink's retry/error tracking was in place, but the trigger was still `OutboxProcessingHostedService`.
- **Quartz package** — Added `Quartz.Extensions.Hosting` `3.18.1` to central package management and API package references. NuGet reports this package as compatible with `net10.0`.
- **Scheduled job** — Added API `ProcessOutboxMessagesJob : IJob` with `[DisallowConcurrentExecution]`. The job opens a scope, resolves all registered `IOutboxProcessor` instances, and runs them with per-processor exception isolation.
- **Host wiring** — Replaced `AddHostedService<OutboxProcessingHostedService>()` with `AddQuartz(...)` + `AddQuartzHostedService(...)`. The trigger uses `OutboxProcessingOptions.PollingInterval`, starts immediately, and repeats forever. The old handwritten hosted loop was removed.
- **Verification** — `dotnet restore LexiLink.sln --disable-build-servers -v minimal` passed; `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` passed with 0 errors (known EF materialization warnings surfaced on clean rebuild); API auth tests 3/3; Stats integration tests 5/5.
- **Result** — Slice 10 is complete: outbox now has retry/error persistence and scheduled processing. Next alignment slice is Raw Inbox pattern.

### Slice 9 — Auth/authorization baseline (2026-05-12)

- **Kamil reference check** — Kamil has explicit user access/auth boundaries before module operations are public. LexiLink had `IExecutionContextAccessor` reading a `sub` claim, but no authentication middleware was populating claims and endpoint groups were open.
- **Authentication scheme** — Added API `LexiLinkBearer` authentication handler. Current baseline accepts `Authorization: Bearer <player-guid>` and emits `sub` + `NameIdentifier` claims. This is intentionally a baseline/test scheme, not final Apple/Google/JWT verification.
- **Execution context** — `ExecutionContextAccessor` now reads `sub` first and falls back to `ClaimTypes.NameIdentifier`.
- **Authorization policy** — Added `AuthenticatedPlayer` policy. Categories, Links, Games, Stats, and most Players endpoints require it. `POST /players/guest` and `GET /players/by-auth` remain anonymous because they are registration/login lookup entry points.
- **API tests** — Added `LexiLink.API.Tests` with WebApplicationFactory smoke tests: root allows anonymous, `/categories` returns 401 without bearer token, and invalid bearer token returns 401. The test project removes hosted services so auth behavior can be checked without a database.
- **Quality gate** — Added API tests to `scripts/test.sh` DB-free test list, so local and CI gates include auth smoke coverage.
- **Verification** — `dotnet restore LexiLink.sln --disable-build-servers -v minimal` passed; `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` passed 0/0; `dotnet test src/API/LexiLink.API.Tests/LexiLink.API.Tests.csproj --no-restore --disable-build-servers -v minimal` passed 3/3. Existing EF materialization warnings appeared during test build from already-known domain private constructors.
- **Carry-over** — Real token issuing/verification and guest-to-auth transition rules remain future auth work. The next Kamil alignment slice is outbox scheduling/retry hardening.

### Slice 8 — CI quality gate (2026-05-12)

- **Kamil reference check** — Kamil has a repeatable automated build/test gate. LexiLink already had `scripts/test.sh` for local serial integration execution, but no CI workflow.
- **GitHub Actions workflow** — Added `.github/workflows/ci.yml`. It runs on PRs and pushes to `main`/`master`.
- **Postgres service** — CI starts `postgres:17` with database `lexilink`, user `lexiadmin`, password `0852`, matching the integration-test connection string used by module test bases.
- **Build and migration sequence** — Workflow runs `dotnet restore`, `dotnet build LexiLink.sln --no-restore --disable-build-servers -v minimal -m:1`, then applies DbUp scripts via `LexiLink.DatabaseMigrator`.
- **Test sequence** — Workflow runs `./scripts/test.sh --no-restore -v minimal`, preserving the local quality gate order: DB-free projects first, integration projects serially.
- **Conscious scope** — No Nuke and no warnings-as-errors yet. Those stay deferred until warnings/analyzer policy is worth the extra friction.

### Documentation reset — Kamil alignment plan clarified (2026-05-12)

- **Problem** — `activeContext.md` had accumulated current state, delivered history, long decision notes, and implementation details in one place. `ROADMAP.md` also still contained older historical sprint checklists, making the next Kamil-alignment sequence hard to see.
- **Active context cleanup** — Rewrote `activeContext.md` as a short current-state file: completed alignment slices, active constraints, working files to watch, and the immediate next action.
- **Roadmap alignment backlog** — Added a top-level `Kamil Alignment Backlog` to `ROADMAP.md`. It explicitly excludes API endpoint style and PostgreSQL/DbUp, then orders the remaining Kamil differences: CI quality gate, auth/authorization, outbox scheduling/retry, raw inbox, internal commands, event bus abstraction, module composition isolation, and time abstraction.
- **Comparison doc cleanup** — Updated `kamil-modular-monolith-comparison.md` so it points to `ROADMAP.md` for execution order, separates "Sıraya Alındı", "Kamil'den Alındı", "Kamil'den Yakında Al", and "Körlemesine Kopyalama", and keeps the conscious non-actions visible.
- **No production code changes** — This was documentation-only planning work.

### Slice 7 — Central build/package governance + application convention ArchTests (2026-05-12)

- **Kamil reference check** — Kamil's sample centralizes common MSBuild defaults and package versions, and supplements layer tests with application-level convention tests. LexiLink had repeated `TargetFramework`/nullable/implicit-usings settings and repeated package versions across project files.
- **Central MSBuild defaults** — Added root `Directory.Build.props` with shared `net10.0`, nullable, and implicit usings defaults. Project files now inherit these instead of repeating them.
- **Central package management** — Added root `Directory.Packages.props` with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` and all existing package versions. `.csproj` files now keep versionless `PackageReference` entries. EF Core package references were normalized to central `Microsoft.EntityFrameworkCore` `10.0.4`.
- **Application convention tests** — Added `ApplicationConventionTests` to lock in Kamil-style rules that fit LexiLink's current design: application handlers are internal, validators are internal, command/query request objects expose no public setters, and request handlers go through module `ICommandHandler`/`IQueryHandler` contracts instead of raw `MediatR.IRequestHandler`.
- **Verification** — `dotnet restore LexiLink.sln --disable-build-servers -v minimal` → all projects up-to-date; `dotnet test src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj --no-restore --disable-build-servers -v minimal` → 27/27 passing; `dotnet build LexiLink.sln --no-restore --disable-build-servers -v minimal -m:1` → 0 warning/0 error.
- **Conscious scope** — No `Directory.Build.targets` implicit project-reference magic yet. Explicit project references remain easier to audit at current repo size.

### Test runner alignment — shared integration DB, serial integration projects (2026-05-12)

- **Kamil reference check** — Kamil's integration tests use a dedicated integration-test database and clean it around tests. The important constraint is controlled execution over the shared DB, not pretending parallel solution-level test cleanup is safe.
- **Local quality gate** — Added `scripts/test.sh`. It runs DB-free test projects first, then Games/Players/Stats integration test projects one by one. Extra arguments are forwarded to `dotnet test`, and the script always passes `-m:1` so MSBuild does not spawn parallel worker nodes. `./scripts/test.sh --no-restore -v minimal` is the normal fast verification command.
- **Documentation** — Updated `CLAUDE.md` and `activeContext.md` to stop recommending plain `dotnet test` as the all-up command. `dotnet test LexiLink.sln -m:1` remains the solution-level equivalent.
- **Known behavior** — Plain parallel `dotnet test LexiLink.sln` can still race because integration suites clean overlapping tables in the same local DB. That is expected unless we later choose the heavier per-project database isolation approach.

### Slice 6 — Stats read surface completion + composition hardening (2026-05-12)

- **Profile snapshot completed** — `PlayerRegisteredDomainEvent`/integration event now carries `Locale`; `PlayerProfileUpdatedDomainEvent`/integration event carries `AvatarUrl` + `Locale`. Stats `PlayerStats` gained nullable `AvatarUrl` and `Locale` columns; `v_PlayerStats` was recreated with those fields.
- **Leaderboard** — Added `GetLeaderboardQuery`, `LeaderboardEntryDto`, `LeaderboardOrderBy`, and `GET /stats/leaderboard?orderBy=bestScore|totalScore|gamesCompleted&limit=...`. Query clamps limit to 1-100 and uses enum-controlled SQL ordering, not raw user SQL.
- **Real producer e2e** — `Stats.IntegrationTests` now completes a real Games game through commands, processes the Games outbox, and verifies Stats counters/scores update from the resulting `GameCompletedIntegrationEvent`.
- **Outbox processor hardening** — `OutboxProcessor` now isolates failures per message: one bad outbox message is logged and left unprocessed while the rest of the batch continues.
- **Shared-container composition bug fixed** — Stats e2e exposed that Games/Players both registering abstract `DbContext`, `IUnitOfWork`, `IDomainEventsDispatcher`, and `IOutbox` in one Autofac container could make a Games command commit the wrong context/outbox. Games and Players now use module-owned `GamesUnitOfWork`/`PlayersUnitOfWork` and module-owned domain event dispatchers over concrete `GamesContext`/`PlayersContext` + concrete module `OutboxAccessor`. Generic notification domain-event decorator was removed from module Autofac wiring so Stats integration-event handlers are not wrapped by producer domain-event dispatch infrastructure.
- **Integration tests** — Stats integration suite is now 4 tests: Players lifecycle outbox projection including profile snapshot, direct duplicate GameCompleted idempotency, real Games outbox → Stats projection, and leaderboard ordering.
- **Verification** — `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0 error/0 warning; `dotnet test src/Modules/Stats/IntegrationTests/LexiLink.Modules.Stats.IntegrationTests.csproj --no-restore --disable-build-servers -v minimal` → 4/4; Games integration 18/18; Players integration 4/4; ArchTests 15/15; `dotnet test LexiLink.sln --no-restore --disable-build-servers -v minimal -m:1` → all executable tests passing. Note: plain parallel solution test can race because integration suites share and clean the same local DB.
- **Conscious scope** — Still no Stats Domain project. Current behavior is read-model projection and queries; add a Domain layer only when Stats owns real invariants, not just counters/views.

### Slice 5 — Stats module closes Integration Events / Outbox-Inbox gap (2026-05-12)

- **Kamil check before coding** — Games/Players doğrudan Stats'i çağırmadı; Stats de producer module Application/Domain/Infrastructure internals'a bağlanmadı. Cross-module contract için `Games.IntegrationEvents` ve `Players.IntegrationEvents` public assembly'leri eklendi.
- **Producer outbox path** — Games `GameCompletedDomainEvent` ve Players `PlayerRegistered/AuthProviderLinked/PlayerProfileUpdated` domain event'leri için concrete `IDomainEventNotification<T>` wrapper'ları eklendi. Wrapper payload'ları scalar tutuldu; outbox processor deserialize sırasında domain VO/private ctor sorunlarına girmiyor.
- **Common processor** — `Common.Infrastructure.Outbox.OutboxProcessor` per-module outbox table'ını okur, `DomainNotificationsMap` ile notification type'ı çözer, MediatR publish eder, sonra `ProcessedDate` set eder. API'de `OutboxProcessingHostedService` 5 saniyelik polling loop ile Games/Players processors'ı çalıştırır. Slice 6'da per-message failure isolation eklendi.
- **Integration events** — Producer notification handlers `GameCompletedIntegrationEvent`, `PlayerRegisteredIntegrationEvent`, `AuthProviderLinkedIntegrationEvent`, `PlayerProfileUpdatedIntegrationEvent` publish eder. Stats bu event'leri consume eder.
- **Stats module** — `Stats.Application` query facade + `INotificationHandler<>` consumer'ları; `Stats.Infrastructure` `StatsStartup`, Autofac module, Dapper projection updater. Initial read API: `GET /stats/players/{playerId}`. Slice 6'da leaderboard API eklendi.
- **Inbox/idempotency** — `stats.InboxMessages` integration event `Id` primary key ile exactly-once projection behavior sağlar. Duplicate `GameCompletedIntegrationEvent` publish testinde counters tek kez artıyor.
- **Database** — `stats` schema, `PlayerStats`, `InboxMessages`, `v_PlayerStats` DbUp scripts eklendi. Local DbUp run applied 5 scripts: the pending Games completed-pair index plus 4 Stats scripts.
- **Tests** — `Stats.IntegrationTests` eklendi: Players register/link command → Players outbox → processor → Stats projection; duplicate GameCompleted integration event → Inbox idempotency. Existing Games integration completion test random hint budget bağımlılığından arındırıldı; known chain üzerinden direct steps atıyor.
- **Verification** — Initial verification was Stats integration 2/2; Slice 6 superseded this with Stats integration 4/4 and serial full-solution pass.
- **Conscious scope** — Stats has no rich Domain model yet because this slice is an async projection/read-model. Leaderboards and richer stats aggregates can come after the first integration-event path is stable.

### Next planned — Post-Stats candidates

- **Candidate next slice** — replace the simple 5-second hosted outbox poller with a Quartz-backed processor plus retry/backoff/dead-letter semantics.
- **Candidate next Stats feature** — richer stats such as streaks, per-category stats, or period-based leaderboard.
- **Processor caveat** — current outbox processor is a simple hosted poller. Quartz/retry/backoff/dead-letter behavior is intentionally deferred.

### Slice 4 — Completed start-target pairs must not repeat per player (2026-05-11)

- **Kamil check before coding** — module'lar birbirini doğrudan çağırmamalı. Cross-module bilgi gerekiyorsa integration event veya local projection kullanılmalı. Games game creation sırasında Players DB'sine/contract'larına doğrudan bakmayacak.
- **Feature choice** — Aynı player daha önce tamamladığı `StartLinkId` + `TargetLinkId` çiftini yeni oyunda tekrar almamalı. Bu oyun üretim kuralı Games tarafında uygulanır; completed-pair geçmişi Games'in kendi read model'inden okunur.
- **Domain event payload** — `GameCompletedDomainEvent` artık `PlayerId`, `StartLinkId`, `TargetLinkId`, `Score` taşır. Böylece ileride gerçek integration event/outbox consumer eklendiğinde gerekli payload domain event'te hazır.
- **No-repeat selection** — `CompletedGameLinkPair` + `ICompletedGameLinkPairRepository` eklendi. `CreateGameCommandHandler` player/category completed pair geçmişini yükler ve `Puzzle.Create`'e geçirir. `Puzzle.Create` completed pairs'i eler; tüm uygun çiftler tüketilmişse mevcut `PuzzleTargetLinkMustBeReachableRule` kırılır.
- **Infrastructure read model** — `CompletedGameLinkPairRepository` Dapper ile `games.Games` tablosundan `State = 'Completed'` kayıtlarını okur. `080_IX_Games_CompletedPairs.sql` lookup index'i eklendi.
- **Tests** — `PuzzleTests.Create_WhenPlayerCompletedPairsExist_DoesNotReuseCompletedStartTargetPair` eklendi; `GameCompletedDomainEvent` testi yeni payload'u doğrular.
- **Verification** — `dotnet test src/Modules/Games/Tests/LexiLink.Modules.Games.Tests.csproj --no-restore` → 85/85 passing; `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0 error/0 warning; `dotnet test src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj --no-restore` → 12/12 passing; `dotnet test LexiLink.sln --no-restore` → all executable tests passing (144 total; `Common.Tests` still contains no tests).
- **Conscious Kamil decision** — Player module'a direct kayıt/lookup yapılmadı. Bu özellik game generation invariant'ı olduğu için Games bounded context içinde çözüldü. Cross-module async integration event processing hâlâ ayrı bir gap olarak duruyor.

### Slice 3 — Module initialization/composition wiring (2026-05-11)

- **Kamil check before coding** — Kamil'de API host module initialization çağrısı yapar; module kendi service/composition-root ayrıntılarını saklar. Bizde `Program.cs` şu an `GamesContext`, `PlayersContext`, module Autofac modules, Outbox modules ve typed-id converter detaylarını doğrudan biliyor.
- **Startup APIs** — `GamesStartup` ve `PlayersStartup` eklendi. Her biri `Initialize(IServiceCollection, connectionString)`, `InitializeCompositionRoot(ContainerBuilder, connectionString)`, `CheckMappings()` giriş noktalarını expose eder.
- **Hidden module details** — Per-module `DbContext` registration, typed-id converter replacement, Autofac module registration, Outbox module registration ve domain notification map artık module Infrastructure içinde kalır.
- **API host simplification** — `Program.cs` yalnızca `GamesStartup` / `PlayersStartup` çağırır. `GamesContext`, `PlayersContext`, module `OutboxModule`, typed-id converter ve module Autofac module detaylarına doğrudan bağımlılık kaldırıldı.
- **ArchTest lock** — API assembly için module outbox namespaces, module DbContext types ve `Microsoft.EntityFrameworkCore` bağımlılıkları yasaklandı. Eski composition wiring pattern'i geri gelirse test kırılır.
- **Verification** — `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0 error/0 warning; `dotnet test src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj --no-restore` → 12/12 passing; `dotnet test LexiLink.sln --no-restore` → all executable tests passing (143 total; `Common.Tests` still contains no tests).
- **Conscious scope** — Bu slice mevcut tek ASP.NET host + shared Autofac container modelini korur. Kamil'deki tamamen ayrı module container/static composition root mimarisine bir anda geçmek yerine, host'tan module-specific wiring detaylarını kaldıran güvenli ara adım uygulandı.

### Slice 2 — API module facade (2026-05-11)

- **Kamil check before coding** — Kamil'in API ↔ Module iletişimi doğrudan MediatR `ISender` değildir. API her module için küçük bir public facade kullanır (`ExecuteCommandAsync<TResult>`, `ExecuteCommandAsync`, `ExecuteQueryAsync<TResult>`). Module kendi request execution/composition ayrıntısını saklar.
- **Module facade contracts** — `IGamesModule` ve `IPlayersModule` Application contract'larına eklendi. İmza Kamil'in üç operasyonunu korur; proje stiline uygun olarak `CancellationToken` optional geçirildi.
- **Infrastructure implementation** — `GamesModule` ve `PlayersModule` MediatR-backed facade olarak Infrastructure tarafında eklendi ve ilgili Autofac module içinde `IGamesModule` / `IPlayersModule` olarak register edildi.
- **API endpoint dispatch** — Games/Players Minimal API endpoint'leri artık `ISender` inject etmiyor; command/query çağrılarını ilgili module facade üzerinden yapıyor.
- **ArchTest lock** — API module endpoint namespace'i için `MediatR` ve module Infrastructure bağımlılıkları yasaklandı. Böylece eski endpoint dispatch pattern'i geri gelirse test kırılır.
- **Verification** — `dotnet build src/API/LexiLink.API/LexiLink.API.csproj --no-restore --disable-build-servers -v minimal` → 0 error/0 warning; `dotnet test src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj --no-restore` → 11/11 passing; `dotnet test LexiLink.sln --no-restore` → all executable tests passing (142 total; `Common.Tests` still contains no tests).
- **Conscious remaining gap** — API host'un module Infrastructure projelerini composition için referanslaması şimdilik kalır. Kamil'deki static module initialization/composition-root encapsulation daha sonra ayrı slice olarak ele alınabilir.

### Slice 1 — Architecture tests baseline (2026-05-11)

- **`src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj`** — Kamil'in Architecture Unit Tests yaklaşımına uygun ayrı test projesi. NUnit 4 + FluentAssertions + `NetArchTest.Rules`; `[Category("ArchTests")]` base class. Project `LexiLink.sln` altına `Tests/ArchitectureTests` solution folder'ında eklendi.
- **Layer dependency rules** — Games/Players Domain katmanları kendi Application/Infrastructure/API ve diğer modüle bağımlı olamaz; Application katmanları kendi Infrastructure/API ve diğer modüle bağımlı olamaz; Infrastructure katmanları diğer modüle ve API'ye bağımlı olamaz. Bu Kamil'in "module boundaries + clean architecture per module" kuralını kilitler.
- **Domain model rules** — `*Rule` tipleri `IBusinessRule` implement eder; `IDomainEvent` tipleri `*DomainEvent` suffix'i taşır; aggregate root isimleri (`Category`, `Link`, `Game`, `Player`) `IAggregateRoot` implement eder; `Entity` türevlerinde public ctor yoktur.
- **Verification** — `dotnet test src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj` → 10/10 passing; `dotnet test LexiLink.sln` → all executable test projects passing (Games 102 + Players 29 + ArchTests 10 = 141 tests; `Common.Tests` still contains no tests).
- **Known Kamil gap intentionally not enforced yet** — API hâlâ module facade (`IGamesModule`/`IPlayersModule`) yerine module Infrastructure projelerine ve MediatR `ISender` kullanımına doğrudan bağlı. Kamil'de API küçük module interface'leri üzerinden konuşuyor. Bu bir sonraki alignment slice olarak ele alınmalı; bu test seti mevcut mimariyi kırmadan ilk güvenlik ağıdır.

## Sprint 7 — Players Module ✅ closed (Slices 1-6 done, started 2026-05-11)

Sprint goal: ship the second module (Players) to validate the modular monolith pattern. Scope per Sprint 7 plan in ROADMAP.md: minimum-viable Player identity with Guest + Apple + Google auth, DisplayName + Discriminator profile (Discord-style), no stats (Stats deferred to Sprint 8 as its own module).

### Slice 6 — Tests + integration smoke (2026-05-11)

- **`src/Modules/Players/Tests/`** — NUnit 4 + FluentAssertions + NSubstitute, mirroring Games test style. Coverage: `PlayerRegisterTests` (7), `PlayerLinkAuthProviderTests` (6), `PlayerUpdateProfileTests` (6), `DiscriminatorTests` (6) = **25 unit tests**. The final missing branch was `AvatarUrlMustBeValidIfProvidedRule.MaxLength` coverage via `UpdateProfile_WhenAvatarUrlExceedsMaxLength_BreaksAvatarUrlMustBeValidIfProvidedRule`.
- **`src/Modules/Players/IntegrationTests/`** — real Postgres integration suite with Kamil-style composition root: `ServiceCollection` + `AddDbContext<PlayersContext>` + `AddMediatR`, Autofac `PlayersAutofacModule`, separate empty `OutboxModule` dictionary, `TestExecutionContextAccessor`, `[Category("Integration")]`, and per-test cleanup of `players.PlayerAuthIdentities`, `players.Players`, `players.OutboxMessages`.
- **Integration coverage**: `RegisterGuestPlayer_Test`, `LinkAuthProvider_AndGetByAuthProvider_Test`, `UpdatePlayerProfile_Test`, `GetPlayerByAuthProvider_WhenUnknown_ReturnsNull_Test` = **4 integration tests**. This verifies command decorators + EF writes + Dapper read side + enum string mapping against the real `players` schema/views.
- **Solution wiring** — `LexiLink.Modules.Players.IntegrationTests.csproj` added to `LexiLink.sln` under `Modules/Players/IntegrationTests`.
- **Verification** — `dotnet test src/Modules/Players/Tests/LexiLink.Modules.Players.Tests.csproj` → 25/25 passing; `dotnet test src/Modules/Players/IntegrationTests/LexiLink.Modules.Players.IntegrationTests.csproj` → 4/4 passing against local Postgres.

### Slice 5 — API host wiring (2026-05-11)

- **`src/API/LexiLink.API/Modules/Players/PlayerEndpoints.cs`** — 5 routes: `POST /players/guest`, `POST /players/{id}/auth-providers`, `PATCH /players/{id}/profile`, `GET /players/{id}`, `GET /players/by-auth?provider=Apple&externalId=...`.
- **`Program.cs` wiring** — `PlayersContext`, `PlayersAutofacModule`, and separate `PlayersOutboxModule` registered alongside Games equivalents; type aliases resolve the two `OutboxModule` class names; both `CheckMappings` calls run after container build.
- **Smoke verified end-to-end** — register guest → link Apple → patch profile → by-auth lookup returns same player; unknown sub returns 404; validation catches empty device id; duplicate Apple sub is rejected by DB unique constraint.
- **Slice fix** — EF auto-discovered `Player.AuthIdentities` public getter as a second navigation; fixed with `builder.Ignore(p => p.AuthIdentities)` in `PlayerEntityTypeConfiguration`.

### Slice 4 — Database scripts (2026-05-11)

- **`src/Database/LexiLink.Database/Structure/players/Schema/001_CreateSchema.sql`** — creates `players` schema idempotently.
- **Tables** — `010_Players.sql` with unique `(DisplayName, DiscriminatorValue)`, `020_PlayerAuthIdentities.sql` with composite PK `(PlayerId, Provider)` + unique `(Provider, ExternalId)`, and `070_OutboxMessages.sql` mirroring Games outbox shape with prefixed PK name.
- **View** — `110_v_Players.sql` denormalizes `Handle = DisplayName || '#' || lpad(DiscriminatorValue::text, 4, '0')`.
- **DbUp verified** — first run applied 5 scripts; idempotent re-run showed 0 pending scripts.

### Slice 1 — Domain layer (2026-05-11)

- **`src/Modules/Players/Domain/LexiLink.Modules.Players.Domain.csproj`** — net10.0, refs `Common.Domain`, `InternalsVisibleTo` for `Players.Application` + `Players.Infrastructure` + `Players.Tests`.
- **`Player` aggregate root** with `RegisterGuest` factory (`internal`), `LinkAuthProvider` (Apple/Google generic, Guest reddedilir), `UpdateProfile` (avatar + locale). Fields: `_displayName`, `_discriminator` (owned VO), `_avatarUrl?`, `_locale`, `_createdAt`, `_isGuest`, `_authIdentities` (owned collection). `LinkAuthProvider` ilk non-Guest entry sonrası `_isGuest=false` yapar.
- **Owned VOs**: `PlayerId` (TypedIdValueBase), `Discriminator` (`Value: int` 1-9999, `ToString() => "D4"` formatı), `AuthIdentity` (`Provider`, `ExternalId`, `Email?`, `LinkedAt`).
- **Enum** `AuthProvider`: `Guest=0 | Apple=1 | Google=2`. Guest sadece factory tarafından eklenir, `LinkAuthProvider` ile eklenemez (rule).
- **9 domain rules**: `DisplayNameMustNotBeEmptyRule`, `DisplayNameMustMeetMinimumLengthRule` (`MinLength=2`), `DisplayNameMustNotExceedMaxLengthRule` (`MaxLength=32`), `LocaleMustBeValidFormatRule` (regex `^[a-z]{2}-[A-Z]{2}$` BCP 47 short form), `DeviceIdMustNotBeEmptyRule`, `ExternalAuthIdMustNotBeEmptyRule`, `PlayerMustNotAlreadyHaveAuthProviderRule`, `SocialAuthProviderRequiredRule`, `DiscriminatorMustBeInRangeRule`, `AvatarUrlMustBeValidIfProvidedRule` (HTTP(S) URL, max 500 char — null/empty serbest).
- **3 domain events**: `PlayerRegisteredDomainEvent(PlayerId, DisplayName, Discriminator, IsGuest)`, `AuthProviderLinkedDomainEvent(PlayerId, AuthProvider, ExternalId)`, `PlayerProfileUpdatedDomainEvent(PlayerId)`.
- **Contracts**: `IPlayerRepository` (`GetByIdAsync` + `GetByAuthProviderAsync` + `AddAsync` — Kamil'in `IUserRepository`'sinde olmayan `GetByAuthProvider` mobile login flow için kritik); `IDiscriminatorGenerator` domain service interface (handler tarafında async DB lookup yapan implementasyona delege eder, factory deterministik kalır).
- **Build clean** (0 error, 1 expected CS8618 warning on `Id` — Game/Category pattern'iyle aynı).

### Slice 2 — Application layer (2026-05-11)

- **`src/Modules/Players/Application/LexiLink.Modules.Players.Application.csproj`** — refs `Players.Domain` + `Common.Application`; `Dapper 2.1.72`, `FluentValidation 12.1.1`.
- **Per-module CQRS contracts (Kamil-faithful)** — `Contracts/{ICommand, ICommand<TResult>, IQuery<TResult>, CommandBase, CommandBase<TResult>, QueryBase<TResult>}` + `Configuration/Commands/ICommandHandler` + `Configuration/Queries/IQueryHandler`. Games modülünün ve Kamil'in birebir aynısı.
- **`IPlayerContext` + `PlayerContext` Application'da, Infrastructure'da değil** — Kamil'in `Meetings.Application.Members.MemberContext`'iyle aynı yerleşim. Impl `IExecutionContextAccessor.UserId`'yi `new PlayerId(...)` ile sarar. HTTP'den okumadığı için Infrastructure'da olması gereksiz. Interface Kamil'de `Domain.Members.IMemberContext` (Domain'de — debatable), biz Application'da daha temiz seçim yaptık.
- **3 command**:
  - `RegisterGuestPlayerCommand(DeviceId, DisplayName, Locale) : CommandBase<Guid>` — handler `IDiscriminatorGenerator.GenerateForAsync(displayName)` çağırır, sonra `Player.RegisterGuest(...)`, son olarak `_playerRepository.AddAsync`.
  - `LinkAuthProviderCommand(PlayerId, Provider, ExternalId, Email?) : CommandBase` — handler Player'ı yükler (`NotFoundException` if null), `player.LinkAuthProvider(...)` çağırır.
  - `UpdatePlayerProfileCommand(PlayerId, AvatarUrl?, Locale) : CommandBase` — handler `player.UpdateProfile(...)` çağırır.
- **2 query**:
  - `GetPlayerByIdQuery(PlayerId) : QueryBase<PlayerDetailsDto>` — Dapper `QueryMultipleAsync` (Player from `v_Players` + AuthIdentities from `PlayerAuthIdentities`), NotFoundException atar.
  - `GetPlayerByAuthProviderQuery(Provider, ExternalId) : QueryBase<PlayerDetailsDto?>` — login flow için kritik; **null geçerli yanıt** (Apple/Google sub claim'i ilk kez gelince "yok" demek doğru cevap, NotFound exception değil). Dapper string-cast: `new { Provider = query.Provider.ToString(), ... }` çünkü Dapper enum'u default int gönderir ama kolon `varchar(32)`.
- **DTOs**: `PlayerDetailsDto(Id, DisplayName, Discriminator int, Handle, AvatarUrl?, Locale, IsGuest) { init AuthIdentities }`, `AuthIdentityDto(Provider, ExternalId, Email?, LinkedAt)`. Init-only collection + `dto with { AuthIdentities = ... }` pattern Game'in `GameDetailsDto.History` ile aynı.
- **FluentValidation `AbstractValidator<T>` per command** — yüzeysel kontroller (`NotEmpty`, `MaximumLength`/`MinimumLength` referansları `*Rule.MaxLength` sabitlerinden), default mesajlarla. Kamil `.WithMessage(...)` kullanıyor ama biz Games konvansiyonuna uyduk (proje içi tutarlılık).
- **Build clean** (0 error, 0 warning).

### Slice 3 — Infrastructure layer (2026-05-11)

- **`src/Modules/Players/Infrastructure/LexiLink.Modules.Players.Infrastructure.csproj`** — refs `Players.Application` + `Common.Infrastructure`; `Dapper 2.1.72`, `FluentValidation 12.1.1`, `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1`, `Serilog 4.3.1`. Games.Infrastructure ile birebir paket listesi.
- **`PlayersContext : DbContext`** — `DbSet<Player>`, `DbSet<OutboxMessage>`. `ConfigureConventions` reflects over referenced assemblies for `TypedIdValueBase` subclasses (Games pattern; PlayerId burada otomatik mapping kazanır). `OnModelCreating` → `ApplyConfigurationsFromAssembly`.
- **`PlayerEntityTypeConfiguration`** — `ToTable("Players", "players")`; private fields field-access (`_displayName`, `_avatarUrl?`, `_locale`, `_createdAt`, `_isGuest`); `OwnsOne<Discriminator>` `DiscriminatorValue` column; `OwnsMany<AuthIdentity>` → `players.PlayerAuthIdentities` (FK PlayerId, composite key `(PlayerId, Provider)`, `Provider` `varchar(32)` via `HasConversion<string>()`).
- **`PlayerRepository : IPlayerRepository`** — `GetByIdAsync` (EF), **`GetByAuthProviderAsync` two-step**: Dapper lookup `SELECT "PlayerId" FROM "players"."PlayerAuthIdentities" WHERE "Provider"=... AND "ExternalId"=...`, then EF load by id. Owned collection üzerinden direkt EF `Any(...)` query'leri private field/wrapper getter yüzünden kırılgan; iki adım garantili.
- **`RandomDiscriminatorGenerator : IDiscriminatorGenerator`** — Dapper ile aynı `DisplayName`'e ait mevcut discriminator'ları çekiyor, 10 random attempt + sequential 1-9999 fallback. Tüm 9999 değer dolu ise `InvalidOperationException`. Race condition unique constraint'le DB-level çözülüyor (handler-level retry henüz yok).
- **`OutboxAccessor : IOutbox`** + **`OutboxMessageEntityTypeConfiguration`** → `players.OutboxMessages` (Games kopyası; `IOutbox` interface Common.Application'da kalır, impl per-module).
- **`SqlConnectionFactory : ISqlConnectionFactory`** — Games'in kopyası; lifetime-scoped Npgsql connection caching.
- **6 decorator copies** (`Configuration/Processing/`): `UnitOfWorkCommandHandlerDecorator<T>` + `<T,TResult>`, `ValidationCommandHandlerDecorator<T>` + `<T,TResult>`, `LoggingCommandHandlerDecorator<T>` + `<T,TResult>`. Hepsi Players'ın kendi `ICommandHandler<T>` / `ICommandHandler<T,TResult>` / `ICommand` / `ICommand<T>` constraint'lerini target alıyor (per-module per `feedback_decorator_over_pipeline` memory).
- **`PlayersAutofacModule`** — `SqlConnectionFactory`, `DbContext` binding, `UnitOfWork`, `DomainEventsAccessor/Dispatcher`, `OutboxAccessor`, `PlayerRepository`, `RandomDiscriminatorGenerator`, `PlayerContext` (kendi yeri Application ama registration burada), `Random` singleton, handler/validator assembly scan, decorator chain `IRequestHandler<>/<,>` üzerinde UoW → Validation → Logging order, `INotificationHandler<>` üzerinde `DomainEventsDispatcherNotificationHandlerDecorator`.
- **`OutboxModule`** — Games kopyası; ctor `BiDictionary<string, Type>` alır, registers `DomainNotificationsMapper`, static `CheckMappings` Players.Application + Players.Infrastructure assemblies'i tarayıp eksik wrappers raporlar. Composition root Sprint 7 Slice 5'te her iki modül için ayrı dict + ayrı `RegisterModule` çağrısı yapacak.
- **Build clean** (0 error, 0 warning). Players modülü 4 csproj (Domain, Application, Infrastructure + future Tests) → 6 modül-projesinden 3'ü artık `LexiLink.sln`'de.

---

## Sprint 6 — Tests (closed 2026-05-09)

### Slice 1 — Test infra rework: xUnit → NUnit 4 + Kamil helpers

- **Package switch in both test projects (`Modules/Games/Tests` and `Common/Tests`)** — replaced `xunit 2.9.3` + `xunit.runner.visualstudio 3.1.4` + `<Using Include="Xunit" />` with **NUnit 4.0.1 + NUnit3TestAdapter 4.5.0 + FluentAssertions 6.12.0 + NSubstitute 5.1.0** + `<Using Include="NUnit.Framework" />` + `<Using Include="FluentAssertions" />`. Microsoft.NET.Test.Sdk + coverlet.collector kept as-is. Existing auto-scaffolded `UnitTest1.cs` placeholders deleted.
- **`Modules/Games/Tests/SeedWork/TestBase.cs`** — abstract base with five static helpers: `AssertPublishedDomainEvent<T>(Entity)`, `AssertPublishedDomainEvents<T>(Entity)` (plural, ≥1), `AssertDomainEventNotPublished<T>(Entity)`, `AssertBrokenRule<TRule>(TestDelegate)`, `AssertBrokenRuleAsync<TRule>(AsyncTestDelegate)`. The rule-assertion helpers wrap NUnit's `Assert.Catch<BusinessRuleValidationException>`/`Assert.CatchAsync<...>` and verify `BrokenRule.Should().BeOfType<TRule>()`.
- **`Modules/Games/Tests/SeedWork/DomainEventsTestHelper.cs`** — reflection walker. Two methods: `GetAllDomainEvents(Entity)` recursively collects events from the aggregate root and any nested `Entity` fields/collections (using `BindingFlags.Instance | Public | NonPublic`); `ClearAllDomainEvents(Entity)` does the same for clearing. Currently overkill for our domain (events live only on the aggregate root) but matches Kamil verbatim and stays useful as the Game aggregate grows nested entities. Avoids reflection cycles via `HashSet<object>` visited tracking.
- **`Modules/Games/Domain/LexiLink.Modules.Games.Domain.csproj` `InternalsVisibleTo` extended** — added `LexiLink.Modules.Games.Infrastructure` and `LexiLink.Modules.Games.Tests` to the existing `LexiLink.Modules.Games.Application` entry. Without this, the tests fall through to `FileSystemAclExtensions.Create(...)` from the BCL when `Category.Create(...)` is called (the `internal` factory is invisible, and `Category` is `public` so the type itself resolves but the method does not).

### Slice 2 — Category domain tests (10 tests)

`Categories/CategoryTests.cs` directly extends `TestBase`. Coverage:
- `Create_WithValidValues_IsSuccessful` (asserts `CategoryCreatedDomainEvent` with `CategoryId.Should().Be(category.Id)`).
- `Create_WhenNameIsEmpty_BreaksCategoryNameMustNotBeEmptyRule`, `Create_WhenNameIsWhitespace_...` (separate test for `"   "`).
- `Create_WhenNameExceedsMaxLength_BreaksCategoryNameMustNotExceedMaxLengthRule` (101 chars), `Create_WhenNameIsAtMaxLength_IsSuccessful` (boundary at 100).
- `Create_WhenDescriptionExceedsMaxLength_...` (501 chars).
- Same five for `EditGeneralInfo` (raises `CategoryEditedDomainEvent`).

### Slice 3 — Link domain tests (12 tests)

Three test files plus `LinkTestsBase` factory:
- `Links/LinkTestsBase.cs` — provides `NewCategoryId`, `NewLinkId`, `CreateLink(...)` factory that creates the aggregate then clears initial events for clean act-phase assertions.
- `Links/LinkTests.cs` (2): `Create_WithValidValues_RaisesLinkCreatedDomainEvent`, `Create_WithIsActiveFalse_StartsInactive`.
- `Links/LinkOutgoingTests.cs` (6): `AddOutgoingLink_{WhenValid_AddsAndRaisesEvent, WhenPointingToSelf, WhenTargetIsDifferentCategory, WhenAlreadyExists}` + `RemoveOutgoingLink_{WhenExists, WhenNotExists}`.
- `Links/LinkLifecycleTests.cs` (4): `Activate_{WhenInactive, WhenAlreadyActive}` + `Deactivate_{WhenActive, WhenAlreadyInactive}`.

### Slice 4 — Game command tests (33 tests across seven files)

`Games/GameTestsBase.cs` provides:
- `BuildPuzzle(start, target, optimalPath, difficulty)` — **uses reflection** to invoke `Puzzle`'s private `(CategoryId, Difficulty, LinkId, LinkId, IEnumerable<LinkId>)` ctor directly. This bypasses the random start-link selection in `Puzzle.Create`, which is non-deterministic for tests. The factory itself is exercised in `PuzzleTests`.
- `BuildGame(puzzle, maxSteps, hints, undos, resets, playerId, clearEvents)` — wraps `Game.Create` and clears initial events by default.
- `NeighborResolver((from, to)...)` — NSubstitute-based mock with explicit per-link mappings.
- `LinearNeighborResolver(linkA, linkB, linkC, ...)` — convenience for chains where each node's only neighbor is the next.
- `FixedScoreCalculator(points)` — NSubstitute mock returning a fixed `Score.Of(points)`.

Per-command files:
- `GameCreateTests.cs` (5): RaisesGameCreatedDomainEvent, StartsWithStateInitial, CurrentLinkIsStartLink, HistoryIsEmpty, ScoreIsNull.
- `GameStartTests.cs` (3): TransitionsToInProgress + RaisesGameStartedDomainEvent, WhenAlreadyStarted_BreaksRule, WhenAbandoned_BreaksRule.
- `GameMakeStepTests.cs` (7): valid step, four rule violations (not started, abandoned, invalid neighbor), completion path, last-step warning, fail-on-budget-exhaustion.
- `GameUseHintTests.cs` (4): from start position (returns `OnWrongPath` with first optimal step — start is excluded from `_optimalPath`), standing on path (returns `OnCorrectPath` with next), not-started rule, no-hints-remaining rule.
- `GameUndoTests.cs` (5): happy path, empty-history rule, not-started rule, last-step-warning → in-progress transition, no-undos-remaining rule.
- `GameResetTests.cs` (4): happy path, empty-history rule, not-started rule, no-resets-remaining rule.
- `GameAbandonTests.cs` (5): from initial, from in-progress, three "already finished" rule violations (abandoned, completed, failed).

### Slice 5 — Owned VO tests (29 tests)

- `Games/Allowances/{Hint,Undo,Reset}AllowanceTests.cs` — three near-identical test classes (one per allowance VO): `Of_StartsWithRemainingEqualToTotal_AndUsedZero`, `Consume_DecrementsRemainingAndIncrementsUsed`, `Consume_WhenRemainingIsZero_BreaksXxxAllowanceMustHaveRemainingRule` (+ `Consume_IsImmutable` on Hint to lock in the immutability contract).
- `Games/StepBudgetTests.cs` (10): `Of`, `Step`, `UndoStep` (+ floor at zero), `Reset`, plus the four predicates (`IsExhausted`, `IsAtLastWarning`, `IsBelowLastWarning` true/false branches).
- `Games/ScoreTests.cs` (3): `Of_StoresPoints`, `Of_AllowsZero`, `TwoScoresWithSamePoints_AreEqual` (locks in the `ValueObject` equality contract).
- `Games/Puzzles/PuzzleTests.cs` (6): exercises `Puzzle.Create` (the factory + random start-link selection), with NSubstitute mocks for `IPathFinderService` and `IGameConfigurationService`. Covers the two factory rules (`CategoryMustHaveEnoughLinksToStartGameRule` < 5 links; `PuzzleTargetLinkMustBeReachableRule` when pathFinder returns null) plus three `RequestHint` paths (on-path, off-path, standing-on-target).

**Slice 5 fix during run:** `UseHint_FromInProgress_..._ReturnsOnCorrectPath` initially failed — the test assumed standing on `start` returns `OnCorrectPath`. In fact `Puzzle.RequestHint` excludes `start` from `_optimalPath` (the optimal path is steps-after-start), so being at start returns `OnWrongPath` with the first optimal step as the recommendation. Test split into two: `UseHint_FromStartPosition_ReturnsOnWrongPathWithFirstOptimalStep` and `UseHint_WhenStandingOnOptimalPath_ReturnsOnCorrectPathWithNextStep` (latter steps onto the path first).

### Slice 6 — IntegrationTests project + handler tests (18 tests)

- **`src/Modules/Games/IntegrationTests/LexiLink.Modules.Games.IntegrationTests.csproj`** — new project. Packages: `Autofac 9.1.0`, `Autofac.Extensions.DependencyInjection 10.0.0`, `MediatR 14.1.0`, `Microsoft.EntityFrameworkCore 10.0.4`, `Microsoft.Extensions.{DependencyInjection,Logging} 10.0.4`, `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1`, `Serilog 4.3.1`, plus the same NUnit/FluentAssertions/NSubstitute test stack as UnitTests. Project refs: `Domain`, `Application`, `Infrastructure`. Added to `LexiLink.sln`.
- **`SeedWork/TestBase.cs`** — replicates the API's composition root in `[OneTimeSetUp]`: `AddDbContext<GamesContext>` with `UseNpgsql + ReplaceService<IValueConverterSelector, ...>`, `AddMediatR(typeof(IMediator).Assembly)`, `AddSingleton<IExecutionContextAccessor, TestExecutionContextAccessor>`, `Serilog.ILogger.None` for handler injection, `ContainerBuilder.Populate(services)` + `RegisterModule(new GamesAutofacModule(connStr))` + `RegisterModule(new OutboxModule(emptyDict))`. `[SetUp]` opens a fresh `BeginLifetimeScope()`, resolves `ISender` and `GamesContext`, then runs `DELETE FROM "games"."..."` cleanup across all 7 module-owned tables in FK-respecting order. `[TearDown]` disposes the scope. Connection string from `ASPNETCORE_LexiLink_IntegrationTests_ConnectionString` env var (default `localhost:5432/lexilink`). `[Category("Integration")]` so the suite can be filtered out in environments without Postgres.
- **`SeedWork/TestExecutionContextAccessor.cs`** — fake `IExecutionContextAccessor` returning fixed UserId + CorrelationId, `IsAvailable = true`. Required so `LoggingCommandHandlerDecorator` doesn't throw when pushing the request enricher.
- **Per-aggregate Helper classes** (Kamil's `MeetingHelper.cs` pattern):
  - `Categories/CategoryHelper.cs` — `CreateCategoryAsync(sender, name, description)`.
  - `Links/LinkHelper.cs` — `CreateLinkAsync`, `CreateLinksAsync` (variadic), `LinkBidirectionallyAsync`.
  - `Games/GameHelper.cs` — `CreateGameAsync`, `SetupChainedGameAsync` (creates a Category + 6 chained Links + edges + a Game; returns a `GameSetup` record with all created ids by word).
- **Test classes**:
  - `CategoryIntegrationTests.cs` (4): `CreateCategory_Test`, `EditCategory_Test`, `GetCategories_Test` (asserts both names returned in list), `GetCategoryDetails_LinkCountReflectsAssociatedLinks_Test` (creates the category, adds 2 links, asserts `LinkCount == 2`).
  - `LinkIntegrationTests.cs` (6): create + outgoing add/remove + activate/deactivate + GetLinksByCategory.
  - `GameIntegrationTests.cs` (8): create, start, full play-through using hint→step loop until completion (asserts state=Completed + non-null Score), use hint, undo, reset, abandon, GetGameById denormalized words.
- **Run result** — 18/18 pass against the same Postgres instance used by Sprint 5's smoke test. Each test takes ~50-100ms; full suite ~1-2s.

---

## Sprint 5 — Trailing Migration / Schema Deployment (closed 2026-05-09)

### Slice 6 — End-to-end smoke test against real Postgres (2026-05-09)

- **Migrator first run** — `dotnet run --project src/Database/LexiLink.DatabaseMigrator -- "Host=localhost;...;Database=lexilink;..." src/Database/LexiLink.Database/Structure` against an empty `lexilink` database. DbUp output: created `MigrationsJournal`, then ran 12 scripts in alphanumeric directory order (Schema/001, Tables/010..070, Views/110..140). All scripts journaled. Re-running is a no-op.
- **End-to-end via HTTP** — boot API on `localhost:5099` (`ASPNETCORE_ENVIRONMENT=Development`):
  - `POST /categories {"name":"Animals","description":"Animal-themed words"}` → `201 { id }`.
  - 6× `POST /links` (cat, mat, bat, bag, bug, rug) → 201 each.
  - 10× `POST /links/{src}/outgoing/{tgt}` for a bidirectional chain (cat↔mat↔bat↔bag↔bug↔rug) → 204 each.
  - `POST /games {"playerId":..., "categoryId":..., "difficulty":"Easy"}` → 201 with id; `GET /games/{id}` showed start=`bug`, target=`mat`, depth=3 (Easy = 3-5).
  - `POST /games/{id}/start` → 204.
  - 3× `POST /games/{id}/steps {"nextLinkId":...}` (bug→bag→bat→mat) → 204 each.
  - `GET /games/{id}` final state: `state="Completed"`, `score=300`, `stepsTaken=3`, `history` of 3 rows in correct order, `linkValue` denormalized through the v_Links join.
- **Read-side view verification** — `GET /categories` (lists Animals), `GET /categories/{id}` (returns `linkCount=6`), `GET /links?categoryId={id}` (returns 6 links). All four read-side views (`v_Categories`, `v_Links`, `v_Games`, `v_GameHistory`) are exercised by the smoke flow.
- **`OutboxMessages` table stays empty** — by design; we have no `IDomainEventNotification<T>` wrappers yet, so `DomainEventsDispatcher.DispatchEventsAsync` finds nothing to wrap and writes nothing to the outbox. Infrastructure is wired and ready.
- **Five EF / Dapper bugs surfaced and fixed during the run**:
  - `Score.Points` is `int` (non-null); had `.IsRequired(false)` on the property which EF rejects ("cannot be marked nullable because the type is not a nullable type"). Moved the optionality to the owned navigation: `builder.Navigation("_score").IsRequired(false)`.
  - `LinkId` (and other `TypedIdValueBase` subclasses) on owned-entity properties triggered `ConstructorBindingConvention` ("No suitable constructor found for type 'LinkId'") because `StronglyTypedIdValueConverterSelector` is consulted only after EF has decided whether a property is scalar vs navigation. Added a `ConfigureConventions` override on `GamesContext` that reflects over assemblies for `TypedIdValueBase` subclasses and registers `TypedIdValueConverter<T>` per type. Required adding an explicit parameterless ctor to `TypedIdValueConverter<T>` (the prior `(ConverterMappingHints? = null)` ctor wasn't visible to `Activator.CreateInstance(type)`).
  - `OptimalPathStep.Position` and `GameHistoryStep.StepNumber` are `int` PK columns. EF defaulted to `ValueGeneratedOnAdd` (treats as identity) and didn't pass values during INSERT — Postgres returned 23502 NOT NULL violation. Added `.ValueGeneratedNever()` on both property mappings.
  - `CategoryDetailsDto(...long LinkCount...)` mismatch: Postgres `COUNT(*)` returns BIGINT (`long` in C#) but the DTO declares `int`. Dapper positional-record materializer requires exact type match. Cast in SQL: `(SELECT COUNT(*)::int FROM ...)`.
  - First two `POST /links/.../outgoing/...` calls in the bash for-loop returned 404/405 (transient — server warming up?), forcing a manual retry of the bug↔rug edges. Not a code bug; documented as "warm-up race" — wait for the API to be fully ready before issuing the first edge POST.
- Build clean (0 error, 0 warning). Sprint 5 closed.

### Slice 5 — DomainNotificationsMapper + OutboxModule + CheckMappings (2026-05-09)

- **`Modules/Games/Infrastructure/Configuration/Outbox/OutboxModule.cs`** — separate Autofac module (not part of `GamesAutofacModule`). Ctor takes `BiDictionary<string, Type> domainNotificationsMap`. Registers `DomainNotificationsMapper` as singleton via `RegisterType<DomainNotificationsMapper>().As<IDomainNotificationsMapper>().WithParameter("domainNotificationMap", _domainNotificationsMap).SingleInstance()`. Static `CheckMappings(BiDictionary)` reflects over `Assemblies.Application` + the Infrastructure assembly for non-abstract types implementing `IDomainEventNotification`, throws `ApplicationException` listing any types missing from the dict.
- **`Program.cs` wiring** — builds `var domainNotificationsMap = new BiDictionary<string, Type>()` (initially empty — no cross-module notifications yet); inside the `ConfigureContainer` callback registers both `GamesAutofacModule` and `OutboxModule`; after `var app = builder.Build()` calls `OutboxModule.CheckMappings(domainNotificationsMap)`. Comment in code explains that future notifications get hand-listed before this point.
- **`GamesAutofacModule` cleanup** — removed the empty `Register(_ => new DomainNotificationsMapper(new BiDictionary<string, Type>()))` registration; the dispatcher now resolves `IDomainNotificationsMapper` from `OutboxModule`'s registration.
- Build clean.

### Slice 4 — EF-backed Outbox per-module (2026-05-09)

- **`Modules/Games/Infrastructure/Outbox/OutboxMessageEntityTypeConfiguration.cs`** (`internal`) maps `OutboxMessage` to `[games].[OutboxMessages]` with the column shape from the existing `Common.Application.Outbox.OutboxMessage` (Id PK, OccurredOn, Type, Data, ProcessedDate nullable).
- **`Modules/Games/Infrastructure/Outbox/OutboxAccessor.cs : IOutbox`** (`internal`, scoped) — injects `GamesContext`. `Add(OutboxMessage)` calls `_gamesContext.Set<OutboxMessage>().Add(message)`. The `IOutbox` interface stays in `Common.Application.Outbox`.
- **`GamesContext`** — added `DbSet<OutboxMessage> OutboxMessages` (the `IEntityTypeConfiguration` is auto-discovered by `ApplyConfigurationsFromAssembly`).
- **`GamesAutofacModule` registration switch** — `RegisterType<InMemoryOutbox>().As<IOutbox>().SingleInstance()` → `RegisterType<OutboxAccessor>().As<IOutbox>().InstancePerLifetimeScope().FindConstructorsWith(allCtors)`. The constructor finder is needed because `OutboxAccessor`'s ctor is `internal`.
- **`Common.Infrastructure/Outbox/InMemoryOutbox.cs`** + parent folder deleted. No remaining references.

### Slice 3 — Read-side view DDL (2026-05-09)

Four `.sql` files under `src/Database/LexiLink.Database/Structure/games/Views/`:

- `110_v_Categories.sql` — `CREATE OR REPLACE VIEW "games"."v_Categories" AS SELECT Id, Name, Description FROM "games"."Categories"`.
- `120_v_Links.sql` — Id, CategoryId, Value, Description, IsActive.
- `130_v_Games.sql` — all Game scalars + computed `("HintsRemaining" + "HintsUsed") AS "HintsTotal"`, same for Undos and Resets. Matches the column shape `GetGameByIdQueryHandler` selects.
- `140_v_GameHistory.sql` — GameId, StepNumber, LinkId. Joined to v_Links by `GetGameByIdQueryHandler` for word denormalization.

`CREATE OR REPLACE VIEW` so re-running on an existing DB is safe; column drops/renames will need a new sequence-numbered file.

### Slice 2 — Schema + table DDL (2026-05-09)

Eight `.sql` files: `Schema/001_CreateSchema.sql` (`CREATE SCHEMA IF NOT EXISTS "games"`), then 7 tables under `Tables/010_…` through `070_…`:

- `Categories` (Id uuid PK, Name text NOT NULL, Description text NOT NULL).
- `Links` (Id uuid PK, Value text, Description text, IsActive boolean, CategoryId uuid FK→Categories) + `IX_Links_CategoryId`.
- `LinkOutgoingLinks` (LinkId uuid, OutgoingLinkId uuid, composite PK; FK LinkId→Links ON DELETE CASCADE; FK OutgoingLinkId→Links no cascade — soft delete only).
- `Games` (Id uuid PK, PlayerId uuid, CurrentLinkId uuid, State varchar(32), CategoryId uuid, Difficulty varchar(32), StartLinkId uuid, TargetLinkId uuid, Score integer NULL, MaxSteps int, StepsTaken int, HintsRemaining/HintsUsed/UndosRemaining/UndosUsed/ResetsRemaining/ResetsUsed int).
- `GameHistory` (GameId uuid, StepNumber int, LinkId uuid, composite PK; FK GameId→Games ON DELETE CASCADE).
- `GameOptimalPath` (GameId uuid, Position int, LinkId uuid, composite PK; FK GameId→Games ON DELETE CASCADE).
- `OutboxMessages` (Id uuid PK, OccurredOn timestamp, Type text, Data text, ProcessedDate timestamp NULL) + `IX_OutboxMessages_ProcessedDate_OccurredOn` (anticipates Quartz processor query shape).

All identifiers double-quoted PascalCase. Schema name `games` lowercase. `IF NOT EXISTS` everywhere for re-run safety even though DbUp's journal makes each script run once per environment.

### Slice 1 — DbUp runner + Database project scaffold (2026-05-09)

- **`src/Database/LexiLink.DatabaseMigrator/`** — console app, `Microsoft.NET.Sdk` + `OutputType=Exe`, `net10.0`. Packages: `dbup-postgresql 6.1.2`, `Serilog 4.3.1`, `Serilog.Sinks.Console 6.0.0`. `Program.cs`:
  - Validates two args: `<connectionString> <scriptsDirectory>`. Returns -1 with usage hint on missing args or non-existent directory.
  - `EnsureDatabase.For.PostgresqlDatabase(connectionString)` creates the DB if missing.
  - `DeployChanges.To.PostgresqlDatabase(connectionString).WithScriptsFromFileSystem(scriptsDirectory, new FileSystemScriptOptions { IncludeSubDirectories = true }).JournalToPostgresqlTable("public", "MigrationsJournal").LogToConsole().Build()` — DbUp 6.x API.
  - Logs script count discovered, success, or failure (with inner exception). Returns 0 / -1.
- **`src/Database/LexiLink.Database/Structure/`** — bare folder layout: `games/{Schema,Tables,Views}/`. No `.csproj` — DbUp reads from the filesystem at runtime, so the directory is referenced by its path, not as a built artifact.
- **`LexiLink.sln`** — `LexiLink.DatabaseMigrator` added to solution.
- **Existing module-embedded scaffold deleted** — `src/Modules/Games/Infrastructure/Database/{GamesDatabaseInitializer.cs, Initialize/{001_InitialGames.sql, 002_GamesViews.sql}}` and the `<EmbeddedResource Include="Database\Initialize\*.sql" />` item from `Games.Infrastructure.csproj`. `dbup-postgresql 7.0.1` package ref also removed from that project. API `Program.cs` no longer auto-runs migrations on startup — schema deployment is the operator's job (or CI/CD's), exactly Kamil's split between the API host and the database migrator.
- Build: 0 error, 0 warning. Migrator runs successfully against a fresh local Postgres (verified in Slice 6).

---

## Sprint 4 — API Host (closed 2026-05-05)

### Slice 5 — Real appsettings + Scalar API reference (2026-05-05)

- **`Scalar.AspNetCore 2.14.10`** added to `LexiLink.API.csproj`. `app.MapScalarApiReference("/scalar")` mounted inside the `if (app.Environment.IsDevelopment())` block alongside `app.MapOpenApi()`. Scalar consumes the OpenAPI document generated by `AddOpenApi` at `/openapi/v1.json` — no extra wiring or schema config needed.
- **Connection string fail-fast guard**:
  - `appsettings.json` now ships `"LexiLinkDb": ""` (empty). Production deployments must set `ConnectionStrings__LexiLinkDb` env var or `appsettings.{env}.json`; otherwise startup throws.
  - `appsettings.Development.json` carries the localhost dev default (`Host=localhost;Database=lexilink;Username=postgres;Password=postgres`) so `dotnet run` in `Development` env Just Works.
  - `Program.cs` guard switched from `?? throw` to explicit `string.IsNullOrWhiteSpace` check so empty strings also fail fast. Exception message points the reader to both the env var and `appsettings.Development.json` so the recovery path is obvious.
- **Smoke test** —
  - `GET /scalar` → 302 redirect to `/scalar/` (Scalar's canonical path); `GET /scalar/` returns 200 with HTML titled "Scalar API Reference" referencing `/openapi/v1.json`.
  - `GET /openapi/v1.json` → 200 (still listing 17 paths / 22 method bindings).
  - `POST /categories { "name": "", "description": "" }` → still returns 422 from validation decorator (verifies the chain didn't regress).
  - `ASPNETCORE_ENVIRONMENT=Production dotnet run` → `System.InvalidOperationException: Connection string 'LexiLinkDb' is not configured. Set ConnectionStrings__LexiLinkDb env var or populate appsettings.Development.json.` (fails before binding the listener — exactly the desired behavior).
- Build clean (0 error, 0 warning). Sprint 4 closed.

### Slice 4 — FluentValidation `AbstractValidator<T>` + decorator chain rework (2026-05-05)

- **14 internal validator classes** alongside their command files. Each is `internal class XxxCommandValidator : AbstractValidator<XxxCommand>` with a public default ctor and `RuleFor(...)` chains:
  - Categories: `CreateCategoryCommandValidator` (Name NotEmpty + Max 100, Description Max 500), `EditCategoryCommandValidator` (CategoryId NotEmpty + name/description rules).
  - Links: `CreateLinkCommandValidator` (CategoryId NotEmpty, Value NotEmpty), `AddOutgoingLinkCommandValidator`, `RemoveOutgoingLinkCommandValidator`, `ActivateLinkCommandValidator`, `DeactivateLinkCommandValidator` (NotEmpty Guid checks).
  - Games: `CreateGameCommandValidator` (PlayerId/CategoryId NotEmpty + `Difficulty.IsInEnum()`), 6 single-Guid command validators (`StartGame`, `MakeStep` (with NextLinkId), `UseHint`, `Undo`, `Reset`, `AbandonGame`).
- **Length constants referenced from domain rules** — `CategoryNameMustNotExceedMaxLengthRule.MaxLength` (=100), `CategoryDescriptionMustNotExceedMaxLengthRule.MaxLength` (=500). Surface filter and deep filter stay in sync; if the domain rule's MaxLength changes, the validator follows automatically.
- **`FluentValidation 12.1.1`** package added to `Games.Application.csproj` (Games.Infrastructure decorators already had it).
- **Auto-discovery** — already wired in Slice 1: `RegisterAssemblyTypes(applicationAssembly).AsClosedTypesOf(typeof(IValidator<>)).AsImplementedInterfaces().InstancePerLifetimeScope()`. The `ValidationCommandHandlerDecorator` activates per command via the chain.
- **Decorator chain rework** — initial Slice 1 setup put UoW + Validation on `ICommandHandler<>` and only Logging on `IRequestHandler<>` (Kamil's apparent split). Diagnostic endpoint (`/_diag/chain`) revealed that `IRequestHandler<StartGameCommand>` correctly resolved to `LoggingCommandHandlerDecorator` but its `_decorated` field was the *bare handler*, not `Validation(UoW(handler))`. Root cause: Autofac fills a decorator's ctor parameter that matches the decorated service type using the previously-registered chain on **that service**; here Logging's `ICommandHandler<T>` parameter was satisfied by the previous `IRequestHandler<>` registration (the bare handler), cast at runtime to `ICommandHandler<T>` (the cast succeeds because each handler implements both). The separate `ICommandHandler<>` decorator chain was never reached. Fix: register **all three** decorators (UoW → Validation → Logging) on `IRequestHandler<>`/`IRequestHandler<,>` in innermost-first order. Every decorator implements `ICommandHandler<T>` → `IRequestHandler<T>` so each becomes the new outermost `IRequestHandler<>` registration in turn, and the ctor cast unifies the chain. Constraints (`where T : ICommand[<TResult>]`) prevent the chain from accidentally wrapping queries — query handlers continue to resolve as the bare `IRequestHandler<,>` registration.
- **Smoke test** — three POSTs that previously fell through to `CheckRule(...)` now return 422 from validation:
  - `POST /categories { name: "", description: "" }` → `422 { errors: ["'Name' must not be empty."] }`.
  - `POST /categories { name: "a"*150, description: "" }` → `422 { errors: ["The length of 'Name' must be 100 characters or fewer. You entered 150 characters."] }`.
  - `POST /games/00000000-.../start` → `422 { errors: ["'Game Id' must not be empty."] }`.
- Diagnostic endpoints (`/_diag/validators`, `/_diag/chain`) used for the investigation were removed before this writeup. Build clean (0 error, 0 warning).

### Slice 3 — Minimal API endpoints (2026-05-05)

- **Three endpoint files under `src/API/LexiLink.API/Modules/Games/`** — group-per-file pattern, each exposes `MapXxxEndpoints(this IEndpointRouteBuilder)` extension:
  - `CategoryEndpoints.cs` — 4 routes:
    - `POST /categories` body `CreateCategoryRequest(Name, Description)` → 201 with `{ id }`.
    - `PATCH /categories/{id:guid}` body `EditCategoryRequest(Name, Description)` → 204.
    - `GET /categories` → 200 `List<CategoryListItemDto>`.
    - `GET /categories/{id:guid}` → 200 `CategoryDetailsDto`.
  - `LinkEndpoints.cs` — 8 routes:
    - `POST /links` body `CreateLinkRequest(CategoryId, Value, Description, IsActive)` → 201 with `{ id }`.
    - `POST /links/{linkId:guid}/outgoing/{outgoingLinkId:guid}` → 204 (no body — both ids in path).
    - `DELETE /links/{linkId:guid}/outgoing/{outgoingLinkId:guid}` → 204.
    - `POST /links/{id:guid}/activate` → 204.
    - `POST /links/{id:guid}/deactivate` → 204.
    - `GET /links?categoryId={categoryId}` → 200 `List<LinkListItemDto>` (query-string filter).
    - `GET /links/{id:guid}` → 200 `LinkDetailsDto`.
    - `GET /links/{id:guid}/outgoing` → 200 `List<OutgoingLinkDto>`.
  - `GameEndpoints.cs` — 8 routes:
    - `POST /games` body `CreateGameRequest(PlayerId, CategoryId, Difficulty)` → 201 with `{ id }`.
    - `GET /games/{id:guid}` → 200 `GameDetailsDto`.
    - `POST /games/{id:guid}/start` → 204.
    - `POST /games/{id:guid}/steps` body `MakeStepRequest(NextLinkId)` → 204.
    - `POST /games/{id:guid}/hint` → 200 `HintResultDto`.
    - `POST /games/{id:guid}/undo` → 204.
    - `POST /games/{id:guid}/reset` → 204.
    - `POST /games/{id:guid}/abandon` → 204.
- **Pattern in each lambda** — inject `ISender` + `CancellationToken` (plus path/body args), construct the command/query record, await `sender.Send(...)`, return one of `Results.Created($"/{prefix}/{id}", new { id })` / `Results.Ok(dto)` / `Results.NoContent()`. No try/catch — exception middleware handles all error paths.
- **Request DTOs** — positional `record`s (`CreateCategoryRequest`, `EditCategoryRequest`, `CreateLinkRequest`, `CreateGameRequest`, `MakeStepRequest`) live in the same file as the endpoint group that uses them. Response DTOs reuse the existing `*Dto` records from `Games.Application` (`CategoryDetailsDto`, `LinkDetailsDto`, `GameDetailsDto`, etc.).
- **`MapGroup("/categories").WithTags("Categories")`** etc. for OpenAPI grouping. Path constraints (`{id:guid}`) on every Guid path param so non-Guid values 404 before reaching the handler.
- **`Program.cs` updates**: import `LexiLink.API.Modules.Games` namespace; register `JsonStringEnumConverter` globally via `ConfigureHttpJsonOptions` (so `Difficulty` enum accepts `"Easy"`/`"Medium"`/`"Hard"` strings); call `app.MapCategoryEndpoints()`, `app.MapLinkEndpoints()`, `app.MapGameEndpoints()` after the placeholder `GET /`.
- **Smoke test** — host on port 5099, `Development` env. `GET /openapi/v1.json` returns OpenAPI 3 doc listing **17 paths / 22 method bindings** (= 14 commands + 8 queries, matches the application surface). `GET /categories` returns `500 { "status": 500, "title": "Internal server error" }` — DB is unreachable so EF Core throws on connect; the unhandled-exception branch of `ExceptionHandlingMiddleware` catches and produces the canonical JSON shape, confirming the full middleware → MediatR → handler → repo → DbContext chain is wired end-to-end. Build clean (0 error, 0 warning).

### Slice 2 — Exception middleware + IExecutionContextAccessor (2026-05-05)

- **`Common.Application/IExecutionContextAccessor.cs`** — interface (Kamil-faithful): `Guid UserId`, `Guid CorrelationId`, `bool IsAvailable`. Lives in Common.Application so handlers/decorators can depend on it without referencing Infrastructure or API.
- **`API/Configuration/ExecutionContext/ExecutionContextAccessor.cs`** — HTTP-context impl. Reads `sub` claim for `UserId`; throws `ApplicationException` when no claim (matches Kamil — when auth is missing this is *expected* to be unavailable, callers must guard with `IsAvailable`). Reads `X-Correlation-ID` header for `CorrelationId`; throws if header missing or unparseable. `IsAvailable` returns `_httpContextAccessor.HttpContext != null`.
- **`API/Configuration/ExecutionContext/CorrelationMiddleware.cs`** — ensures `X-Correlation-ID` is always present on every request. If header missing or not a valid `Guid`, generates `Guid.NewGuid()` and writes it back into `Request.Headers`. Public `CorrelationHeaderKey` const referenced by `ExecutionContextAccessor`.
- **`API/Configuration/ExceptionHandling/ExceptionHandlingMiddleware.cs`** — single try/catch middleware mapping the three known exceptions to HTTP status + JSON body via `System.Text.Json`:
  - `BusinessRuleValidationException` → 400 `{ status, title: "Business rule violation", detail: ex.Details, rule: ex.BrokenRule.GetType().Name }`
  - `NotFoundException` → 404 `{ status, title: "Not Found", entityName, id }`
  - `InvalidCommandException` → 422 `{ status, title: "Invalid command", errors: [...] }`
  - Anything else → 500 `{ status, title: "Internal server error" }` + Serilog `Error` log.
- **Logging decorators retro-fitted** — both `LoggingCommandHandlerDecorator<T>` and `LoggingCommandHandlerWithResultDecorator<T, TResult>` now take `IExecutionContextAccessor` ctor param + push `RequestLogEnricher` along with `CommandLogEnricher` (Kamil order). The new `RequestLogEnricher.Enrich` checks `IsAvailable` first and try/catches `ApplicationException` so it silently no-ops in the current no-auth state — when auth ships, the enricher activates without code change. Slot for `IRecurringCommand` short-circuit remains deferred (matches Kamil's TODO until InternalCommands infra ships).
- **`Program.cs` wiring**:
  - `services.AddHttpContextAccessor()` + `services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>()` (flows into Autofac via `Populate`; decorators in Games.Infrastructure resolve it via constructor).
  - Middleware order — `app.UseMiddleware<ExceptionHandlingMiddleware>()` (outermost) → `app.UseMiddleware<CorrelationMiddleware>()` → `app.UseSerilogRequestLogging()` → endpoints. Exception handler outermost so it catches errors from any inner middleware too; correlation runs second so logs and exception responses can read the header.
- **Smoke test (temp endpoints, removed before commit)** — three throw-only endpoints exercised one-shot: `BusinessRuleValidationException` returned 400 with `rule: "TestRule"` + `detail`; `NotFoundException` returned 404 with `entityName: "TestEntity"` + `id`; `InvalidCommandException` returned 422 with `errors: ["err1","err2"]`. Correlation header echoed accepted with no error. Test endpoints + helper rule deleted after verification. Build clean (0 error).

### Slice 1 — Composition root foundation (2026-05-05)

- **`src/API/LexiLink.API/`** — new ASP.NET Core (`Microsoft.NET.Sdk.Web`, `net10.0`) project. Packages: `Autofac.Extensions.DependencyInjection 10.0.0`, `MediatR 14.1.0`, `Microsoft.AspNetCore.OpenApi 10.0.0`, `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1`, `Serilog.AspNetCore 9.0.0`. Project refs: `Common.Infrastructure`, `Modules/Games/Infrastructure` (transitively brings everything else). Solution updated.
- **`Program.cs`** — `WebApplication.CreateBuilder` → Serilog (`ReadFrom.Configuration` + console sink) → `UseSerilog` host + singleton `ILogger` for handler injection → `AutofacServiceProviderFactory` → `AddDbContext<GamesContext>` with `UseNpgsql(connectionString).ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>()` → `AddMediatR(cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly))` (registers MediatR infra only, no handler scan duplication) → `AddOpenApi()` scaffold → `ConfigureContainer<ContainerBuilder>(cb => cb.RegisterModule(new GamesAutofacModule()))`. Single placeholder endpoint `GET /` returning `"LexiLink API"`. `UseSerilogRequestLogging`.
- **`Modules/Games/Infrastructure/Configuration/GamesAutofacModule.cs`** — single composition module. Registers:
  - `GamesContext` (auto via `AddDbContext`) → resolves `DbContext` lambda for `UnitOfWork` (Common.Infrastructure-defined; depends on abstract `DbContext`).
  - `UnitOfWork`, `DomainEventsAccessor`, `DomainEventsDispatcher` — all `InstancePerLifetimeScope`. `DomainNotificationsMapper` with empty `BiDictionary<string, Type>` as `SingleInstance` (will be populated when `IDomainEventNotification<T>` wrappers ship). `InMemoryOutbox` as `IOutbox` (`SingleInstance`, no-op stub).
  - Repos: `CategoryRepository`, `LinkRepository`, `GameRepository` — all `InstancePerLifetimeScope` + `FindConstructorsWith(allCtors)`.
  - Domain services: `StandardScoreCalculator`, `StandardGameConfigurationService`, `PathFinderService` as `SingleInstance` (pure, threadsafe). `LinkNeighborResolver` as `InstancePerLifetimeScope` (depends on scoped `GamesContext`). `Random` as `SingleInstance`.
  - Handler assembly scan: `RegisterAssemblyTypes(applicationAssembly).AsClosedTypesOf(typeof(ICommandHandler<>)).AsImplementedInterfaces().InstancePerLifetimeScope().FindConstructorsWith(allCtors)`. Same for `ICommandHandler<,>`, `IQueryHandler<,>`, `IValidator<>`. `AsImplementedInterfaces` is what makes each handler resolvable as both `ICommandHandler<>` (decorator target) and `IRequestHandler<>` (MediatR target).
  - Decorator stack — order in registration is innermost-first:
    1. `RegisterGenericDecorator(UnitOfWorkCommandHandlerDecorator<>, ICommandHandler<>)`
    2. `RegisterGenericDecorator(UnitOfWorkCommandHandlerWithResultDecorator<,>, ICommandHandler<,>)`
    3. `RegisterGenericDecorator(ValidationCommandHandlerDecorator<>, ICommandHandler<>)`
    4. `RegisterGenericDecorator(ValidationCommandHandlerWithResultDecorator<,>, ICommandHandler<,>)`
    5. `RegisterGenericDecorator(LoggingCommandHandlerDecorator<>, IRequestHandler<>)` ← Kamil bridge
    6. `RegisterGenericDecorator(LoggingCommandHandlerWithResultDecorator<,>, IRequestHandler<,>)` ← Kamil bridge
    7. `RegisterGenericDecorator(DomainEventsDispatcherNotificationHandlerDecorator<>, INotificationHandler<>)`
- **`Common.Infrastructure/AllConstructorFinder.cs`** — `IConstructorFinder` for `internal` ctors. `BindingFlags.Instance | Public | NonPublic`. `ConcurrentDictionary<Type, ConstructorInfo[]>` cache. Throws `NoConstructorsFoundException(targetType, this)` (Autofac 9.x signature) if none found. `public` (cross-assembly use).
- **`Common.Infrastructure/Outbox/InMemoryOutbox.cs`** — `IOutbox` stub. `Add` appends to a `List<OutboxMessage>`; `GetMessages` returns read-only view. Real EF-backed outbox table lands with the trailing migration sprint.
- **`Modules/Games/Infrastructure/Assemblies.cs`** — `internal static class` with `Assemblies.Application = typeof(ICommand).Assembly`. Mirrors Kamil's per-module helper.
- **Smoke test** — `dotnet run` on port 5099. Host binds, container builds without errors, application enters running state, clean shutdown. Database is unreachable (no migrations yet, no Postgres running) but no DB query is exercised by the placeholder endpoint, so startup succeeds. Build: 0 error, 0 warning.

---

## Sprint 3 — Games.Infrastructure (closed 2026-05-04)

### Cross-category edge — write-time enforcement (2026-05-04)

- **`Link.CategoryId` public projection** — added to `Modules/Games/Domain/Links/Link.cs`: `public CategoryId CategoryId => _categoryId;`. Mirrors `OutgoingLinkIds` projection — exposes private field state for caller-side cross-aggregate validation.
- **`LinkOutgoingMustBeSameCategoryRule`** — new rule in `Modules/Games/Domain/Links/Rules/`. `IsBroken` when `_sourceCategoryId != _targetCategoryId`. Message: "Outgoing link must belong to the same category as the source link."
- **`Link.AddOutgoingLink` signature** — `(LinkId outgoingLinkId)` → `(LinkId outgoingLinkId, CategoryId outgoingCategoryId)`. Rule order: `LinkCannotPointToItselfRule` → `LinkOutgoingMustBeSameCategoryRule` → `LinkOutgoingAlreadyExistsRule`.
- **`AddOutgoingLinkCommandHandler` orchestration** — now loads both source and target Links via `ILinkRepository.GetByIdAsync`, throws `NotFoundException` for either missing, forwards `target.CategoryId` to the aggregate method. Cost: one extra `GetByIdAsync` per command.
- **DDD/Kamil rationale** — invariant belongs write-time because cross-category edges are semantically wrong, not a query convenience. Method-parameter style mirrors `Game.MakeStep(LinkId, ILinkNeighborResolver, IScoreCalculator)` (SKILLS rule #5). Domain service alternative (`ILinkCategoryChecker`) rejected as speculative abstraction.
- **Consequence** — `PathFinderService.FindOptimalPath` needs no category filter; the graph is category-clean by construction. Rule count 18 → 19; events unchanged.

### Domain services — implementations (slice 4, 2026-05-04)

- **`StandardGameConfigurationService : IGameConfigurationService`** — `Modules/Games/Domain/Services/`. `public sealed`. Hardcoded difficulty values via switch expressions; `_ => throw ArgumentOutOfRangeException` for unmapped enum values. Default values:
  - `Easy`: depth (3,5), maxSteps `target+5`, hints 3, undos 5, resets 2.
  - `Medium`: depth (5,7), maxSteps `target+4`, hints 2, undos 3, resets 1.
  - `Hard`: depth (7,10), maxSteps `target+3`, hints 1, undos 2, resets 1.
  These are placeholder values — to be tuned by playtesting. Sprint 4+ may introduce a `ConfigurableGameConfigurationService` (Infrastructure-side, reads from DB/file) following the `StandardScoreCalculator` → `Configurable*` precedent.
- **`PathFinderService : IPathFinderService`** — `Modules/Games/Domain/Services/`. `public sealed`. Pure BFS algorithm, depends only on `ILinkNeighborResolver` (DI-injected). Two methods:
  - `FindTarget(start, categoryLinkIds, minDepth, maxDepth)` — BFS from start, restricted to category-internal nodes; returns the first reachable node whose depth lies in `[minDepth, maxDepth]`. Deterministic (returns first-found by BFS traversal order); randomization comes from the random `startLinkId` chosen upstream in `Puzzle.Create`. `null` if no candidate exists — `PuzzleTargetLinkMustBeReachableRule` catches this domain-side.
  - `FindOptimalPath(start, target)` — BFS with parent tracking, reconstructs path back from target. Returns `[step1, step2, ..., target]` excluding start (so `Count = depth = number of steps`, matching `Puzzle.Depth` semantics used in the score formula). Empty list if unreachable. **No category filter** — assumes the graph is bounded by category at construction time; cross-category edges (which Link.AddOutgoingLink doesn't currently prevent) would let the optimal path "leak" out, but this is a known design hole flagged separately.
- **`LinkNeighborResolver : ILinkNeighborResolver`** — `Modules/Games/Infrastructure/Domain/Services/`. `internal`. Injects `GamesContext`. Implementation: synchronous EF Core query — `_gamesContext.Links.FirstOrDefault(x => x.Id == linkId)` (owned `_outgoingLinks` collection auto-loaded by EF), projects via the new `Link.OutgoingLinkIds` public property. Sync-over-EF is acceptable here because the interface signature is sync (defined for use inside `Game.MakeStep`), and the per-step query latency is non-critical compared to overall command throughput.
- **`Link.OutgoingLinkIds` public projection** — added to `Modules/Games/Domain/Links/Link.cs`: `IReadOnlyCollection<LinkId> OutgoingLinkIds => _outgoingLinks.Select(o => o.TargetId).ToList().AsReadOnly();`. Reasoning: outgoing topology is part of Link's aggregate identity (it's already the basis of three invariant rules) and exposing it publicly mirrors how `Game.History` projects from `_history`.

**Layering choice:** `PathFinderService` and `StandardGameConfigurationService` live in **Domain** (pure logic, no I/O); `LinkNeighborResolver` lives in **Infrastructure** (touches `GamesContext`). Same pattern as `StandardScoreCalculator` (Domain) — see `SKILLS.md` rule #8 / CONVENTIONS.md "Domain services" row.

**Wire-up deferred** — Sprint 4 Autofac composition root will register: `PathFinderService` and `StandardGameConfigurationService` as singletons (pure, threadsafe); `LinkNeighborResolver` as scoped (per-DbContext lifetime).

### `Modules/Games/Infrastructure/Domain/Games/` — Game aggregate (slice 3, 2026-05-04)

- `GameEntityTypeConfiguration : IEntityTypeConfiguration<Game>` — `internal`. Maps to table `Games` in schema `games`. Layout:
  - Scalars on Game: `PlayerId`, `_currentLinkId` → `CurrentLinkId`, `_gameState` → `State` (with `HasConversion<string>().HasMaxLength(32)` → `varchar(32)`).
  - `OwnsOne<Puzzle>("_puzzle")` flattened into `Games` row: `CategoryId`, `Difficulty` (`varchar(32)` enum-as-string), `StartLinkId`, `TargetLinkId`. **Nested** `OwnsMany<OptimalPathStep>("_optimalPath")` → table `GameOptimalPath` in schema `games`, FK `GameId`, payload `Position` + `LinkId`, composite PK `(GameId, Position)` (EF flattens nested-owned keys to the aggregate root).
  - `OwnsOne<Score>("_score")` with `Property(s => s.Points).HasColumnName("Score").IsRequired(false)` → single nullable column `Score` on `Games`. EF reads NULL row as `_score = null`.
  - `OwnsOne<StepBudget>("_stepBudget")` → `MaxSteps` + `StepsTaken`.
  - `OwnsOne<HintAllowance>` / `UndoAllowance` / `ResetAllowance` → `HintsRemaining`+`HintsUsed`, `UndosRemaining`+`UndosUsed`, `ResetsRemaining`+`ResetsUsed`. **The view computes `*Total = Remaining + Used`** — that arithmetic must live in the `[games].v_games` view (Sprint 3 last slice), not in EF mapping.
  - `OwnsMany<GameHistoryStep>("_history")` → table `GameHistory` in schema `games`, FK `GameId`, payload `StepNumber` + `LinkId`, composite PK `(GameId, StepNumber)`. Mirrors `[Games].[v_GameHistory]` columns referenced by `GetGameByIdQueryHandler`.
  - All owned navigations get `Navigation("_xxx").UsePropertyAccessMode(PropertyAccessMode.Field)` — eight in total.
- `GameRepository : IGameRepository` — `internal`, injects `GamesContext`. Two methods only: `GetByIdAsync(GameId, ct)` and `AddAsync(Game, ct)`. Owned types/collections auto-loaded by EF — no `Include`, no `AsNoTracking`. UnitOfWork decorator handles commit.

### Domain additions (driven by Game mapping)

- New VOs in `Modules/Games/Domain/Games/`:
  - `GameHistoryStep(int StepNumber, LinkId LinkId)` — `sealed : ValueObject`, `internal` ctor + EF parameterless ctor. Replaces the raw `LinkId` element type of `Game._history`.
  - `Puzzles/OptimalPathStep(int Position, LinkId LinkId)` — same shape, lives next to `Puzzle.cs`. Replaces `Puzzle._optimalPath` element type.
- `Game._startLinkId` and `_targetLinkId` field's removed; usages in `EvaluatePostStepTransitions`, `Undo`, `ResetToStart` now read from `_puzzle.StartLinkId` / `_puzzle.TargetLinkId`. Single source of truth — no more constructor-time copy.
- `Game._history` field type `List<LinkId>` → `List<GameHistoryStep>`. `MakeStep` wraps with `new GameHistoryStep(_history.Count + 1, nextLinkId)`. `Undo`'s `_history[^1]` becomes `_history[^1].LinkId`. `History` public projection now does `_history.Select(s => s.LinkId).ToList().AsReadOnly()` to preserve the existing `IReadOnlyCollection<LinkId>` contract.
- `Puzzle._optimalPath` field type `List<LinkId>` → `List<OptimalPathStep>`. Primary ctor now takes `IEnumerable<LinkId> optimalPath` and indexes them: `optimalPath.Select((id, i) => new OptimalPathStep(i, id)).ToList()`. `RequestHint` uses `FindIndex(s => s.LinkId == ...)` and `[idx + 1].LinkId`. `OptimalPath` public projection still returns `IReadOnlyList<LinkId>`.
- Six VOs gained `private SomeVo() { }` parameterless ctors for EF materialization: `Score`, `StepBudget`, `HintAllowance`, `UndoAllowance`, `ResetAllowance`, `Puzzle` (the last initializes `_optimalPath = []`). EF Core 10's parameterized-ctor binding is brittle for owned types with collection params — explicit parameterless ctor is reliable.
- `GameHistoryMustNotBeEmptyRule` signature: `List<LinkId>` → `IReadOnlyCollection<GameHistoryStep>`. `IsBroken` / `Message` unchanged.

### `Modules/Games/Infrastructure/Domain/Links/` — Link aggregate (slice 2, 2026-05-03)

- `LinkEntityTypeConfiguration : IEntityTypeConfiguration<Link>` — `internal`. Maps to table `Links` in schema `games`. `HasKey(x => x.Id)` (typed-ID auto-converted by selector). Backing fields `_value` (column `Value`), `_description` (column `Description`), `_isActive` (column `IsActive`), `_categoryId` (column `CategoryId`). Owned collection mapping for `_outgoingLinks` via `OwnsMany<OutgoingLink>("_outgoingLinks", ...)` → table `LinkOutgoingLinks` in schema `games`, FK column `LinkId`, payload column `OutgoingLinkId`, composite PK `(LinkId, OutgoingLinkId)`. `Navigation("_outgoingLinks").UsePropertyAccessMode(PropertyAccessMode.Field)` to be explicit about field access. Column names match `[Games].[LinkOutgoingLinks]` already referenced by `GetLinkOutgoingLinksQueryHandler`.
- `LinkRepository : ILinkRepository` — `internal`, injects `GamesContext`. Methods:
  - `GetByIdAsync` — `FirstOrDefaultAsync(x => x.Id == id)`; owned collection auto-loaded by EF, no explicit `Include`.
  - `GetIdsByCategoryAsync` — `EF.Property<CategoryId>(x, "_categoryId") == categoryId` projection to `Id`.
  - `GetActiveIdsByCategoryAsync` — adds `EF.Property<bool>(x, "_isActive")` filter.
  - `AddAsync` — `Links.AddAsync`. **No `Commit()` method** — UnitOfWorkCommandHandlerDecorator handles it.

### Domain additions (driven by Link mapping)

- New VO `OutgoingLink` (`Modules/Games/Domain/Links/OutgoingLink.cs`) — `sealed class : ValueObject`, single property `LinkId TargetId`. `internal` ctor + EF parameterless ctor. Wraps the foreign-Id reference so EF has an entity-shaped target for `OwnsMany`, and adds semantic clarity (it's an "outgoing edge", not a stray LinkId).
- `Link._outgoingLinks` field type `List<LinkId>` → `List<OutgoingLink>`. `AddOutgoingLink` wraps; `RemoveOutgoingLink` `RemoveAll(o => o.TargetId == ...)`. Domain event payloads (`OutgoingLinkAddedDomainEvent`, `OutgoingLinkRemovedDomainEvent`) unchanged — still carry `LinkId`.
- Two rules updated to `IReadOnlyCollection<OutgoingLink>` and `.Any(o => o.TargetId == ...)`: `LinkOutgoingAlreadyExistsRule`, `LinkOutgoingMustExistRule`.

### `Common.Infrastructure` — EF Core building blocks (Kamil-faithful)

- `IUnitOfWork` moved from `Common.Domain` → `Common.Infrastructure` to match Kamil's BuildingBlocks/Infrastructure placement. Signature unchanged for now (`Task<int> CommitAsync(CancellationToken ct = default)`); the optional `Guid? internalCommandId` parameter Kamil has is deferred until InternalCommands infra ships (Beyond Sprint 5). Two consumers updated: `UnitOfWorkCommandHandlerDecorator<T>` and `UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>` `using` lines now point to `LexiLink.Common.Infrastructure`.
- `UnitOfWork : IUnitOfWork` — injects generic `DbContext` + `IDomainEventsDispatcher`; `CommitAsync` first dispatches domain events, then `_context.SaveChangesAsync(ct)`. Birebir Kamil. Composition root binds the abstract `DbContext` to the per-module concrete (`GamesContext`).
- `TypedIdValueConverter<TTypedIdValue> : ValueConverter<TTypedIdValue, Guid>` — Andrew Lock's strongly-typed-ID pattern; `id => id.Value` for serialize, `value => Activator.CreateInstance(typeof(TTypedIdValue), value)` for deserialize.
- `StronglyTypedIdValueConverterSelector : ValueConverterSelector` — auto-applies `TypedIdValueConverter<>` to any property whose CLR type derives from `TypedIdValueBase`. `ConcurrentDictionary` cache.

### `Modules/Games/Infrastructure/` — DbContext + first aggregate

- `GamesContext : DbContext` at the module Infrastructure root. `DbSet<Category>`, `DbSet<Link>`, `DbSet<Game>`. Ctor takes `(DbContextOptions, ILoggerFactory)` (matches Kamil's `MeetingsContext` signature). `OnModelCreating` does `ApplyConfigurationsFromAssembly(GetType().Assembly)` — entity configurations auto-scanned from the assembly.
- `Domain/Categories/CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>` — `internal`. Maps to table `Categories` in schema `games` (lowercase per Postgres convention). `HasKey(x => x.Id)` (typed-ID auto-converted by selector). Backing fields `_name` (column `Name`) and `_description` (column `Description`).
- `Domain/Categories/CategoryRepository : ICategoryRepository` — `internal`, injects `GamesContext` (concrete, not generic `DbContext`). Methods: `GetByIdAsync`, `AddAsync`. **No `Commit()` method** — the UnitOfWorkCommandHandlerDecorator in the pipeline calls `IUnitOfWork.CommitAsync` after the handler runs.
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1` added to `LexiLink.Modules.Games.Infrastructure.csproj`.

---

## Sprint 2 — Application Layer (closed 2026-05-03)

### `Common.Infrastructure` — Domain event dispatch

- `DomainEventsAccessor` — pulls domain events from EF Core `ChangeTracker` (`Entries<Entity>()`), returns flattened `IReadOnlyCollection<IDomainEvent>`; `ClearAllDomainEvents` resets per-aggregate buckets after dispatch.
- `DomainEventsDispatcher` — Autofac scope resolves an `IDomainEventNotification<>` per concrete event type, MediatR publishes the events in-process, then notifications are JSON-serialized (`AllPropertiesContractResolver`) into `OutboxMessage`s.
- `DomainEventsDispatcherNotificationHandlerDecorator<T>` — wraps every `INotificationHandler<T>` so cascade events from a handler also get dispatched.
- `DomainNotificationsMapper` + `BiDictionary<string, Type>` — bidirectional map between outbox `Type` strings and runtime notification types.

### `Modules/Games/Infrastructure/Configuration/Processing/` — Command-handler decorators (Kamil-style)

- `UnitOfWorkCommandHandlerDecorator<T>` — void variant; commits `IUnitOfWork` after the inner `ICommandHandler<T>` returns. `where T : ICommand`.
- `UnitOfWorkCommandHandlerWithResultDecorator<T, TResult>` — result variant for `CreateGame : CommandBase<Guid>`, `UseHint : CommandBase<HintResultDto>`. `where T : ICommand<TResult>`.
- `LoggingCommandHandlerDecorator<T>` + `LoggingCommandHandlerWithResultDecorator<T, TResult>` — Serilog-based, Kamil-faithful: `LogContext.Push(new CommandLogEnricher(command))` adds `"Context" = "Command:{Id}"` to every log line; try/catch logs `Information` on success and `Error` on exception (then rethrows). Result variant uses `{@Command}` destructuring for the entry log and logs the result on success.
  - Deliberately **omitted vs Kamil**: `RequestLogEnricher` + `IExecutionContextAccessor` ctor parameter (no HTTP host yet) and `if (command is IRecurringCommand) { … }` short-circuit (no InternalCommands infra yet). Both pluggable later without restructuring.
- `ValidationCommandHandlerDecorator<T>` + `ValidationCommandHandlerWithResultDecorator<T, TResult>` — FluentValidation, Kamil-faithful: ctor takes `IList<IValidator<T>>`, runs `.Validate(command)` per validator, flattens `Errors`, throws `InvalidCommandException(List<string> Errors)` if any error surfaces; otherwise delegates to inner handler. Result variant returns the inner `Task<TResult>` directly (no `await`) — matches Kamil byte-for-byte.
- All `internal` (Autofac decorator pattern; only the container constructs them).
- Generic constraint targets Games' own `ICommand` (from `Games.Application.Contracts`), not MediatR's `IRequest` directly — matches Kamil's per-module Processing/ layout exactly.
- Packages added to `LexiLink.Modules.Games.Infrastructure.csproj`: `Serilog 4.3.1`, `FluentValidation 12.1.1`.

### `Common.Application/Exceptions/`

- `InvalidCommandException : Exception` — carries `List<string> Errors`; thrown by validation decorators. Sibling of `NotFoundException`. Will map to HTTP 422 in Sprint 4 exception middleware.

### `Common.Application`

- `NotFoundException` (`Common/Application/Exceptions/`) — carries `EntityName` + `Id`. Thrown from handlers when a `GetByIdAsync` or `QuerySingleOrDefaultAsync` returns `null`. Will map to HTTP 404 in Sprint 4.

### `Games.Application` — Configuration

- `Configuration/Commands/` — `ICommand`, `ICommand<TResult>`, `ICommandHandler<>`, `ICommandHandler<,>`, `CommandBase`, `CommandBase<TResult>`. (Per-module by design — see `SKILLS.md` rule #10.)
- `Configuration/Queries/` — `IQuery<TResult>`, `IQueryHandler<,>`, `QueryBase<TResult>` (carries `Id` for pipeline correlation).
- `Contracts/` — module-facing types referenced by command/query bases.

### `Games.Application` — Links

- Commands (5):
  - `CreateLinkCommand` / `CreateLinkCommandHandler` — inserts a new Link with category check.
  - `AddOutgoingLinkCommand` / `AddOutgoingLinkCommandHandler`.
  - `RemoveOutgoingLinkCommand` / `RemoveOutgoingLinkCommandHandler`.
  - `ActivateLinkCommand` / `ActivateLinkCommandHandler`.
  - `DeactivateLinkCommand` / `DeactivateLinkCommandHandler`.
- Queries (3):
  - `GetLinkDetailsQuery` → `LinkDetailsDto(Id, CategoryId, Value, Description, IsActive)`.
  - `GetLinksByCategoryQuery` → `List<LinkListItemDto(Id, Value, IsActive)>`.
  - `GetLinkOutgoingLinksQuery` → `List<OutgoingLinkDto(Id, Value, IsActive)>` via JOIN through the `LinkOutgoingLinks` join table.
- All handlers `internal`; all DTOs positional `record`s; SQL via raw string literals against `[Games].[v_Links]`.

### `Games.Application` — Games

- Commands (7):
  - `CreateGameCommand : CommandBase<Guid>` / `CreateGameCommandHandler` — verifies Category, fetches active Link ids, constructs `Puzzle.Create(...)` then `Game.Create(...)`, persists via `IGameRepository.AddAsync`. Handler injects `ICategoryRepository`, `ILinkRepository`, `IGameRepository`, `IPathFinderService`, `IGameConfigurationService`, and `Random`.
  - `StartGameCommand : CommandBase` / `StartGameCommandHandler` — loads Game (404 if missing) and calls `game.Start()`.
  - `MakeStepCommand : CommandBase` / `MakeStepCommandHandler` — loads Game and calls `game.MakeStep(LinkId, ILinkNeighborResolver, IScoreCalculator)`. The two domain services are injected and forwarded as method parameters (per SKILLS.md rule #5).
  - `UseHintCommand : CommandBase<HintResultDto>` / `UseHintCommandHandler` — loads Game, calls `game.UseHint()` (now returns `HintResult`), projects to `HintResultDto(HintType, Guid)`.
  - `UndoCommand : CommandBase` / `UndoCommandHandler` — loads Game and calls `game.Undo()`.
  - `ResetCommand : CommandBase` / `ResetCommandHandler` — loads Game and calls `game.ResetToStart()`.
  - `AbandonGameCommand : CommandBase` / `AbandonGameCommandHandler` — loads Game and calls `game.Abandon()`.
- Queries (1):
  - `GetGameByIdQuery` → `GameDetailsDto` — `QueryMultipleAsync` against `[Games].[v_Games]` JOIN `[Games].[v_Links]` (3× alias for Start/Target/Current word denormalization) + `[Games].[v_GameHistory]` JOIN `[Games].[v_Links]` for the step list. DTO is positional record with one `init` `History` slot for the nested collection — handler does `dto with { History = history }`.
- DTOs:
  - `HintResultDto(HintType Type, Guid RecommendedLinkId)`.
  - `GameDetailsDto` — flat fields (Id, PlayerId, CategoryId, Difficulty, Start/Target/Current Link+Word, State, Score?, MaxSteps, StepsTaken, Hints/Undos/Resets Total+Used) + `IReadOnlyList<GameHistoryStepDto> History`.
  - `GameHistoryStepDto(int StepNumber, Guid LinkId, string LinkValue)`.

### `Games.Application` — Categories

- Commands (2):
  - `CreateCategoryCommand : CommandBase<Guid>` / `CreateCategoryCommandHandler`.
  - `EditCategoryCommand : CommandBase` / `EditCategoryCommandHandler` — calls `category.EditGeneralInfo(...)`.
- Queries (2):
  - `GetCategoryDetailsQuery` → `CategoryDetailsDto(Id, Name, Description, LinkCount)` — `LinkCount` via correlated subquery.
  - `GetCategoriesQuery` → `List<CategoryListItemDto(Id, Name)>` ordered by `Name`.

### Domain additions (during Sprint 2)

- `Link.Activate()` / `Link.Deactivate()` methods.
- Rules: `LinkMustBeInactiveToActivateRule`, `LinkMustBeActiveToDeactivateRule`.
- Events: `LinkActivatedDomainEvent`, `LinkDeactivatedDomainEvent`.
- (Consequence: domain event count rose from 15 to 17; rule count from 16 to 18.)
- `ILinkRepository.GetActiveIdsByCategoryAsync(CategoryId, ...)` — added for `CreateGameCommand` puzzle generation; the existing `GetIdsByCategoryAsync` returns all link ids regardless of `IsActive`, so this sibling method makes the active-only intent explicit at the call site.
- `Game.UseHint()` return type changed from `void` to `HintResult` — handler needs the value for `HintResultDto` projection. The `_puzzle.RequestHint(...)` value was already produced inside the method and passed to `HintUsedDomainEvent`; we just stop discarding it. Event payload unchanged.

---

## Sprint 1 — Domain Hardening (closed 2026-05-02)

### BuildingBlocks (`Common/Domain`)

- `Entity` (domain event collection + `CheckRule(...)`).
- `Entity<TId>` (Id-based equality + operator overloads).
- `ValueObject` (reflection-cached equality with `IgnoreMemberAttribute` opt-out).
- `IBusinessRule`, `BusinessRuleValidationException`.
- `IDomainEvent : INotification`, `DomainEvent` base (`Id` + `OccurredOn`).
- `IAggregateRoot` marker, `IRepository<T> where T : IAggregateRoot`, `IUnitOfWork`.
- `TypedIdValueBase` rejecting `Guid.Empty`.

### Games module — Domain

- **3 aggregates** with private parameterless ctor + private primary ctor + `internal static Create` factory pattern.
  - `Category` — name (≤ 100, non-empty) + description (≤ 500). 3 invariant rules. 2 events.
  - `Link` — value, description, `_categoryId`, `_outgoingLinks`, `_isActive`. Content immutable. Self-loop, duplicate, and missing-edge rules on outgoing topology. 3 events.
  - `Game` — full state machine (`Initial → InProgress → LastStepWarning → Completed/Failed/Abandoned`); commands `Start`, `MakeStep`, `UseHint`, `Undo`, `ResetToStart`, `Abandon`; private `EvaluatePostStepTransitions` for post-step state evaluation; private `Complete(IScoreCalculator)` and `Fail()`. 8 game-level rules (allowance rules nested in VOs). 10 events.
- **Puzzle** moved from separate aggregate → `sealed class Puzzle : ValueObject` inside Game. Domain services (`IPathFinderService`, `IGameConfigurationService`, `Random`) received as method parameters of `Puzzle.Create` — never stored. `ChooseTargetLink` private helper isolates the one `!` null-forgiving operator inside `Create`.
- **Allowance VO family** — `HintAllowance`, `UndoAllowance`, `ResetAllowance` each with `Of(int total)` factory + `Consume()` returning new instance after `CheckRule(*MustHaveRemainingRule)`. Three separate VOs (not generic), so types stay semantic.
- **`StepBudget` VO** — replaces the loose `_maxSteps` int + `history.Count` arithmetic that was previously scattered across Game.cs. Encapsulates `Step()`, `UndoStep()`, `Reset()`, and the `IsExhausted` / `IsAtLastWarning` / `IsBelowLastWarning` predicates.
- **Score VO + `IScoreCalculator`** — `Score` is a pure VO (`Points` + `Of(int)`); formula extracted to `IScoreCalculator`. Default `StandardScoreCalculator` lives in Domain (pure logic). Difficulty multiplier: Easy=1.00, Medium=1.15, Hard=1.25.
- **Repository contracts in Domain** — `ICategoryRepository`, `ILinkRepository`, `IGameRepository`. Implementations deferred to Sprint 3.

### Bug fixes / cleanup during Sprint 1

- `GameMustBeNotStartedRule` — inverted-logic bug fixed.
- `Score.CalculateScore` — `targetDepth = stepsTaken` bug fixed (now `_puzzle.Depth`).
- Old `_remaining{Hints,Undos,Resets}` + `_used` int pairs removed; replaced by allowance VOs.
- Old `GameMustHaveRemaining*Rule` files deleted (rules moved into the VOs).
- Old `Categories/Rules/` and `Puzzles/` folders cleaned up.
- `Link._outgoinglinks` renamed to `_outgoingLinks` (lowercase 'l' typo).

---

## See Also

- `activeContext.md` — what's currently in flight.
- `ROADMAP.md` — what's planned next.
