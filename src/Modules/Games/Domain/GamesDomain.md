# Games Domain Modeli (GamesDomain.md)

Bu döküman, LexiLink projesinin **Games** modülüne ait çekirdek iş mantığını, domain kurallarını ve mimari yapısını özetler. Tasarım, *Domain-Driven Design (DDD)* pratiklerine ve *Modular Monolith* prensiplerine tam uyumludur.

## Bounded Context (Sınırlı Bağlam)
Games modülü, kullanıcıların belirli kategoriler altındaki kelime zincirlerini (Links) takip ederek, başlangıç noktasından hedef kelimeye en verimli şekilde ulaşmaya çalıştığı oyun oturumlarını yönetir.

##  Aggregates

### 1. Game (Oyun)
Oyunun durumunu, puanlamasını ve hamle geçmişini yöneten merkezi agregatedır.
- **Root Entity:** `Game`
- **Identity:** `GameId`
- **Durum Yönetimi:** `NotStarted`, `InProgress`, `Completed`, `Failed`, `TimedOut`.

#### Metotlar (Public Interface):
- `Create(...)`: Yeni bir oyun oturumu oluşturur (Statik Factory).
- `Start()`: Hedef yolu çözer ve oyunu `InProgress` durumuna sokar.
- `MakeStep(nextLinkId, availableSubIds)`: Bir sonraki kelimeye geçer, kombo takibi yapar ve hedef kontrolü gerçekleştirir.
- `GetHint()`: Mevcut yola göre ipucu üretir ve gerekiyorsa oyuncuyu doğru yola yönlendirir (Redirect).
- `UndoMove()`: Yapılan son hamleyi geri alır (Limit dahilinde).
- `ResetToStart()`: Tüm ilerlemeyi sıfırlayıp başlangıca döndürür (Limit dahilinde).
- `Fail()`: Oyunu başarısız olarak sonlandırır.
- `Timeout()`: Zaman aşımı durumunda oyunu sonlandırır.

### 2. Link (Bağlantı/Kelime)
Hiyerarşik kelime ağacını temsil eden bağımsız agregatedır.
- **Root Entity:** `Link`
- **Identity:** `LinkId`
- **Metotlar:**
  - `Of(value, subLinkIds)`: Değer ve alt bağlantı ID'leri ile nesne oluşturur.
- **İlişkiler:** Nesne referansı yerine sadece `LinkId` listesi (`SubLinkIds`) tutulur.

### 3. GameCategory (Kategori)
Oyunların mantıksal gruplandırılmasını sağlar.
- **Root Entity:** `GameCategory`
- **Identity:** `GameCategoryId`
- **Metotlar:**
  - `Create(name, description)`: Yeni kategori oluşturur.

## Değer Nesneleri (Value Objects)
- **ScoreValue:** Puanlama değerini sarmalar. `Add()` metodu ile immutability korunarak artırım yapılabilir.

## İş Kuralları (Business Rules)
İş kuralları `CheckRule()` mekanizmasıyla doğrulanır.

### Oyun Kuralları (Games):
- `GameMustBeInSpecificStateRule`: İşlemin oyun durumuna uygunluğunu denetler.
- `NextStepMustBeSubLinkOfCurrentRule`: Geçersiz kelime geçişlerini engeller.
- `GameStartLinkCannotBeNullRule`: Başlangıç noktasız oyun kurulamaz.
- `GameHasHistoryToUndoRule`: Geçmiş yoksa geri alma yapılamaz.
- `UndoLimitReachedRule`: Maksimum geri alma sayısını (3) denetler.
- `ResetLimitReachedRule`: Maksimum sıfırlama sayısını (1) denetler.
- `HintLimitReachedRule`: Maksimum ipucu sayısını (2) denetler.

### Diğer Kurallar:
- `LinkCannotBeEmptyRule`: Kelime metni boş veya sadece boşluk olamaz.
- `ScoreCannotBeNegativeRule`: Skor değeri asla negatif olamaz.

## Domain Servisleri
- **IScoreCalculator:** `Calculate(targetDepth, steps, maxSteps, combo)` metodu ile skor üretir.
- **ITargetLinkResolver:** `Resolve(startId, depth)` metodu ile hedef yol mimarisini kurar.

## Mimari Standartlar
- **Modular Monolith:** Agregatelar birbirine sadece `ID` ile bağlıdır.
- **Rich Domain Model:** Mantık (Logic) entity'lerin içinde toplanmıştır.
- **Always Valid:** Nesneler her zaman geçerli bir durumda tutulur.
