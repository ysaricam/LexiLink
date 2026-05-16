# LexiLink ve Kamil Grzybek Modular Monolith Karşılaştırması

Bu doküman LexiLink'i Kamil Grzybek'in
[`modular-monolith-with-ddd`](https://github.com/kgrzybek/modular-monolith-with-ddd)
örnek projesiyle karşılaştırır. Amaç örneği birebir kopyalamak değil; hangi
farkların bilinçli, hangilerinin geçici, hangilerinin de LexiLink'in sonraki
mimari adımlarını etkilemesi gerektiğini netleştirmektir.

Bu karşılaştırmada bilinçli olarak kapsam dışı bırakılan konular:

- Veritabanı sağlayıcısı seçimi.
- API endpoint dağıtım stili.

Bu iki konu LexiLink'te bilinçli alınmış kararlar olduğu için problem olarak
değerlendirilmez.

## Yönetici Özeti

LexiLink, Kamil'in genel modül yapısını, CQRS klasör düzenini, domain event
stilini, outbox yönünü ve integration event contract fikrini takip ediyor. En
büyük farklar runtime composition, asenkron mesajlaşma derinliği, build
yönetişimi ve operasyonel olgunluk tarafında.

Kamil'in projesi güçlü modül izolasyonu ve gelecekte modülleri ayırabilme
hedefiyle tasarlanmış: her modül kendi composition root'una, internal command
mekanizmasına, inbox/outbox işlemeye, scheduled job'lara, daha zengin
architecture test'lere ve tam bir auth/permission hikayesine sahip.

LexiLink şu an daha küçük ve daha okunabilir bir sistemi tercih ediyor: ortak
host container, explicit project reference'lar, modül bazlı unit-of-work
dispatcher'ları, MediatR tabanlı in-process integration event'ler ve daha basit
bir Stats projection modülü. Bu aşama için makul bir tercih. Yine de LexiLink
production-facing hale gelmeden veya daha fazla cross-module workflow eklemeden
önce Kamil'den alınması gereken net parçalar var.

Bu dokümanın Kamil alignment uygulama planı tamamlandı. İlk
production-readiness baseline'ı da `ROADMAP.md` içinde kapatıldı. Bu dosya
artık mimari farkların gerekçesini tutar; günlük sıra için `activeContext.md`,
teslim geçmişi için `progress.md` okunmalıdır.

LexiLink için Kamil uyumu ve ilk production-readiness baseline'ı tamamlandı.
Kalan değerli takip işleri artık ürün ve dağıtım odaklıdır: gerçek
Apple/Google provider entegrasyonu credential'lar hazır olduğunda, game
content/admin tooling, gameplay polish ve deployment packaging.

2026-05-12 güncellemesi: Merkezi build/package yönetimi ve Kamil tarzı
application convention architecture test'leri LexiLink'e eklendi. Kalan build
yönetişimi işleri CI ve isteğe bağlı warnings/analyzers policy seviyesindedir.

## Detaylı Karşılaştırma

| Alan | Kamil'in projesi | LexiLink | Farkın sebebi | LexiLink için hangisi daha iyi |
| --- | --- | --- | --- | --- |
| Module composition root | Her modülün kendi static composition root'u var ve module startup her modül için ayrı Autofac container kuruyor. Module facade method'ları MediatR'ı modül lifetime scope'undan resolve ediyor. | Modüller API host'un ortak Autofac container'ına register ediliyor. Module facade'lar DI'dan `ISender` alıp doğrudan send ediyor. Shared container içinde module-owned UoW/dispatcher/outbox servisleri concrete/self scoped tutuluyor ve composition tests ile common service leakage korunuyor. | LexiLink daha küçük başladı ve runtime cross-module concern sayısı daha az. Önceki gerçek collision'lar module-owned servislerle kapatıldı; yeni collision olmadığı için per-module container rewrite ertelendi. | İzolasyon için Kamil'in modeli daha güçlü. LexiLink'in ortak root yaklaşımı şu an kabul edilebilir; modüller büyüdükçe veya yeni collision oluşursa per-execution scope/per-module root tekrar değerlendirilmeli. |
| Lazy initialization | Static composition root'a rağmen Kamil'de setup gerçekten lazy değil; startup modül container'ını eager initialize ediyor. | LexiLink de modülleri host container'a eager register ediyor. | İki sistem de deterministic startup wiring'e ihtiyaç duyuyor. Buradaki "lazy init" farkı büyük ölçüde yanlış anlaşılma. | Aksiyon gerekmez. Eager startup daha açık. |
| Module facade visibility | `MeetingsModule` gibi module implementation'ları public ve diğer modüller module API üzerinden çağırabiliyor. | `GamesModule` gibi implementation'lar internal ve public application contract'ların arkasında register ediliyor. | LexiLink production kodunda synchronous module-to-module call kullanmıyor. | Module interaction event-driven kaldığı sürece LexiLink'in daha sıkı visibility tercihi daha iyi. Public module facade, direct cross-module call bilinçli bir pattern olursa anlamlı. |
| Cross-module communication | Kamil hem asynchronous integration event kullanıyor hem de örneğin Registrations -> UserAccess için module facade üzerinden synchronous gateway kullanıyor. | LexiLink Games/Stats projection akışlarında integration event kullanıyor. Energy modülünün eklenmesiyle ilk synchronous cross-module gateway gelmiş oldu: `Games.Application/IEnergyGuard`. Adapter `LexiLink.API/CrossModule/EnergyGuard.cs`'de yaşar ve Energy modülünün public facade'ını (`IEnergyModule`) çağırır. Quests modülü 2026-05-15'te eklendiğinde LexiLink'in ilk **reverse cross-module event dependency**'si canlandı: `Energy.Application` artık `Quests.IntegrationEvents.QuestClaimedIntegrationEvent`'i tüketiyor; Quests outbox'tan publish edilen event Energy'nin `QuestClaimedIntegrationEventHandler`'ını tetikliyor ve handler defansif `EnsurePlayerEnergyExistsCommand` + `GrantEnergyCommand` dispatch ediyor. Reward delivery bilinçli olarak event-driven (invariant değil, intent); yeni bir sync gateway gerekmedi. | Game başlatma anında energy yeterliliği invariant'sal bir karardı — sync gateway doğru cevap. Quest reward'ı ise intent — async event-driven doğru cevap. Bu iki örnek birlikte LexiLink'in cross-module pattern matrisini tanımlıyor. | Mevcut model uygun. Kurallar: (1) yeni sync gateway *yalnızca* invariant-level check için açılır ve `IEnergyGuard` pattern'i (Games.Application interface + API host adapter) korunur; (2) yeni reverse cross-module event dep'lerde ArchTests granular allow eklenir — consumer module'ün Application katmanı sadece producer'ın `IntegrationEvents` assembly'sini referans edebilir, `Domain/Application/Infrastructure` forbidden kalır. |
| MediatR/decorator yapısı | Kamil command/query abstraction'larını register ediyor ve decorator'ları `ICommandHandler<>`, `IRequestHandler<>` ve notification handler'lar arasında bölüyor. | LexiLink unit of work, validation ve logging için doğrudan MediatR `IRequestHandler<>` / `IRequestHandler<,>` decorator'ları kullanıyor. | LexiLink'in mevcut MediatR/Autofac kurulumunda Kamil tarzı split, command'ların beklenen decorator'lardan kaçmasına yol açabiliyor. | Bu codebase için LexiLink'in mevcut decorator yaklaşımı daha iyi. Kamil'in pattern'i ancak custom command handler interface'lerinin her zaman resolve path üzerinde olduğu kanıtlanırsa kopyalanmalı. |
| Unit of work ve domain event dispatch | Kamil ortak `IUnitOfWork`, `IDomainEventsDispatcher` ve outbox servislerini güvenle register edebiliyor; çünkü her modülün kendi container'ı var. | LexiLink `GamesUnitOfWork`, `PlayersUnitOfWork` ve modül bazlı domain-event dispatcher'lar kullanıyor. | Ortak host container'da generic infrastructure registration'lar kolayca çakışabiliyor. LexiLink Stats/outbox eklerken bu sınıf problemi yaşadı. | Mevcut shared container altında LexiLink'in modül bazlı servisleri daha iyi. Kamil'in ortak abstraction'ları LexiLink per-module container'a geçerse daha güvenli hale gelir. |
| Domain notification mapping | Kamil domain-event notification wrapper'larını resolve edip module infrastructure içinde outbox message name'lerine map ediyor. | LexiLink ortak `DomainNotificationsMap` singleton'ına sahip ve module startup method'ları mapping'leri explicit ekliyor. Mapping yoksa fail-fast davranıyor. | LexiLink shared container registration'ları içinde predictable mapping'e ihtiyaç duydu. | LexiLink'in fail-fast mapping'i iyi. Global singleton basit; ancak plugin-like module loading gibi bir model gelirse tekrar değerlendirilmeli. |
| Domain notification konumu | Kamil domain-event notification wrapper'larını Application'a koyuyor, Infrastructure bunları scan ediyor. | LexiLink producer outbox notification wrapper'larını şu an Infrastructure altında tutuyor. | LexiLink bu wrapper'ları application behavior değil, serialization/outbox plumbing olarak görüyor. | Wrapper'lar application event policy'nin parçası sayılıyorsa Kamil'in konumu daha temiz. LexiLink'in mevcut konumu şimdilik kabul edilebilir; outbox davranışı application contract haline gelirse tekrar bakılmalı. |
| Outbox mimarisi | Kamil per-module outbox tabloları, Quartz-triggered processing command'ları, type map'leri ve event-bus publication kullanıyor. | LexiLink module outbox tablolarını okuyan ortak, schema-parametrized `OutboxProcessor` kullanıyor; domain notification'lar MediatR ile module içi işleniyor ve public integration event'ler `IEventsBus` üzerinden publish ediliyor. API Quartz job'u processor'ları tetikliyor. | LexiLink şu an external message delivery değil, tek bir in-process async projection path'e ihtiyaç duyuyor. | Distributed messaging için Kamil'in external bus tarafı hâlâ daha production-ready. Scheduling/retry/error ve abstraction baseline'ı artık güçlü. |
| Outbox failure davranışı | Kamil'in outbox processing'i scheduled ve module-specific; fakat hatalı bir message command execution path'i durdurabiliyor. | LexiLink her outbox message için exception yakalıyor, logluyor, hatalı message'ı unprocessed bırakıyor ve batch'e devam ediyor. | LexiLink'in ortak processor'ı local batch resilience'ı bilerek önceliklendiriyor. | Local resilience açısından LexiLink daha iyi. Scheduling modeli açısından Kamil daha güçlü. Uzun vadede ikisinin birleşimi ideal: scheduler/backoff/dead-letter + per-message isolation. |
| Inbox pattern | Kamil incoming integration event'leri consuming module içinde raw inbox message olarak saklıyor ve sonra inbox command'larla işliyor. | Stats artık incoming integration event'leri raw serialized `stats.InboxMessages` tablosuna append ediyor; ayrı Stats inbox processor projection'ı güncelliyor ve Quartz job tarafından tetikleniyor. | Bu pattern duplicate event idempotency, retry/error metadata ve consumer bazlı failure isolation için alındı. | Stats için Kamil'e hizalanmış baseline iyi. Yeni consumer oluşursa aynı pattern bilinçli olarak çoğaltılmalı; her modüle peşinen eklenmemeli. |
| Event bus abstraction | Kamil `IEventsBus` abstraction'ı, integration-event subscription'ları ve module inbox'a yazan generic event-bus handler'lar kullanıyor. | LexiLink public integration event publish/consume için `IEventsBus` ve `IIntegrationEventHandler<T>` kullanıyor. İlk implementation `InMemoryEventsBus`; external broker yok. | Transport bağımlılığı MediatR'dan ayrıldı, fakat process separation gerçek ihtiyaç olmadığı için broker eklenmedi. | Mevcut kapsam için LexiLink artık iyi hizalı. Modüller process olarak ayrılırsa `IEventsBus` implementation'ı değiştirilebilir. |
| Integration event contract'ları | Kamil ayrı module IntegrationEvents projeleri kullanıyor; consumer'lar module internals yerine bu contract'lara referans veriyor. | LexiLink de Games ve Players için ayrı IntegrationEvents projeleri kullanıyor; `IIntegrationEvent` artık MediatR `INotification`'dan türemiyor. | Public contract'lar in-process transport detayından ayrıldı. | Bu hizalanma iyi. External bus gelirse contract'ları değiştirmeden infrastructure implementation değiştirilebilir. |
| Internal commands | Kamil `InternalCommands`, command scheduler, processing job'lar, Polly retry ve error persistence kullanıyor. | Stats artık module-owned `stats.InternalCommands`, scheduler, processor ve `ProcessStatsInboxCommand` kullanıyor. Quartz Stats inbox job'u direct processor çağırmak yerine internal command schedule eder ve processor'ı çalıştırır. | Boş altyapı yerine scheduled projection maintenance gerçek kullanım alanı olarak seçildi. | İlk baseline Kamil'e hizalı. Diğer modüllere ancak gerçek delayed/retried side effect oluşursa çoğaltılmalı. |
| Scheduled processing | Kamil outbox, inbox, internal commands ve recurring job'lar için Quartz kullanıyor. | LexiLink outbox ve Stats inbox/internal command processing için Quartz hosted jobs kullanıyor. Recurring business jobs henüz yok. | Önce gerçek async processing path'leri hizalandı; recurring job ihtiyacı olmadığı için eklenmedi. | Mevcut kapsam için LexiLink yeterli. Recurring business jobs eklenirse aynı scheduled-processing çizgisi genişletilmeli. |
| Stats/read-model module yapısı | Kamil'in business module'ları genelde Domain/Application/Infrastructure içeriyor ve read model'ler full module içinde duruyor. | LexiLink Stats modülü sadece Application/Infrastructure içeriyor ve projection/read-model module gibi davranıyor. | Stats derived data. Fake domain model eklemek invariant sağlamadan ceremony artırırdı. | LexiLink burada daha iyi. Modül projection sahibiyse ve domain behavior yoksa read-model-only module geçerli bir tercih. |
| Architecture tests | Kamil per-module architecture test'lerde immutable command/query, handler naming, non-public handler/validator, direct MediatR handler kullanmama ve internal command constructor kuralları gibi zengin application convention'ları kontrol ediyor. | LexiLink merkezi architecture test'lerde layer dependency, module boundary, integration-event isolation, API composition boundary ve artık application convention kontrollerini birlikte çalıştırıyor. | LexiLink en riskli boundary olan accidental module coupling'i merkezi tutuyor; Kamil'den alınan code-shape kontrolleri mevcut tasarımla uyumlu kısımlarla eklendi. | LexiLink artık bu başlıkta yeterli baseline'a sahip. Internal command constructor kuralı, internal command pattern'i eklenirse ayrı test edilmeli. |
| Build yönetişimi | Kamil `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, analyzers, warnings-as-errors, StyleCop, Nuke ve CI pipeline configuration kullanıyor. | LexiLink root `Directory.Build.props`, `Directory.Packages.props`, local `scripts/test.sh` ve GitHub Actions CI quality gate kullanıyor; analyzers/warnings-as-errors henüz yok. | Ortak TFM/nullable/implicit-usings, central package version yönetimi ve tekrar edilebilir CI gate eklendi. | Kamil hâlâ analyzer policy tarafında daha olgun. Warnings-as-errors mevcut EF materialization warning'leri temizlenmeden açılmamalı. |
| Project reference'lar | Kamil implicit project reference ve package reference'ları `Directory.Build.targets` ile conditional package management üzerinden yönetiyor. | LexiLink her `.csproj` içinde explicit project reference kullanıyor. | Kamil çok modüllü yapıda boilerplate azaltıyor; LexiLink discoverability'yi tercih ediyor. | Mevcut boyutta LexiLink'in explicit reference'ları daha iyi. Önce central package version'lar eklenmeli; implicit project-reference magic ancak repo yeterince tekrarlı hale gelirse düşünülmeli. |
| Target framework | Kamil `net8.0` hedefini `Directory.Build.props` içinde merkezi yönetiyor. | LexiLink `net10.0` hedefini root `Directory.Build.props` içinde merkezi yönetiyor. | Kamil'in örneği stable baseline'ı önceliklendiriyor; LexiLink güncel platformu kullanıyor. | Deployment/runtime desteği garanti ise LexiLink `net10.0` üzerinde kalabilir; merkezi yönetim artık hizalı. |
| Integration test execution | Kamil'in integration-test script'i integration test projelerini ortak local integration DB'ye karşı sequential çalıştırıyor. | LexiLink'in `scripts/test.sh` script'i de integration test projelerini sequential çalıştırıyor ve MSBuild node/test DB race'lerini azaltmak için `-m:1` geçiyor. | İki projede de shared local integration database state var ve parallel cleanup race'lerinden kaçınmak gerekiyor. | LexiLink artık Kamil'in pratik yaklaşımıyla hizalı. `-m:1` özellikle Codex/sandbox tarzı ortamlarda faydalı. |
| Authorization/security | Kamil UserAccess, identity setup, permission policy'leri ve permission attribute'ları içeriyor. | LexiLink'te development bearer guard, `ProductionJwt` validation, token issuing boundary, authenticated-player policy, protected endpoint groups ve guest-to-auth coverage var. Gerçek Apple/Google external token verifier credential'lar gelene kadar deferred. | LexiLink hâlâ MVP/internal aşamada; first-party JWT production boundary kuruldu, provider-specific doğrulama dış config/credential gerektiriyor. | Current baseline LexiLink için yeterli. Public mobile release öncesi Apple/Google verifier eklenmeli; geniş permission matrix ancak gerçek authorization ihtiyacı doğarsa açılmalı. |
| Domain/Application içinde execution context | Kamil rule'ların current user kavramına ihtiyaç duyduğu yerlerde member context gibi interface'leri domain model'e daha yakın tutuyor. | LexiLink player/execution context'i şimdilik Application'da tutuyor. | LexiLink domain invariant gerçekten gerektirmedikçe current-user concern'ünü Domain'e taşımıyor. | Şu an LexiLink daha temiz. Context Domain'e sadece domain rule'lar onsuz ifade edilemiyorsa taşınmalı. |
| Aggregate construction ve internal method'lar | İki proje de invariant'ları korumak için private constructor ve internal/private factory kullanıyor. Kamil çoğu zaman construction'ı domain workflow'ları üzerinden yönlendiriyor; LexiLink gereken yerlerde Application/Infrastructure/tests için friend assembly açıyor. | LexiLink module domain project'lerinde explicit `InternalsVisibleTo` kullanıyor. | Ayrı assembly'ler ve direct application factory kullanımı, factory'ler public yapılmadığı veya workflow yeniden tasarlanmadığı sürece friend access gerektiriyor. | LexiLink'in yaklaşımı pragmatik. Domain model construction'ı aggregate workflow içinde tutabiliyorsa Kamil'in yaklaşımı daha temiz. Factory access yeniden tasarlanmadan `InternalsVisibleTo` kaldırılmamalı. |
| DTO stili | Kamil çoğunlukla settable property'li DTO class'ları kullanıyor. | LexiLink query DTO'ları için positional record kullanıyor. | LexiLink daha yeni C# özelliklerini kullanıyor ve read model'leri default immutable tutuyor. | Specific mapper/query shape mutable DTO gerektirmedikçe LexiLink daha iyi. |
| Time abstraction | Kamil özellikle Meetings ve Payments gibi time-sensitive domain alanlarında `SystemClock` kullanıyor; infrastructure path'lerinde yine yer yer `DateTime.UtcNow` var. | LexiLink Common `IClock`/`SystemClock` baseline'ına sahip; Players command timestamp'leri ve async processing metadata clock üzerinden ilerliyor. | Domain-visible zaman kararları ve processor timestamp'leri test edilebilir hale getirildi; domain-event occurrence metadata sade tutuldu. | LexiLink artık bu başlıkta yeterli baseline'a sahip. Yeni time-sensitive policy çıkarsa mevcut `IClock` testlere genişletilmeli. |
| Event sourcing | Kamil'in Payments modülü event-sourced persistence pattern'leri ve checkpoint/subscription infrastructure içeriyor. | LexiLink state persistence + projection/history table kullanıyor. | LexiLink'in mevcut domain'leri ledger-grade event replay gerektirmiyor. | Games/Players/Stats için bugün LexiLink daha iyi. Event sourcing ancak future domain auditability, replay veya temporal reconstruction ihtiyacını gerçekten haklı çıkarıyorsa eklenmeli. |
| Schema/project tooling | Kamil sample ortamı için daha güçlü database project/migration governance'a sahip. | LexiLink SQL structure/migration script'leri ve DbUp tarzı migration execution kullanıyor. | LexiLink'in database tooling'i bilinçli olarak daha basit ve provider'a uygun. | Schema validation ve drift detection tekrar eden problem haline gelmedikçe LexiLink'in mevcut stili korunabilir. |

## Öneriler

### Sıraya Alındı

Planlı Kamil alignment ve ilk production-readiness başlığı kalmadı. 2026-05-14
tarihinde Energy modülü teslim edildi; ilk synchronous cross-module gateway
(`IEnergyGuard`) bilinçli sapma olarak bu dokümanda kayıt altına alındı.
2026-05-15'te Quests modülü kapandı; LexiLink'in ilk reverse cross-module
event dependency'si (`Energy.Application` ↔ `Quests.IntegrationEvents`)
canlandı ve ArchTest'lerde granular allow olarak kilitlendi. Sıradaki öneri
`activeContext.md` içinde tutulan Game Content/Admin Tooling fazıdır.

### LexiLink'in Mevcut Tercihini Koru

- Shared host container şimdilik kalabilir; shared-registration collision'ları
  önlemek için module-owned UoW/dispatcher yaklaşımı sürmeli.
- Direct MediatR `IRequestHandler` decorator'ları korunmalı; çünkü gerçek
  runtime resolution path ile uyumlu.
- Read-model-only Stats modülü doğru.
- Explicit project reference'lar mevcut boyut için daha okunabilir.
- Read DTO'larında record kullanımı iyi.
- Synchronous gateway yerine event-driven module communication tercih edilmeli.

### Kamil'den Alındı

- Central build props ve central package version yönetimi.
- Command/query immutability, handler visibility, validator visibility ve
  forbidden raw MediatR handler usage için application convention
  architecture test'leri.
- API -> module facade dispatch.
- Module startup API'ları.
- Public IntegrationEvents assemblies.
- Serial integration test runner yaklaşımı.
- Local test script'iyle aynı akışı çalıştıran CI quality gate.
- Auth middleware, authenticated execution context ve minimal authorization
  policy baseline'ı. Gerçek external token doğrulama hâlâ ayrı production işi.
- Outbox retry/error tracking baseline'ı: `RetryCount`, `NextRetryDate`,
  persisted `Error`, retry eligibility ve partial-failure test coverage.
- Quartz hosted outbox scheduler: handwritten hosted polling loop kaldırıldı.
- Stats raw Inbox modeli: serialized `InboxMessages`, ayrı processor, Quartz
  job, retry/error metadata ve duplicate/failure integration testleri.
- Stats internal commands baseline'ı: `stats.InternalCommands`, scheduler,
  processor, `ProcessStatsInboxCommand`, retry/error metadata ve architecture
  convention testi.
- Event bus abstraction baseline'ı: `IEventsBus`,
  `IIntegrationEventHandler<T>`, in-process bus implementation, MediatR'dan
  bağımsız public integration event contract'ları.
- Module composition isolation guard'ları: shared container korunur; module-owned
  common service leakage ve event bus scope capture architecture tests ile
  kilitlenir.
- Time abstraction baseline'ı: Common `IClock`/`SystemClock`, Players
  time-dependent command decisions, outbox/inbox/internal-command
  processing/retry timestamps ve sabit clock unit testleri.

### İhtiyaç Oluşunca Kamil'den Al

- Internal commands/recurring jobs için Quartz veya daha durable scheduler
  genişletmesi.
- External broker destekli gerçek event bus implementation'ı.
- Somut registration collision dönerse per-module composition root veya
  per-execution module scope.
- Yeni time-sensitive domain policy çıkarsa mevcut `IClock` kullanımını o
  policy'nin testlerine genişlet.

### Körlemesine Kopyalama

- Kamil'in custom command-handler decorator split'i, LexiLink'te custom handler
  interface'lerinin her zaman resolved MediatR path olduğu kanıtlanmadan
  kopyalanmamalı.
- Implicit `Directory.Build.targets` project reference'ları dependency'leri
  gizleyebilir. Büyük ölçekte faydalı olabilir; bugün LexiLink için explicit
  `.csproj` reference'ları daha açık.
- Event sourcing sadece sample'da var diye eklenmemeli. Gerçek domain ihtiyacına
  bağlanmalı.

## Kaynak Notları

Kamil referans dosyaları
[`kgrzybek/modular-monolith-with-ddd`](https://github.com/kgrzybek/modular-monolith-with-ddd)
reposundan incelendi:

- [`MeetingsStartup.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/Configuration/MeetingsStartup.cs)
- [`MeetingsCompositionRoot.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/Configuration/MeetingsCompositionRoot.cs)
- [`MeetingsModule.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/MeetingsModule.cs)
- [`ProcessingModule.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/Configuration/Processing/ProcessingModule.cs)
- [`DomainEventsDispatcher.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/BuildingBlocks/Infrastructure/DomainEventsDispatching/DomainEventsDispatcher.cs)
- [`ProcessOutboxCommandHandler.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/Configuration/Processing/Outbox/ProcessOutboxCommandHandler.cs)
- [`ProcessInboxCommandHandler.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/Configuration/Processing/Inbox/ProcessInboxCommandHandler.cs)
- [`CommandsScheduler.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/Configuration/Processing/InternalCommands/CommandsScheduler.cs)
- [`IntegrationEventGenericHandler.cs`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Modules/Meetings/Infrastructure/Configuration/EventsBus/IntegrationEventGenericHandler.cs)
- [`Directory.Build.props`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Directory.Build.props)
- [`Directory.Build.targets`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Directory.Build.targets)
- [`Directory.Packages.props`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/src/Directory.Packages.props)
- [`runIntegrationTests.cmd`](https://github.com/kgrzybek/modular-monolith-with-ddd/blob/master/runIntegrationTests.cmd)

Yerelde incelenen LexiLink referans dosyaları:

- `src/API/LexiLink.API/Program.cs`
- `src/Modules/Games/Infrastructure/Configuration/GamesStartup.cs`
- `src/Modules/Games/Infrastructure/Configuration/GamesAutofacModule.cs`
- `src/Modules/Games/Infrastructure/Configuration/GamesModule.cs`
- `src/Modules/Games/Infrastructure/Configuration/Processing/GamesUnitOfWork.cs`
- `src/Modules/Games/Infrastructure/Configuration/Processing/GamesDomainEventsDispatcher.cs`
- `src/Common/Infrastructure/Outbox/OutboxProcessor.cs`
- `src/API/LexiLink.API/Configuration/Inbox/ProcessStatsInboxMessagesJob.cs`
- `src/Modules/Stats/Application/PlayerStats/ProcessIntegrationEvents/GameCompletedIntegrationEventHandler.cs`
- `src/Modules/Stats/Infrastructure/Inbox/StatsInboxProcessor.cs`
- `src/Modules/Stats/Infrastructure/Queries/PlayerStatsProjectionUpdater.cs`
- `src/Tests/ArchitectureTests/LayerDependencyTests.cs`
- `scripts/test.sh`
