# ROADMAP.md

The forward-looking sprint plan. *What's already shipped* lives in `progress.md`; *what's happening right now* lives in `activeContext.md`.

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
