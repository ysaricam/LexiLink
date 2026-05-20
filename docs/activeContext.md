# activeContext.md

Project'in o anki yönü ve en yakın sıra. Geçmiş teslimatlar `progress.md`,
uzun vadeli plan `ROADMAP.md`, mimari karşılaştırma notları
`kamil-modular-monolith-comparison.md` içindedir.

> Last updated: 2026-05-20 (Administration Slice B8 closed)

---

## Active Sprint

**Administration Module** — sixth backend module. Sprint plan locked
in `ROADMAP.md > Administration Module`. Foundation slice (B1) is the
next implementation step.

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

**Administration sprint devam ediyor — B1–B8 kapandı.**
B1 modül foundation 2026-05-18, B2 admin registration + outbox publish +
bootstrap seed 2026-05-19, B3 admin authentication 2026-05-20, B4
admin authorization cross-cut 2026-05-20, B5 audit projection +
`/admin/audit` endpoint 2026-05-20, B6 Quests catalog data-driven
2026-05-20, B7 quest admin operations + first per-module
AdminAuditing decorator 2026-05-20, B8 energy admin operations
2026-05-20.

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

Sırada **Slice B9 — Players admin operations (ban/unban)**:
- `Player.Ban(reason, now)` + `Player.Unban(now)` domain methods +
  state flag + ban event.
- `BanPlayerCommand`, `UnbanPlayerCommand`,
  `GetPlayerAdminDetailQuery` (rich admin view).
- `AuthenticatedPlayer` policy banned tokens'ı reddetsin (login
  boundary).
- API: `GET /admin/players/search`, `GET /admin/players/{id}`,
  `POST /admin/players/{id}/ban`, `POST /admin/players/{id}/unban`.

Sonra B10 (content admin guard — mevcut anonim `POST /categories`
endpoint'leri `/admin/...` altına geçer).

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
