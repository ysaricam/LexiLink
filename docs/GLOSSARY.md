# GLOSSARY.md

Ubiquitous language for the LexiLink Games, Players, Energy, Quests,
Hint, Undo, and Reset modules. Every term below appears in code as a
class, interface, enum, or namespace; this file gives it a
one-paragraph definition so a new contributor can read the code
without spelunking.

---

## Aggregates

### Category — `Modules/Games/Domain/Categories/Category.cs`
A named set of words from which a game can be seeded (e.g., *animals*, *colors*). Aggregate root. Holds `_name` (≤ 100 chars, non-empty) and an optional `_description` (≤ 500 chars). Lifecycle: created via `Category.Create(name, description)`; renamed via `EditGeneralInfo(...)`. Emits `CategoryCreatedDomainEvent` and `CategoryEditedDomainEvent`.

### Link — `Modules/Games/Domain/Links/Link.cs`
A word node in the directed word-graph. Aggregate root. Holds `_categoryId` (the Category the word belongs to), `_value` (the word itself), `_description`, `_isActive`, and `_outgoingLinks` (a list of `LinkId`s pointing to neighbor words). Content (`_value`, `_description`) is **immutable** — to change a word's text, deactivate the old Link and create a new one. Outgoing topology *is* mutable through `AddOutgoingLink` / `RemoveOutgoingLink` (graph-relationship management). Activation flips through `Activate()` / `Deactivate()`.

### Game — `Modules/Games/Domain/Games/Game.cs`
A single player's session solving one Puzzle. Aggregate root. The most
complex aggregate in the module. Owns a `Puzzle` value object,
`HintAllowance`, optional `Score`, a `_history` of `LinkId`s walked, a
`StepBudget`, a `GameState`, and plain Undo/Reset usage counters.
Sprint UR1 removed `UndoAllowance` and `ResetAllowance`; every Undo or
Reset now goes through `IUndoGuard` / `IResetGuard` before Game mutates.
Drives an explicit state machine — see *Game States* below.

### PlayerEnergy — `Modules/Energy/Domain/PlayerEnergies/PlayerEnergy.cs`
A player's energy account in the Energy module. Aggregate root, identified by `PlayerEnergyId` (same Guid value as the owning `PlayerId` — cross-module reference by id only). Holds `_currentAmount`, `_maximumAmount`, `_rechargeIntervalSeconds`, and `_lastRefilledOn`. Lifecycle: `InitializeFor(playerId, max, intervalSeconds, initializedAt)` (called by `EnsurePlayerEnergyExistsCommand` after a `PlayerRegisteredIntegrationEvent`). Behavior: `Consume(amount, now)` recharges first via `RechargeBasedOnElapsedTime(now)`, then checks `EnergyMustBeSufficientToConsumeRule`, then debits; the refill timer is armed **only when consume crosses the bucket from at/above max to below max** (so 10/5 → 9/5 and 6/5 → 5/5 leave the timer idle, while 5/5 → 4/5 sets it). `GrantBonus(amount, now)` adds `amount` without checking max — it is the reward path invoked by `QuestClaimedIntegrationEvent`; over-max balance is preserved and the timer stays idle until the bucket drains back below max. Emits `PlayerEnergyConsumedDomainEvent` and `PlayerEnergyRefilledDomainEvent`.

### PlayerQuest — `Modules/Quests/Domain/PlayerQuests/PlayerQuest.cs`
A single quest assignment to a single player. Aggregate root, identified by `PlayerQuestId` (its own Guid, unrelated to `PlayerId`). Holds `_playerId`, `_questDefinitionId`, `_progressBaselineSnapshot`, `_state`, `_issuedAt`, `_claimedAt`, and `_expiresAt`. Post Sprint Q1 progress is **computed at read time** from Stats counters; the aggregate doesn't persist `Progress`/`Goal`/rewards — those live on the linked `QuestDefinition`. Lifecycle: `IssueFor(playerId, questDefinitionId, baselineSnapshot, issuedAt, expiresAt?)` (creates in `Active` state). Behavior (Sprint H reshape): `Claim(now, isReadyToClaim, energyReward, hintReward)` — the handler passes `isReadyToClaim` (computed from current Stats counter minus baseline ≥ definition's threshold, and not past `_expiresAt`) and the catalog's `(energyReward, hintReward)` pair; aggregate flips `Active → Claimed` and emits `PlayerQuestClaimedDomainEvent` carrying both rewards so Energy's and Hint's outbox consumers can each grant their bonus event-driven (each consumer guards on its reward's positivity). Expired daily rows are DELETEd by `GetActiveQuestsQueryHandler` on the next sync, not transitioned. Emits `PlayerQuestIssuedDomainEvent`, `PlayerQuestClaimedDomainEvent`.

### QuestDefinition — `Modules/Quests/Domain/PlayerQuests/QuestDefinition.cs`
Catalog entry describing how a quest is issued and rewarded. Aggregate root identified by `QuestDefinitionId`. Holds `_name` (≤64 chars), `_description` (≤256 chars), `_trigger`, `_threshold`, `_energyReward`, `_hintReward`, `_prerequisiteQuestDefinitionId?` (FK to another definition or null), `_progressBaseline`, `_isActive`. Sprint H reshape: the single `_reward` was split into the `(_energyReward, _hintReward)` pair so a quest can deliver either or both — `QuestRewardMustHaveAtLeastOnePositiveRule` enforces ≥1 positive. Lifecycle: `Create(name, description, trigger, threshold, energyReward, hintReward, prereqId?, progressBaseline, prerequisiteWouldCreateCycle)` — handler walks the prereq chain ahead of time and passes the boolean. `Update(description, threshold, energyReward, hintReward, prereqId?, progressBaseline, prerequisiteWouldCreateCycle)` — Name and Trigger are immutable post-create (changing them would re-key PlayerQuest history). `Deactivate` / `Reactivate` idempotent; deactivated definitions disappear from `/quests/me` listings (claim history rows stay intact). Emits `QuestDefinitionCreatedDomainEvent`, `QuestDefinitionUpdatedDomainEvent`, `QuestDefinitionActivationChangedDomainEvent`.

### PlayerHintInventory — `Modules/Hint/Domain/PlayerHintInventories/PlayerHintInventory.cs`
A player's persistent hint account in the Hint module. Aggregate root, identified by `PlayerHintInventoryId` (same Guid value as the owning `PlayerId` — cross-module reference by id only). Sprint H ships this as Energy's stripped-down sibling: a single `int _balance`, no maximum cap, no refill timer (hints are earned, not regenerated). Lifecycle: `InitializeFor(playerId, initialBalance)` (called by `EnsurePlayerHintInventoryExistsCommand` after a `PlayerRegisteredIntegrationEvent` — `Hint:InitialBalance` config drives the seed; default 0). Behavior: `Consume(amount, now)` — checks `HintAmountMustBePositiveRule` then `HintBalanceMustBeSufficientRule`, decrements, emits `PlayerHintConsumedDomainEvent`. `GrantBonus(amount, now)` — bonus reward path; adds without checking max (over-cap balance is *intentional* and the parallel to `PlayerEnergy.GrantBonus`). Invoked from two places: `Hint.Application/QuestClaimedIntegrationEventHandler` when a claimed quest carries `HintReward > 0`, and `GrantBonusHintCommand` for admin grants. `AdminSet(newBalance, now)` snaps to exact value (must be ≥0); `AdminReset(now)` snaps to zero. The per-game free hint quota (1 charge across all difficulties) lives on the `Game` aggregate's `HintAllowance` and is consumed first — see `Game.HasFreeHintRemaining` + `UseHintWithExternalInventory()`. Emits `PlayerHintInventoryInitializedDomainEvent`, `PlayerHintConsumedDomainEvent`, `PlayerHintGrantedDomainEvent`, `PlayerHintAdminSetDomainEvent`, `PlayerHintAdminResetDomainEvent`.

### PlayerUndoInventory — `Modules/Undo/Domain/PlayerUndoInventories/PlayerUndoInventory.cs`
A player's persistent undo account in the Undo module. Aggregate root,
identified by `PlayerUndoInventoryId` (same Guid value as the owning
`PlayerId` — cross-module reference by id only). Sprint UR2 ships this
as a Hint-style inventory: a single `int _balance`, no maximum cap, no
refill timer. Lifecycle: `InitializeFor(playerId, initialBalance)`;
Sprint UR3 wires that lifecycle to `PlayerRegisteredIntegrationEvent`
via `EnsurePlayerUndoInventoryExistsCommand` and an idempotent
integration-event handler.
Behavior: `Consume(amount, now)` checks
`UndoAmountMustBePositiveRule` and `UndoBalanceMustBeSufficientRule`,
then decrements. `GrantBonus(amount, now)` adds without checking max.
`AdminSet(newBalance, now)` snaps to an exact non-negative value and
`AdminReset(now)` snaps to zero. Games has no per-game free undo quota
after UR1; every in-game undo consumes this inventory through the UR4
`IUndoGuard` adapter (`ConsumePlayerUndoCommand(playerId, 1)`). Emits
`PlayerUndoInventoryInitializedDomainEvent`,
`PlayerUndoConsumedDomainEvent`, `PlayerUndoGrantedDomainEvent`,
`PlayerUndoAdminSetDomainEvent`, `PlayerUndoAdminResetDomainEvent`.

### PlayerResetInventory — `Modules/Reset/Domain/PlayerResetInventories/PlayerResetInventory.cs`
A player's persistent reset account in the Reset module. Aggregate
root, identified by `PlayerResetInventoryId` (same Guid value as the
owning `PlayerId` — cross-module reference by id only). Sprint UR2
ships this as a Hint-style inventory: a single `int _balance`, no
maximum cap, no refill timer. Lifecycle:
`InitializeFor(playerId, initialBalance)`; Sprint UR3 wires that
lifecycle to `PlayerRegisteredIntegrationEvent` via
`EnsurePlayerResetInventoryExistsCommand` and an idempotent
integration-event handler. Behavior: `Consume(amount, now)`
checks `ResetAmountMustBePositiveRule` and
`ResetBalanceMustBeSufficientRule`, then decrements.
`GrantBonus(amount, now)` adds without checking max.
`AdminSet(newBalance, now)` snaps to an exact non-negative value and
`AdminReset(now)` snaps to zero. Games has no per-game free reset quota
after UR1; every in-game reset consumes this inventory through the UR4
`IResetGuard` adapter (`ConsumePlayerResetCommand(playerId, 1)`). Emits
`PlayerResetInventoryInitializedDomainEvent`,
`PlayerResetConsumedDomainEvent`, `PlayerResetGrantedDomainEvent`,
`PlayerResetAdminSetDomainEvent`, `PlayerResetAdminResetDomainEvent`.

---

## Value Objects

### Puzzle — `Games/Puzzles/Puzzle.cs`
The board: a category, a difficulty, a start `LinkId`, a target `LinkId`, and the optimal path between them (computed once at creation by `IPathFinderService`). Sealed VO inside `Game`. Created via `Puzzle.Create(...)` which receives `IPathFinderService`, `IGameConfigurationService`, and a `Random` — never stores them. Exposes `RequestHint(currentLinkId)` returning a `HintResult`.

### HintResult — `Games/Puzzles/HintResult.cs`
What a player gets when they spend a hint: a flag indicating whether their current position is on the optimal path, plus the next correct link if so.

### Score — `Games/Score.cs`
The points a Game ends with. Pure VO with a single `Points` getter and a `Score.Of(int)` factory. Computed by `IScoreCalculator` at completion — `Score` itself does no arithmetic.

### Difficulty — `Games/Difficulty.cs`
Enum: `Easy`, `Medium`, `Hard`. Drives `IGameConfigurationService.ResolveDepthRange`, the score multiplier (Easy=1.00, Medium=1.15, Hard=1.25), and step/allowance budgets.

### GameState — `Games/GameState.cs`
Enum: `Initial`, `InProgress`, `LastStepWarning`, `Completed`, `Failed`, `Abandoned`. Drives the state machine — see below.

### QuestTrigger — `Quests/PlayerQuests/QuestTrigger.cs`
Enum: `GameCompletedTotal`, `GameCompletedDaily`, `AuthProviderLinked`. Tells `IQuestCounterReader` which player counter the quest tracks. Fixed at three values — extending requires a Domain change AND a matching counter read in the API host adapter. Persisted to `quests.QuestDefinitions.Trigger` as `varchar(32)` via EF `HasConversion<string>()`.

### ProgressBaseline — `Quests/PlayerQuests/ProgressBaseline.cs`
Enum: `FromSnapshot` (default — capture player's counter at issuance, measure delta from there) or `FromExistingTotal` (snapshot = 0, measure absolute counter — for retroactive milestones like "you've completed 50 games" that should reward longtime players on first sync). Only meaningful for `GameCompletedTotal`; Daily and AuthProviderLinked ignore the value because their counters already start at the relevant zero.

### QuestState — `Quests/PlayerQuests/QuestState.cs`
Enum: `Active`, `Claimed`. Persisted state shrunk in Sprint Q1 — `ReadyToClaim` is computed at read time (`counter - baseline ≥ threshold`) and `Expired` was replaced by row deletion in the sync pass. Persisted as `varchar(32)` via `HasConversion<string>()`. The API's `PlayerQuestDto.DisplayState` carries the read-time projection (one of "Active" / "ReadyToClaim" / "Claimed") as a string.

### QuestCounters — `Quests/Application/Configuration/CrossModule/IQuestCounterReader.cs`
Record `QuestCounters(int GamesCompletedTotal, int GamesCompletedToday, bool AuthProviderLinked)`. Returned by `IQuestCounterReader.ReadAsync(playerId, nowUtc)` in a single call. Owned by Stats (Total/Daily) and Players (AuthLinked); read through the sync gateway whose implementation lives in `LexiLink.API/CrossModule/QuestCounterReader.cs`.

### HintAllowance + Undo/Reset usage counters — `Games/`
`HintAllowance` remains an immutable VO with `Of(int total)`, `Remaining`, `Used`, and `Consume()`; Game uses field reassignment: `_hintAllowance = _hintAllowance.Consume();`. Sprint H decision: `HintAllowance.Of(1)` for every game regardless of difficulty (`IGameConfigurationService.ResolveHints` returns 1) — additional charges beyond the free quota come from the player's `PlayerHintInventory` via the `IHintGuard` sync gateway. `Game.HintsUsed` (in `GameDetailsDto`) tracks **only** the free quota; inventory consumption does not advance it. Sprint UR1 removed `UndoAllowance` and `ResetAllowance`; Undo/Reset now keep only `_undosUsed` + `_resetsUsed` counters in Game, while the persistent player inventories will live in the new Undo and Reset modules.

### StepBudget — `Games/StepBudget.cs`
Encapsulates `Max` + `Taken` plus the `Step()`, `UndoStep()`, `Reset()` methods and the `IsExhausted` / `IsAtLastWarning` / `IsBelowLastWarning` predicates. Replaced loose `_maxSteps` int + `history.Count` arithmetic that used to live across `Game.cs`.

### EnergyRefillCalculator — `Modules/Energy/Domain/PlayerEnergies/EnergyRefillProjection.cs`
`internal static` pure-math helper. `Project(current, max, lastRefilledOn, intervalSeconds, now)` returns an `EnergyRefillProjection(currentAmount, lastRefilledOn)` after applying time-elapsed refills. Capped at max; partial intervals are preserved (the leftover seconds carry into the next refill). Used by both `PlayerEnergy.RechargeBasedOnElapsedTime` (write path) and `GetPlayerEnergyQueryHandler` (read path, via `InternalsVisibleTo`) so the math has one source of truth.

---

## Game States — transitions

```
                     Start()
        Initial ──────────────► InProgress
                                    │
                  history.Count == max-1
                                    ▼
                              LastStepWarning ── Undo() (if budget recovers) ──► InProgress
                                    │
                          target reached       step taken at limit
                                    ▼                  ▼
                              Completed             Failed

        InProgress / LastStepWarning ── Abandon() ──► Abandoned
```

Every transition is guarded by `CheckRule(...)` and emits a `*DomainEvent`. `Initial → InProgress` requires `GameMustBeNotStartedRule`; most other transitions require `GameMustBeInProgressRule`.

---

## Domain Events

### Game (10) — `Games/Events/`
- `GameCreatedDomainEvent` — game session created (Initial state).
- `GameStartedDomainEvent` — `Start()` succeeded; entered InProgress.
- `StepMadeDomainEvent` — player walked from one Link to a neighbor.
- `LastStepWarningIssuedDomainEvent` — one step away from Failed.
- `HintUsedDomainEvent` — hint allowance consumed; carries the `HintResult`.
- `UndoUsedDomainEvent` — last step rolled back.
- `ResetUsedDomainEvent` — game rewound to start.
- `GameCompletedDomainEvent` — target reached; `Score` computed.
- `GameFailedDomainEvent` — step budget exhausted without reaching target.
- `GameAbandonedDomainEvent` — player gave up.

### Link (5) — `Links/Events/`
- `LinkCreatedDomainEvent` — new word added.
- `OutgoingLinkAddedDomainEvent` — graph edge added.
- `OutgoingLinkRemovedDomainEvent` — graph edge removed.
- `LinkActivatedDomainEvent` — soft-deactivated word brought back.
- `LinkDeactivatedDomainEvent` — soft-deletion.

### Category (2) — `Categories/Events/`
- `CategoryCreatedDomainEvent`
- `CategoryEditedDomainEvent` — name and/or description changed.

### PlayerEnergy (2) — `Energy/Domain/PlayerEnergies/Events/`
- `PlayerEnergyConsumedDomainEvent` — emitted by `Consume()` with `PlayerId`, `Amount`, `RemainingAmount`.
- `PlayerEnergyRefilledDomainEvent` — emitted by `RechargeBasedOnElapsedTime()` only when at least one tick is gained; carries `PlayerId`, `GainedAmount`, `CurrentAmount`.

### PlayerQuest (2) + QuestDefinition (3) — `Quests/Domain/PlayerQuests/Events/`
- `PlayerQuestIssuedDomainEvent` — emitted by `IssueFor(...)`; carries `PlayerQuestId`, `PlayerId`, `QuestDefinitionId`.
- `PlayerQuestClaimedDomainEvent` — emitted by `Claim(now, isReady, energyReward, hintReward)`; carries `PlayerQuestId`, `PlayerId`, `QuestDefinitionId`, `EnergyReward`, `HintReward`. Mapped to `QuestClaimedIntegrationEvent` via outbox so Energy and Hint can each grant their bonus event-driven (each consumer guards on its reward's positivity).
- `QuestDefinitionCreatedDomainEvent` — emitted by `Create(...)`; carries `QuestDefinitionId`, `Name`, `Trigger` (string), `Threshold`, `EnergyReward`, `HintReward`, `PrerequisiteQuestDefinitionId?`, `ProgressBaseline` (string).
- `QuestDefinitionUpdatedDomainEvent` — emitted by `Update(...)`; same minus `Name` (immutable) and `Trigger` (immutable).
- `QuestDefinitionActivationChangedDomainEvent` — emitted by `Deactivate()` / `Reactivate()`; carries `QuestDefinitionId`, `IsActive`.

### PlayerHintInventory (5) — `Hint/Domain/PlayerHintInventories/Events/`
- `PlayerHintInventoryInitializedDomainEvent` — emitted by `InitializeFor(playerId, initialBalance)`; carries `PlayerId`, `InitialBalance`.
- `PlayerHintConsumedDomainEvent` — emitted by `Consume(amount, now)`; carries `PlayerId`, `Amount`, `RemainingBalance`, `ConsumedOn`.
- `PlayerHintGrantedDomainEvent` — emitted by `GrantBonus(amount, now)`; carries `PlayerId`, `Amount`, `NewBalance`, `GrantedOn`. Mapped indirectly via the Hint module's outbox + admin auditing pipeline (no public integration event — the consumer is internal to Hint's own admin audit notification).
- `PlayerHintAdminSetDomainEvent` — emitted by `AdminSet(newBalance, now)`; carries `PlayerId`, `NewBalance`, `SetOn`.
- `PlayerHintAdminResetDomainEvent` — emitted by `AdminReset(now)`; carries `PlayerId`, `ResetOn`.

### PlayerUndoInventory (5) — `Undo/Domain/PlayerUndoInventories/Events/`
- `PlayerUndoInventoryInitializedDomainEvent` — emitted by `InitializeFor(playerId, initialBalance)`; carries `PlayerId`, `InitialBalance`.
- `PlayerUndoConsumedDomainEvent` — emitted by `Consume(amount, now)`; carries `PlayerId`, `Amount`, `RemainingBalance`, `ConsumedOn`.
- `PlayerUndoGrantedDomainEvent` — emitted by `GrantBonus(amount, now)`; carries `PlayerId`, `Amount`, `NewBalance`, `GrantedOn`.
- `PlayerUndoAdminSetDomainEvent` — emitted by `AdminSet(newBalance, now)`; carries `PlayerId`, `NewBalance`, `SetOn`.
- `PlayerUndoAdminResetDomainEvent` — emitted by `AdminReset(now)`; carries `PlayerId`, `ResetOn`.

### PlayerResetInventory (5) — `Reset/Domain/PlayerResetInventories/Events/`
- `PlayerResetInventoryInitializedDomainEvent` — emitted by `InitializeFor(playerId, initialBalance)`; carries `PlayerId`, `InitialBalance`.
- `PlayerResetConsumedDomainEvent` — emitted by `Consume(amount, now)`; carries `PlayerId`, `Amount`, `RemainingBalance`, `ConsumedOn`.
- `PlayerResetGrantedDomainEvent` — emitted by `GrantBonus(amount, now)`; carries `PlayerId`, `Amount`, `NewBalance`, `GrantedOn`.
- `PlayerResetAdminSetDomainEvent` — emitted by `AdminSet(newBalance, now)`; carries `PlayerId`, `NewBalance`, `SetOn`.
- `PlayerResetAdminResetDomainEvent` — emitted by `AdminReset(now)`; carries `PlayerId`, `ResetOn`.

All events derive from `DomainEvent` (`Common/Domain/DomainEvent.cs`) which extends `IDomainEvent : INotification` — so they're MediatR-compatible without further wiring.

---

## Business Rules

### Category (3) — `Categories/Rules/`
- `CategoryNameMustNotBeEmptyRule`
- `CategoryNameMustNotExceedMaxLengthRule` (≤ 100)
- `CategoryDescriptionMustNotExceedMaxLengthRule` (≤ 500, null OK)

### Link (6) — `Links/Rules/`
- `LinkCannotPointToItselfRule` — no self-loops.
- `LinkOutgoingAlreadyExistsRule` — can't add an edge that already exists.
- `LinkOutgoingMustBeSameCategoryRule` — outgoing edges must stay inside the source Link's category.
- `LinkOutgoingMustExistRule` — can't remove an edge that doesn't exist.
- `LinkMustBeInactiveToActivateRule`
- `LinkMustBeActiveToDeactivateRule`

### Game (7) — `Games/Rules/`
- `GameMustBeNotStartedRule` — `Start()` only from Initial.
- `GameMustBeInProgressRule` — `MakeStep`, `UseHint`, `Undo`, `ResetToStart` require InProgress (or LastStepWarning where applicable).
- `GameMustNotBeFinishedRule` — `Abandon` only from active states.
- `StepMustBeValidRule` — target Link must be a neighbor of current Link.
- `GameHistoryMustNotBeEmptyRule` — `Undo` and `ResetToStart` require at least one prior step.
- `CategoryMustHaveEnoughLinksToStartGameRule` — Puzzle creation invariant.
- `PuzzleTargetLinkMustBeReachableRule` — Puzzle creation invariant.

### Allowance (1) — `Games/Allowances/Rules/`
- `HintAllowanceMustHaveRemainingRule`

### PlayerEnergy (5) — `Energy/Domain/PlayerEnergies/Rules/`
- `EnergyConfigurationMustBeValidRule` — `maxAmount > 0` and `rechargeIntervalSeconds > 0` at initialization.
- `EnergyAmountCannotBeNegativeRule` — `Consume(amount)` must receive `amount >= 0`.
- `EnergyAmountCannotExceedMaximumRule` — invariant guard (defensive; not enforced on `Consume`/`GrantBonus`, which deliberately permit over-max via the bonus path).
- `EnergyMustBeSufficientToConsumeRule` — `_currentAmount >= requestedAmount`; the rule whose violation propagates as `BusinessRuleValidationException` and is what blocks a `StartGameCommand` when energy is empty.
- `BonusAmountMustBePositiveRule` — `GrantBonus(amount, now)` requires `amount > 0` (zero or negative bonuses are nonsensical).

### PlayerQuest (1) + QuestDefinition (6) — `Quests/Domain/PlayerQuests/Rules/`
- `QuestMustBeReadyToBeClaimedRule` — `Claim(now, isReadyToClaim, energyReward, hintReward)` requires `_state == Active && isReadyToClaim` (caller-supplied flag computed from Stats counter vs threshold + not past expiry).
- `QuestThresholdMustBePositiveRule` — `QuestDefinition.Create` / `Update` require `threshold > 0`.
- `QuestRewardMustHaveAtLeastOnePositiveRule` — Sprint H reshape: replaces the older `QuestRewardMustBePositiveRule`. `Create` / `Update` require both `energyReward ≥ 0` and `hintReward ≥ 0` **and** at least one of them > 0. Empty-reward quests would be inert and are rejected.
- `QuestNameMustNotBeEmptyRule` — `Create` requires non-empty trimmed name.
- `QuestNameMustNotExceedMaxLengthRule` (≤ 64) and `QuestDescriptionMustNotExceedMaxLengthRule` (≤ 256) — Create/Update bounds checks.
- `QuestPrerequisiteMustNotCreateCycleRule` — parametric on a boolean. The command handler walks the prereq chain via `IQuestDefinitionRepository` before invoking `Create`/`Update` and passes `true` if the proposed prereq points (eventually) back at the definition being created/updated.

### PlayerHintInventory (3) — `Hint/Domain/PlayerHintInventories/Rules/`
- `HintAmountMustBePositiveRule` — `Consume(amount)` and `GrantBonus(amount)` require `amount > 0`.
- `HintAmountMustBeNonNegativeRule` — `InitializeFor(initialBalance)` and `AdminSet(newBalance)` require `≥ 0`.
- `HintBalanceMustBeSufficientRule` — `Consume(amount)` requires `_balance >= amount`. The rule whose violation propagates as `BusinessRuleValidationException` and is what blocks a fall-through `UseHintCommand` when the player inventory is empty.

### PlayerUndoInventory (3) — `Undo/Domain/PlayerUndoInventories/Rules/`
- `UndoAmountMustBePositiveRule` — `Consume(amount)` and `GrantBonus(amount)` require `amount > 0`.
- `UndoAmountMustBeNonNegativeRule` — `InitializeFor(initialBalance)` and `AdminSet(newBalance)` require `≥ 0`.
- `UndoBalanceMustBeSufficientRule` — `Consume(amount)` requires `_balance >= amount`; this is the rule that blocks in-game Undo when player inventory is empty.

### PlayerResetInventory (3) — `Reset/Domain/PlayerResetInventories/Rules/`
- `ResetAmountMustBePositiveRule` — `Consume(amount)` and `GrantBonus(amount)` require `amount > 0`.
- `ResetAmountMustBeNonNegativeRule` — `InitializeFor(initialBalance)` and `AdminSet(newBalance)` require `≥ 0`.
- `ResetBalanceMustBeSufficientRule` — `Consume(amount)` requires `_balance >= amount`; this is the rule that blocks in-game Reset when player inventory is empty.

All are `IBusinessRule` and dispatch through `CheckRule(...)`, throwing `BusinessRuleValidationException` on failure.

---

## Domain Services

| Interface | Purpose | Implementation lives in |
| --- | --- | --- |
| `IPathFinderService` (`Services/`) | Find optimal path from start to target Link; pick a valid target at a given depth. | Infrastructure (Sprint 3) — graph traversal (BFS/Dijkstra). |
| `ILinkNeighborResolver` (`Services/`) | Look up `LinkId`s adjacent to a given `LinkId`. | Infrastructure (Sprint 3) — wraps `ILinkRepository`. |
| `IGameConfigurationService` (`Services/`) | Resolve depth range, step budget, and allowance counts for a `Difficulty`. | Infrastructure (Sprint 3) — config-driven. |
| `IScoreCalculator` (`Services/`) | Compute final `Score` from depth + step/hint/undo/reset counts + difficulty. | **Domain** (`StandardScoreCalculator`) — pure logic, no I/O. A future `ConfigurableScoreCalculator` would live in Infrastructure. |

Domain services are received as method parameters — never stored on aggregates. See SKILLS.md rule #5.

`IEnergyConfigurationService` (`Modules/Energy/Domain/PlayerEnergies/`) exposes `MaximumAmount`, `RechargeIntervalSeconds`, and `GameStartCost`. Read from `IConfiguration` keys `Energy:MaxAmount`, `Energy:RechargeIntervalSeconds`, and `Energy:GameStartCost` with safe defaults (5 / 900 / 1).

`IUndoConfigurationService` and `IResetConfigurationService` expose
`InitialBalance` for fresh inventory initialization. Infrastructure
reads `Undo:InitialBalance` and `Reset:InitialBalance` respectively;
both default to 0 so new players earn these charges through future
quest/admin reward paths.

`IQuestCatalog` (Quests domain service surface; implementation lives in Quests.Infrastructure) — see Cross-Module Gateways section.

---

## Cross-Module Gateways

### IEnergyGuard — `Modules/Games/Application/Configuration/CrossModule/IEnergyGuard.cs`
The first synchronous cross-module gateway in LexiLink. Contract lives in **Games.Application** so Games depends only on its own surface (`EnsureCanStartGameAsync(playerId, ct)`). The adapter (`LexiLink.API/CrossModule/EnergyGuard.cs`) is composed in the API host and translates the call into `IEnergyModule.ExecuteCommandAsync(new ConsumePlayerEnergyCommand(playerId, _energyConfiguration.GameStartCost))`. `StartGameCommandHandler` invokes the guard **before** `game.Start()`: insufficient energy throws `BusinessRuleValidationException` and the game state never advances from `Initial`. Residual dual-write risk (energy debited but `game.Start()` throws on a duplicate call) is accepted for MVP. Documented as the intentional deviation in `kamil-modular-monolith-comparison.md`; architecture tests forbid Games.Application from depending on any Energy namespace.

### IHintGuard — `Modules/Games/Application/Configuration/CrossModule/IHintGuard.cs`
Sprint H sync gateway following the exact `IEnergyGuard` pattern. Contract lives in **Games.Application** so Games depends only on its own surface (`EnsureHintAvailableAsync(playerId, ct)`). The adapter (`LexiLink.API/CrossModule/HintGuard.cs`) is composed in the API host and translates the call into `IHintModule.ExecuteCommandAsync(new ConsumePlayerHintCommand(playerId, 1))`. `UseHintCommandHandler` invokes the guard **only when the per-game free quota is exhausted** (`!game.HasFreeHintRemaining`): the empty-inventory case throws `HintBalanceMustBeSufficientRule` via the Hint module and the puzzle state does not advance. Same dual-write tradeoff as `IEnergyGuard`. Architecture tests forbid Games.Application from depending on any Hint namespace; Games.IT exercises the contract via a configurable `RecordingHintGuard` stub.

### IUndoGuard — `Modules/Games/Application/Configuration/CrossModule/IUndoGuard.cs`
Sprint UR sync gateway contract for in-game Undo inventory spending.
Contract lives in **Games.Application** (`EnsureUndoAvailableAsync`)
so Games depends only on its own surface. `UndoCommandHandler` invokes
the guard before `Game.UseUndoWithExternalInventory()`, and Game then
increments only its `_undosUsed` statistic counter. The API host
adapter (`LexiLink.API/CrossModule/UndoGuard.cs`) calls
`IUndoModule.ExecuteCommandAsync(new ConsumePlayerUndoCommand(playerId,
1))`; insufficient inventory propagates `UndoBalanceMustBeSufficientRule`
and Game does not mutate.

### IResetGuard — `Modules/Games/Application/Configuration/CrossModule/IResetGuard.cs`
Sprint UR sync gateway contract for in-game Reset inventory spending.
Contract lives in **Games.Application** (`EnsureResetAvailableAsync`)
so Games depends only on its own surface. `ResetCommandHandler` invokes
the guard before `Game.ResetWithExternalInventory()`, and Game then
increments only its `_resetsUsed` statistic counter. The API host
adapter (`LexiLink.API/CrossModule/ResetGuard.cs`) calls
`IResetModule.ExecuteCommandAsync(new ConsumePlayerResetCommand(playerId,
1))`; insufficient inventory propagates `ResetBalanceMustBeSufficientRule`
and Game does not mutate.

### IQuestCatalog — `Modules/Quests/Domain/PlayerQuests/IQuestCatalog.cs`
Resolves a `QuestDefinitionId` to its `QuestDefinition` (returns null for deactivated entries so issuance/claim handlers can no-op). Implementation in Quests.Infrastructure (`QuestCatalog`) reads through `IQuestDefinitionRepository`. Registered scoped in `QuestsAutofacModule`.

### IQuestCounterReader — `Modules/Quests/Application/Configuration/CrossModule/IQuestCounterReader.cs`
The Sprint Q1 sync gateway. Contract lives in **Quests.Application** so Quests depends only on its own surface (`ReadAsync(playerId, nowUtc, ct) -> QuestCounters`). The adapter (`LexiLink.API/CrossModule/QuestCounterReader.cs`) is composed in the API host and queries `stats.PlayerStats.GamesCompleted` (Total), `stats.PlayerPeriodStats` (Daily for today UTC), and `players.PlayerAuthIdentities` (AuthLinked existence) via Dapper against a fresh `NpgsqlConnection`. Consumed by `IssueQuestCommandHandler` (baseline snapshot), `ClaimQuestCommandHandler` (ready-to-claim check), and `GetActiveQuestsQueryHandler` (read-time progress projection). Module isolation is preserved by keeping the SQL in the composition root — Quests has no structural reference to Stats or Players.

### QuestClaimedIntegrationEvent — `Modules/Quests/IntegrationEvents/QuestClaimedIntegrationEvent.cs`
Public contract emitted by Quests' outbox after `PlayerQuestClaimedDomainEvent` lands. Sprint H reshape: now carries `PlayerId`, `PlayerQuestId`, `QuestDefinitionId` (Guid), `EnergyReward`, **and** `HintReward`. Consumed by **two** independent handlers — Energy.Application's `QuestClaimedIntegrationEventHandler` skips when `EnergyReward == 0` and otherwise dispatches `GrantEnergyCommand`; Hint.Application's `QuestClaimedIntegrationEventHandler` skips when `HintReward == 0` and otherwise dispatches `GrantHintCommand` (which calls `PlayerHintInventory.GrantBonus`). This was LexiLink's first reverse cross-module event dependency (Energy → Quests), and Sprint H added the second symmetric dependency (Hint → Quests). ArchTests enforce that both dependencies stay public-contract-only — Quests.Domain/Application/Infrastructure remain forbidden from any consumer module; Energy.Application and Hint.Application carry granular allows on `Quests.IntegrationEvents`.

---

## Repositories

| Interface | Aggregate | Methods |
| --- | --- | --- |
| `ICategoryRepository` | `Category` | `GetByIdAsync`, `AddAsync` |
| `ILinkRepository` | `Link` | `GetByIdAsync`, `GetIdsByCategoryAsync`, `AddAsync` |
| `IGameRepository` | `Game` | `GetByIdAsync`, `AddAsync` |
| `IPlayerEnergyRepository` | `PlayerEnergy` | `GetByIdAsync`, `AddAsync` |
| `IPlayerQuestRepository` | `PlayerQuest` | `GetByIdAsync`, `GetActiveOrClaimedByPlayerAndDefinitionAsync`, `GetByPlayerAsync`, `HasClaimedAsync(QuestDefinitionId)`, `AddAsync` |
| `IQuestDefinitionRepository` | `QuestDefinition` | `GetByIdAsync`, `GetAllAsync`, `AddAsync` |
| `IPlayerHintInventoryRepository` | `PlayerHintInventory` | `GetByIdAsync`, `AddAsync` |
| `IPlayerUndoInventoryRepository` | `PlayerUndoInventory` | `GetByIdAsync`, `AddAsync` |
| `IPlayerResetInventoryRepository` | `PlayerResetInventory` | `GetByIdAsync`, `AddAsync` |

No `Update` / `Delete` methods — aggregates mutate in place; `IUnitOfWork.CommitAsync()` persists changes through EF Core's change tracker. Soft-delete uses state methods on the aggregate (see SKILLS.md rule #12).

---

## Application Concepts

### Command / Query
Write-side requests (`CreateLinkCommand`, `EditCategoryCommand`) implement `ICommand` or `ICommand<TResult>`. Read-side requests (`GetCategoriesQuery`, `GetLinkDetailsQuery`) extend `QueryBase<TResult>` (which carries `Id` for pipeline correlation) and implement `IQuery<TResult>`. Both contracts live in `Modules/Games/Application/Configuration/{Commands,Queries}/` — per-module, intentionally not in `Common.Application`.

### Handler
`ICommandHandler<TCommand>` / `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>`. Handlers are `internal` — only the contract surface escapes the module assembly.

### DTO
Read-side projection target. Always a positional `record` (works with Dapper, immutable, modern C# affordance). One DTO per query — feature folder `Application/<Aggregate>/<QueryName>/` owns its DTO file.

### Pipeline Behavior (planned, Sprint 2)
MediatR `IPipelineBehavior<TRequest, TResponse>` implementations: `LoggingBehavior` (uses `QueryBase.Id` for correlation), `ValidationBehavior` (FluentValidation), `UnitOfWorkBehavior` (wraps commands in `IUnitOfWork.CommitAsync`).

### Repository, UnitOfWork
Repositories load and add aggregates; `IUnitOfWork.CommitAsync(CancellationToken)` flushes pending changes through EF Core. The `UnitOfWorkBehavior` calls `CommitAsync` after a successful command handler returns.

### NotFoundException — `Common/Application/Exceptions/`
Thrown by handlers when a `GetByIdAsync` or `QuerySingleOrDefaultAsync` returns `null`. Carries `EntityName` + `Id`. Mapped to HTTP 404 by exception middleware (Sprint 4).

---

## See Also

- `SKILLS.md` — the rules these terms participate in.
- `CONVENTIONS.md` — naming and code-shape conventions for these terms.
- `ROADMAP.md` — when each remaining piece (Application/Infrastructure/API/Tests) lands.
