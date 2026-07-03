# Category Generation Prompt

Use this prompt once per category. Replace the category name, description, and
subthemes before sending it to an AI model.

Target content standard:

- At least 250 unique links per category.
- Every link must have at least 6 outgoing edges.
- At least 1500 directed edges total.
- Ideal directed edge count: 1700-2300.
- Language: `tr-TR`.
- Output must be valid JSON matching `lexilink/category/v1`.

## Prompt

```text
Sen LexiLink adlı bir kavram bağlantı oyunu için içerik üretiyorsun.

Oyun mantığı:
- İçerik bir directed graph'tır.
- Her link/kavram oyuncunun göreceği bir düğümdür.
- Her edge, oyuncunun bir kavramdan diğer kavrama geçebileceği yönlü bağlantıdır.
- Edge şu anlama gelir: "Bu kavramdan zihinsel olarak şu kavrama doğal geçiş yapılabilir."

Kategori:
- Ad: SPOR
- Dil: tr-TR
- Açıklama: Spor dalları, sporcular, turnuvalar, kurallar, ekipmanlar ve spor kültürüyle ilgili kavram bağlantıları.

Zorunlu çıktı formatı:
Sadece geçerli JSON döndür. Markdown, açıklama, yorum, ```json bloğu kullanma.

JSON schema:
{
  "$schema": "lexilink/category/v1",
  "category": {
    "name": "Spor",
    "description": "Spor dalları, sporcular, turnuvalar, kurallar, ekipmanlar ve spor kültürüyle ilgili kavram bağlantıları.",
    "language": "tr-TR"
  },
  "links": [
    {
      "value": "Futbol",
      "description": "Takımların gol atmak için mücadele ettiği, dünyanın en popüler spor dallarından biridir.",
      "wikipediaUrl": "",
      "depthFromAnchor": -1
    }
  ],
  "edges": [
    { "from": "Futbol", "to": "Gol" }
  ],
  "metadata": {
    "source": "ai-generated",
    "symmetrized": false,
    "nodeCount": 250,
    "minOutgoingPerNode": 6,
    "language": "tr-TR",
    "generatorVersion": "lexilink-content-v1"
  }
}

Kesin kurallar:
1. En az 250 benzersiz link üret.
2. Her links[].value kategori içinde benzersiz olmalı.
3. Her links[].value Türkçe kullanıcıya uygun olmalı.
4. Link değerleri 1-4 kelime arasında olmalı.
5. Çok uzun, belirsiz veya aşırı niş kavramlardan kaçın.
6. Her link için en az 6 outgoing edge üret.
7. Her edge directed kabul edilir.
8. Bir bağlantı iki yönlü mantıklıysa iki ayrı edge yaz:
   { "from": "A", "to": "B" } ve { "from": "B", "to": "A" }
9. Her edge.from ve edge.to mutlaka links[].value içinde bulunmalı.
10. Duplicate edge üretme.
11. Self edge üretme: from ve to aynı olamaz.
12. Graph kopuk olmamalı; tüm kavramlar kategori içinde anlamlı şekilde bağlı olmalı.
13. Her linkin outgoing sayısı en az 6 olmalı.
14. Toplam directed edge sayısı en az 1500 olmalı.
15. İdeal edge sayısı 1700-2300 arasıdır.
16. Bağlantılar anlamsal yakınlığa dayanmalı, rastgele olmamalı.
17. Description alanı 1 kısa cümle olmalı, 500 karakteri geçmemeli.
18. wikipediaUrl her zaman "" olsun.
19. depthFromAnchor her zaman -1 olsun.
20. metadata.nodeCount gerçek links sayısıyla uyumlu olsun.

İçerik dağılımı:
- %60 genel bilinen kavramlar
- %30 orta seviye kavramlar
- %10 özel ama hâlâ anlaşılabilir kavramlar

Kategori alt temalarını dengeli kullan:
- Futbol
- Basketbol
- Tenis
- Voleybol
- Formula 1
- Olimpiyatlar
- Atletizm
- Sporcular
- Kurallar ve terimler
- Ekipmanlar ve mekanlar
- Turnuvalar ve organizasyonlar
- Spor kültürü

Üretmeden önce kendi içinde şu kontrolleri yap:
- links sayısı >= 250 mi?
- link value tekrar ediyor mu?
- edges sayısı >= 1500 mü?
- her linkin outgoing sayısı >= 6 mı?
- edge içinde links listesinde olmayan kavram var mı?
- duplicate edge var mı?
- self edge var mı?
- graph kopuk kalıyor mu?

Bu kontrollerden geçmeyen JSON döndürme. Sadece final, geçerli JSON döndür.
```

## How To Use

1. Copy the prompt.
2. Replace `SPOR`, `Spor`, category description, and subthemes.
3. Ask the AI model to return only JSON.
4. Save the output under `docs/category-<name>.json`.
5. Validate/import it with the category importer.

Recommended Turkish category files:

```text
docs/category-spor.json
docs/category-sinema.json
docs/category-muzik.json
docs/category-bilim.json
docs/category-tarih.json
```

Do not manually merge partial AI outputs. If the JSON is truncated, rerun the
generation from scratch with the same prompt.

## Ready Prompt: Sinema

```text
Sen LexiLink adlı bir kavram bağlantı oyunu için içerik üretiyorsun.

Oyun mantığı:
- İçerik bir directed graph'tır.
- Her link/kavram oyuncunun göreceği bir düğümdür.
- Her edge, oyuncunun bir kavramdan diğer kavrama geçebileceği yönlü bağlantıdır.
- Edge şu anlama gelir: "Bu kavramdan zihinsel olarak şu kavrama doğal geçiş yapılabilir."

Kategori:
- Ad: SİNEMA
- Dil: tr-TR
- Açıklama: Filmler, yönetmenler, oyuncular, film türleri, ödüller ve sinema kültürüyle ilgili kavram bağlantıları.

Zorunlu çıktı formatı:
Sadece geçerli JSON döndür. Markdown, açıklama, yorum, ```json bloğu kullanma.

JSON schema:
{
  "$schema": "lexilink/category/v1",
  "category": {
    "name": "Sinema",
    "description": "Filmler, yönetmenler, oyuncular, film türleri, ödüller ve sinema kültürüyle ilgili kavram bağlantıları.",
    "language": "tr-TR"
  },
  "links": [
    {
      "value": "Film",
      "description": "Hareketli görüntülerle anlatılan kurmaca ya da belgesel görsel anlatı biçimidir.",
      "wikipediaUrl": "",
      "depthFromAnchor": -1
    }
  ],
  "edges": [
    { "from": "Film", "to": "Yönetmen" }
  ],
  "metadata": {
    "source": "ai-generated",
    "symmetrized": false,
    "nodeCount": 250,
    "minOutgoingPerNode": 6,
    "language": "tr-TR",
    "generatorVersion": "lexilink-content-v1"
  }
}

Kesin kurallar:
1. En az 250 benzersiz link üret.
2. Her links[].value kategori içinde benzersiz olmalı.
3. Her links[].value Türkçe kullanıcıya uygun olmalı.
4. Link değerleri 1-4 kelime arasında olmalı.
5. Çok uzun, belirsiz veya aşırı niş kavramlardan kaçın.
6. Her link için en az 6 outgoing edge üret.
7. Her edge directed kabul edilir.
8. Bir bağlantı iki yönlü mantıklıysa iki ayrı edge yaz:
   { "from": "A", "to": "B" } ve { "from": "B", "to": "A" }
9. Her edge.from ve edge.to mutlaka links[].value içinde bulunmalı.
10. Duplicate edge üretme.
11. Self edge üretme: from ve to aynı olamaz.
12. Graph kopuk olmamalı; tüm kavramlar kategori içinde anlamlı şekilde bağlı olmalı.
13. Her linkin outgoing sayısı en az 6 olmalı.
14. Toplam directed edge sayısı en az 1500 olmalı.
15. İdeal edge sayısı 1700-2300 arasıdır.
16. Bağlantılar anlamsal yakınlığa dayanmalı, rastgele olmamalı.
17. Description alanı 1 kısa cümle olmalı, 500 karakteri geçmemeli.
18. wikipediaUrl her zaman "" olsun.
19. depthFromAnchor her zaman -1 olsun.
20. metadata.nodeCount gerçek links sayısıyla uyumlu olsun.

İçerik dağılımı:
- %60 genel bilinen kavramlar
- %30 orta seviye kavramlar
- %10 özel ama hâlâ anlaşılabilir kavramlar

Kategori alt temalarını dengeli kullan:
- Film türleri
- Dünya sineması
- Türk sineması
- Yönetmenler
- Oyuncular
- Senaryo ve anlatı
- Kamera ve görüntü
- Kurgu ve post prodüksiyon
- Film müzikleri
- Ödüller ve festivaller
- Sinema salonu ve izleme kültürü
- Animasyon ve görsel efektler
- Klasik filmler
- Modern popüler filmler
- Sinema terimleri

Bağlantı örnekleri:
- Film -> Yönetmen
- Yönetmen -> Senaryo
- Senaryo -> Karakter
- Karakter -> Oyuncu
- Oyuncu -> Oscar
- Oscar -> Akademi Ödülleri
- Festival -> Cannes Film Festivali
- Korku Filmi -> Gerilim
- Bilim Kurgu -> Uzay
- Animasyon -> Seslendirme

Üretmeden önce kendi içinde şu kontrolleri yap:
- links sayısı >= 250 mi?
- link value tekrar ediyor mu?
- edges sayısı >= 1500 mü?
- her linkin outgoing sayısı >= 6 mı?
- edge içinde links listesinde olmayan kavram var mı?
- duplicate edge var mı?
- self edge var mı?
- graph kopuk kalıyor mu?

Bu kontrollerden geçmeyen JSON döndürme. Sadece final, geçerli JSON döndür.
```
