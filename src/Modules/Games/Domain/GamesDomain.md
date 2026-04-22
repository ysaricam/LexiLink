# Games Domain Modeli (GamesDomain.md)

Bu döküman, LexiLink projesinin **Games** modülüne ait çekirdek iş mantığını, domain kurallarını ve mimari yapısını özetler. Tasarım, *Domain-Driven Design (DDD)* pratiklerine ve *Modular Monolith* prensiplerine tam uyumludur.

## Bounded Context (Sınırlı Bağlam)
Games modülü, kullanıcıların belirli kategoriler altındaki kelime zincirlerini (Links) takip ederek, başlangıç noktasından hedef kelimeye en verimli şekilde ulaşmaya çalıştığı oyun oturumlarını yönetir.

##  Aggregates & Repositories

Tüm Agregatelar `AggregateRoot` sınıfından türetilir ve kalıcılık işlemleri için sadece kendi ID'lerine özel "Lean Repository" arayüzlerini kullanırlar.

### 1. Game (Oyun)
Oyunun durumunu, puanlamasını ve hamle geçmişini yöneten merkezi agregatedır.
- **Root Entity:** `Game` | **Identity:** `GameId` | **Repository:** `IGameRepository`
- **Metotlar (Internal/Public):**
  - `CreateNew(...)`: Yeni bir oyun oturumu oluşturur (Internal Factory).
  - `Start()`: Hedef yolu çözer, oyunu başlatır ve `GameStartedDomainEvent` fırlatır.
  - `MakeStep(...)`: Hamle yapar, kombo takibi yapar ve `GameStepMadeDomainEvent` fırlatır.
  - `GetHint()`: İpucu üretir ve `GameHintUsedDomainEvent` fırlatır.
  - `UndoMove()`: Hamleyi geri alır ve `GameMoveUndoneDomainEvent` fırlatır.
  - `ResetToStart()`: Başlangıca döner ve `GameResetToStartDomainEvent` fırlatır.

### 2. Link (Bağlantı/Kelime)
Hiyerarşik kelime ağacını temsil eden agregatedır.
- **Root Entity:** `Link` | **Identity:** `LinkId` | **Repository:** `ILinkRepository`
- **Metotlar:**
  - `CreateNew(value, subLinkIds)`: Yeni link oluşturur ve `LinkCreatedDomainEvent` fırlatır.

### 3. GameCategory (Kategori)
Oyunların mantıksal gruplandırılmasını sağlar.
- **Root Entity:** `GameCategory` | **Identity:** `GameCategoryId` | **Repository:** `IGameCategoryRepository`
- **Metotlar:**
  - `CreateNew(name, description)`: Kategori oluşturur ve `GameCategoryCreatedDomainEvent` fırlatır.

## Domain Events (Olaylar)
Sistemdeki durum değişiklikleri aşağıdaki olaylar ile diğer modüllere duyurulur:

- **GameCreatedDomainEvent**: Oyun nesnesi ilk oluşturulduğunda.
- **GameStartedDomainEvent**: Oyun başlatıldığında (Zorluk ve hedef bilgileri içerir).
- **GameStepMadeDomainEvent**: Her hamlede (Doğruluk ve adım numarası içerir).
- **GameCompletedDomainEvent**: Hedef ulaşıldığında (Final skoru içerir).
- **GameFailedDomainEvent**: Başarısızlık durumunda (Hata sebebi içerir).
- **GameHintUsedDomainEvent**: İpucu kullanımında (Kullanılan kelime ve adım bilgisi içerir).
- **GameMoveUndoneDomainEvent**: Geri alma yapıldığında.
- **GameResetToStartDomainEvent**: Başlangıca dönüldüğünde.
- **LinkCreatedDomainEvent**: Yeni bir kelime sisteme tanımlandığında.
- **GameCategoryCreatedDomainEvent**: Yeni bir kategori tanımlandığında.

## Değer Nesneleri (Value Objects)
- **ScoreValue:** Puanlama değerini sarmalar ve negatif değerleri engeller.
- **HintResult:** İpucu sonucunu (Mesaj, Öneri, Durum) sarmalar.
- **TypedIdValueBase:** Tüm kimlikler (GameId, LinkId vb.) için tip güvenli temel sınıf.

## İş Kuralları (Business Rules)
- `GameMustBeInSpecificStateRule`: Geçersiz durum geçişlerini engeller.
- `NextStepMustBeSubLinkOfCurrentRule`: Geçersiz kelime bağlantılarını denetler.
- `GameCategoryNameCannotBeEmptyRule`: Kategori adının zorunluluğunu denetler.
- `LinkCannotBeEmptyRule`: Kelime metninin zorunluluğunu denetler.
- `Undo/Reset/Hint Limit Rules`: Kaynak kullanım sınırlarını denetler.

## Mimari Standartlar
- **Encapsulation:** Factory metotlar (`CreateNew`) modül sınırlarını korumak için `internal` tanımlanmıştır.
- **ID-Only References:** Agregatelar birbirine nesne referansı yerine sadece `ID` ile bağlıdır.
- **Observability:** Tüm kritik domain değişimleri zengin verili olaylarla takip edilir.
- **Zero Warning Policy:** Derleme uyarıları ve nullability sorunları tamamen giderilmiştir.
