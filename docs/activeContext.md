# activeContext.md

What's happening on the project **right now**. Update this file at the start and end of each significant work session — short and current beats long and stale.

> Last updated: 2026-05-03 (Sprint 3 — Game aggregate mapping/repository slice in flight)

---

## Active Sprint

**Sprint 3 — Games.Infrastructure** (started 2026-05-03 after Sprint 2 decorator stack closed). DB choice: **PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1`. Otherwise birebir Kamil-faithful.

Tracking: `ROADMAP.md` → Sprint 3 section.

---

## Currently Building

Sprint 3 third slice in flight: **Game aggregate mapping/repository**. Plan approved 2026-05-03; implementation starting. Heaviest slice — five owned VOs (Puzzle, Score?, StepBudget, three Allowances) plus two collections (`_history` at root, `_optimalPath` nested inside Puzzle). After this lands, only domain service implementations + read-side views/migrations remain in Sprint 3.

- `Common.Infrastructure/` — EF Core building blocks Kamil-faithful: `IUnitOfWork` (moved from Common.Domain), `UnitOfWork` (DbContext + dispatcher), `TypedIdValueConverter<T>`, `StronglyTypedIdValueConverterSelector`.
- `Modules/Games/Infrastructure/` — `GamesContext` at root with three `DbSet`s + `ApplyConfigurationsFromAssembly`; aggregate slices done in `Domain/Categories/` and `Domain/Links/`; `Domain/Games/` to be added now.
- `Modules/Games/Infrastructure/Configuration/Processing/` — full decorator stack from Sprint 2 (UoW, Logging, Validation; void + result variants).
- `Common.Infrastructure/DomainEventsDispatching/` — domain event dispatch from Sprint 2 still in place.

Games module Application layer remains feature-complete (14 commands + 8 queries).

**Approved decisions for the Game slice:**
1. **Game-level `_startLinkId`/`_targetLinkId` removed.** Duplicated `_puzzle.StartLinkId`/`_puzzle.TargetLinkId`; usages in `EvaluatePostStepTransitions`, `Undo`, `ResetToStart` delegate to Puzzle. Single source of truth.
2. **`_history` wrapped as `GameHistoryStep(int StepNumber, LinkId LinkId)` VO + `OwnsMany`.** Composite PK `(GameId, StepNumber)`. Mirrors the `OutgoingLink` precedent and matches `[Games].[v_GameHistory]` columns.
3. **`_optimalPath` (inside Puzzle VO) wrapped as `OptimalPathStep(int Position, LinkId LinkId)` VO + nested `OwnsMany`.** Table `GameOptimalPath` keyed by `(GameId, Position)` — EF flattens nested-owned keys to the aggregate root. jsonb alternative rejected for consistency.
4. **Owned VOs gain `private SomeVo() { }` ctors.** Six VOs (`Score`, `StepBudget`, three Allowances, `Puzzle`); `Puzzle`'s ctor initializes `_optimalPath = []`. EF Core 10's parameterized-ctor binding is brittle for owned types with collection params — explicit parameterless ctor is reliable.
5. **`GameState` and `Difficulty` stored as `varchar(32)`** via `HasConversion<string>().HasMaxLength(32)`. Forward-compatible (re-ordering enums doesn't corrupt data).
6. **`Score?` mapped via `OwnsOne<Score>` with `Property(s => s.Points).HasColumnName("Score").IsRequired(false)`.** Single nullable column; `null` row → `_score = null`.
7. **Owned scalars flattened into `Games` table** with column names matching the read view: `MaxSteps`, `StepsTaken`, `HintsRemaining`+`HintsUsed`, `UndosRemaining`+`UndosUsed`, `ResetsRemaining`+`ResetsUsed`. The view computes `*Total = Remaining + Used` — that arithmetic lives in the view, not in EF mapping.
8. **`GameRepository` minimal:** only `GetByIdAsync` and `AddAsync`. Owned types/collections auto-loaded by EF — no `Include`, no `AsNoTracking`.

---

## Last Completed

- **Sprint 3 slice 2 — Link aggregate mapping/repository.** `LinkEntityTypeConfiguration` + `LinkRepository` under `Modules/Games/Infrastructure/Domain/Links/`. New domain VO `OutgoingLink` (sealed `ValueObject`, single prop `LinkId TargetId`) replaces the raw `List<LinkId>` inside Link — gives EF a target for `OwnsMany` and keeps `_outgoingLinks` semantically distinct from random LinkId collections. Two outgoing-rules (`LinkOutgoingAlreadyExistsRule`, `LinkOutgoingMustExistRule`) updated to `IReadOnlyCollection<OutgoingLink>`. `OwnsMany` to table `[games].LinkOutgoingLinks` with composite PK `(LinkId, OutgoingLinkId)`; field-access mode set explicit. Repo queries private fields via `EF.Property<T>(x, "_categoryId" / "_isActive")`. Owned collection auto-loaded by EF — no explicit `Include`.
- `Games.Application` Games: 7 commands (`CreateGame` returning `Guid`, `StartGame`, `MakeStep`, `UseHint` returning `HintResultDto`, `Undo`, `Reset`, `AbandonGame`) + `GetGameByIdQuery` returning `GameDetailsDto` (with denormalized `StartWord`/`TargetWord`/`CurrentWord` and `IReadOnlyList<GameHistoryStepDto> History`).
- Domain tweak: `Game.UseHint()` `void` → `HintResult` (so handler can project to DTO without reading event payload).
- Domain addition: `ILinkRepository.GetActiveIdsByCategoryAsync` (for `CreateGameCommand` puzzle generation).
- `Games.Application` Categories: `CreateCategory`, `EditCategory`, `GetCategoryDetails` (with `LinkCount`), `GetCategories`.
- `Games.Application` Links: 5 commands + 3 queries.
- `Common.Application/Exceptions/NotFoundException`, `Common.Application/Data/ISqlConnectionFactory`.
- Domain additions for Link soft-delete: `Link.Activate()` / `Link.Deactivate()` with rules + events.

---

## Next Focus

After the Game slice (currently in flight) lands, Sprint 3 has two remaining slices:

1. **Domain service implementations** — `PathFinderService`, `LinkNeighborResolver`, `GameConfigurationService`.
2. **Read-side views + initial migration** — `[games].v_categories`, `v_links`, `v_games`, `v_game_history` views (with `Total = Remaining + Used` arithmetic for allowances) plus `LinkOutgoingLinks`/`GameHistory`/`GameOptimalPath` schema. **Pre-existing risk to address here:** all Dapper handlers use SQL-Server-style `[Schema].[Table]` brackets — Postgres standard mode doesn't parse them. View layer needs to translate or handlers need updating.

Sprint 2 carry-overs (remain deferred to Sprint 4):
- **Autofac composition root** — `GamesModule` Autofac module calling `RegisterGenericDecorator(...)` with order logging → validation → UoW → inner handler. Decorators exist but aren't wired yet.
- **Per-aggregate FluentValidation `AbstractValidator<T>`** classes — added alongside each command as the persistence path lights up.

Already done (Sprint 3 slices 1 & 2):
- `IUnitOfWork` moved Common.Domain → Common.Infrastructure; `UnitOfWork`, `TypedIdValueConverter<T>`, `StronglyTypedIdValueConverterSelector` added.
- `GamesContext` skeleton at `Modules/Games/Infrastructure/` root; auto-scans entity configurations.
- `CategoryEntityTypeConfiguration` + `CategoryRepository` (smallest aggregate, sets the per-aggregate pattern in `Modules/Games/Infrastructure/Domain/{Aggregate}/`).
- `LinkEntityTypeConfiguration` + `LinkRepository`; `OutgoingLink` VO introduced; `OwnsMany` mapping to `[games].LinkOutgoingLinks` with composite PK.
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1` added to `Games.Infrastructure`.

---

## Open Decisions

- **Activate/Deactivate on Category?** Categories own Links. Deactivating a Category would functionally cascade to its Links for read purposes. Whether to expose this on the aggregate, surface it as an Application orchestration, or skip it entirely — undecided. Default: leave it out for now; revisit when the UI exposes a category-level archive action.

---

## Recent Design Choices That Might Surprise Future-Claude

These look redundant or weird but each is intentional. Don't "clean them up" without re-reading the rationale.

- **Cross-cutting concerns are decorators, not pipeline behaviors — and they live per-module, not in Common.** Command-handler decorators sit in `Modules/Games/Infrastructure/Configuration/Processing/` and target the module's own `ICommandHandler<T>` / `ICommandHandler<T, TResult>` (from `Games.Application.Configuration.Commands`), not MediatR's `IRequestHandler` directly and not `IPipelineBehavior<,>`. Two reasons: (a) `INotificationHandler<T>` (used for in-process domain events — see `DomainEventsDispatcherNotificationHandlerDecorator` in Common) has no `IPipelineBehavior` equivalent, so the notification side *must* be decorator-based and keeping commands on the same pattern means one mental model; (b) the module-specific generic constraint (`where T : ICommand`) prevents extracting these to Common — exactly Kamil's per-module Processing/ layout. Cost: every concern needs two generic decorator types (void + result), and Player module will get its own copies. Don't "modernize" this to pipeline behaviors and don't move decorators to Common.
- **Per-module CQRS contracts (`ICommand`, `IQuery`, handlers).** `Games.Application/Configuration/{Commands,Queries}/` duplicates contracts that *look* like they belong in `Common.Application`. Kamil's deliberate choice — each module is independently extractable into a microservice; shared contracts couple modules together. See `SKILLS.md` rule #10.
- **Positional `record` DTOs.** Kamil's reference uses `class` because records didn't exist when it was written (2019). For LexiLink, `record` is the modern equivalent — works with Dapper, immutable, value equality. See `CONVENTIONS.md` → DTO Style.
- **`QueryBase<T>.Id` looks redundant.** It's not. The forthcoming `LoggingBehavior` correlates a single query through the pipeline using that Id — without it, multiple log lines from one query can't be tied together.
- **Soft delete only — no `Delete` / `Remove` on aggregates.** Past Games reference Links by Id and need their words readable for replay/history. Lifecycle is `Activate()` / `Deactivate()` with rules + events. See `SKILLS.md` rule #12.
- **Nullable repositories + handler throws `NotFoundException`.** Repository contract returns `T?`; handler converts `null` to `NotFoundException` with `?? throw new NotFoundException(...)`. Cleaner than Kamil's pre-nullable pattern of throwing inside the repository.
- **`StandardScoreCalculator` lives in Domain, not Infrastructure.** Pure logic, no I/O. A future `ConfigurableScoreCalculator` would be Infrastructure. See `SKILLS.md` rule #8.
- **Domain services as method parameters, not aggregate fields.** `Game.MakeStep(LinkId, ILinkNeighborResolver, IScoreCalculator)`. The dependency is visible at the call site instead of hidden behind constructor injection. See `SKILLS.md` rule #5.
- **Allowances are three separate VOs (Hint/Undo/Reset), not one generic `Allowance<T>`.** Each carries its own rule; the type system distinguishes them so a method that takes `HintAllowance` can't accidentally receive a `ResetAllowance`. Generic deduplication was considered and rejected.
- **`OutgoingLink` is a one-property VO wrapping `LinkId TargetId` — not just `List<LinkId>` inside Link.** Three reasons: (a) gives EF a target type for `OwnsMany` mapping (typed-IDs alone aren't entity-shaped); (b) semantically distinct — "outgoing edge" is a domain concept inside Link's invariant, not a random LinkId collection; (c) leaves room to grow (e.g., metadata like Order, AddedAt) without a ripple. JSON-column and shared-`DbSet` alternatives were rejected — the former breaks queryability and isn't Kamil's tabular style; the latter would expose the join row as its own entity and break Link's aggregate boundary.
- **`GameHistoryStep` and `OptimalPathStep` are wrapper VOs on the same pattern as `OutgoingLink`.** `GameHistoryStep(int StepNumber, LinkId LinkId)` lives at the Game aggregate root; `OptimalPathStep(int Position, LinkId LinkId)` lives nested inside the `Puzzle` owned VO. Both exist for the same three reasons as `OutgoingLink` — EF needs an entity-shaped target for `OwnsMany`, the wrapper gives the position/step a semantic name (not just a list index), and the schema gains explicit ordering columns. Domain consequence: `Game._history.Add(linkId)` becomes `Game._history.Add(new GameHistoryStep(_history.Count + 1, linkId))`; `_history[^1]` becomes `_history[^1].LinkId`. Minor but contained.
- **Game-level `_startLinkId` and `_targetLinkId` fields removed in favor of `_puzzle.StartLinkId`/`TargetLinkId`.** They duplicated Puzzle's data — set in the ctor and never mutated afterward. Keeping both would force EF to either map two duplicate columns or share one column ambiguously; removing them collapses to a single source of truth. If a future Sprint adds a "puzzle re-roll while preserving Game.Id" feature that needs the original endpoints, re-introducing them is a one-PR change.
- **Owned VOs (`Score`, `StepBudget`, three `Allowance`s, `Puzzle`) carry private parameterless ctors, even though their primary ctors are `private`.** EF Core 10 *can* bind to parameterized ctors when parameter names match property names, but it's brittle for owned types — particularly when a parameter is a collection (Puzzle's `List<LinkId> optimalPath`). Adding `private SomeVo() { }` mirrors the aggregate-root pattern (`private Game() { _history = []; }`) and is reliably honored by EF. Don't remove these as "dead code" — they're the EF materialization path. Same precedent as `OutgoingLink`.

---

## Pointers

- `progress.md` — log of delivered work.
- `ROADMAP.md` — what's next, in detail.
- `SKILLS.md` — the rules.
- `GLOSSARY.md` — what the terms mean.
- `CONVENTIONS.md` — naming, layout, visibility.
