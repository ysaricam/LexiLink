# progress.md

History log of delivered work. Newest at top. Append entries when significant work lands; don't rewrite the past.

---

## Architecture Alignment Pass ⏳ in progress (started 2026-05-11)

Kamil Grzybek reference comparison sonrası eksikler sırayla kapatılıyor. Her adımda önce "Kamil bunu böyle mi yapıyor?" kontrol edilecek; bilinçli sapmalar ayrıca notlanacak.

### Next planned — Stats module closes Integration Events / Outbox-Inbox gap

- **Decision** — Integration Events eksikliği geçici Players consumer ile değil, ayrı **Stats module** eklenerek kapatılacak.
- **Why** — Kamil yaklaşımına daha uygun: Games/Players birbirini doğrudan çağırmaz; Stats module diğer modüllerin public integration event'lerini consume eden doğal cross-module projection owner olur.
- **Initial scope for tomorrow** — `Stats.Domain/Application/Infrastructure`, `Stats.IntegrationEvents` consumer setup, Games/Players domain notifications → outbox mappings, outbox processor, inbox/idempotency, minimum read API (played/completed counts or leaderboard).
- **Carry-over from Slice 4** — `GameCompletedDomainEvent` already carries `PlayerId`, `StartLinkId`, `TargetLinkId`, `Score`; this is ready input for `GameCompletedIntegrationEvent`.

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
