# iOS Release Final Audit - 2026-06-28

Bu dokuman iOS yayin oncesi son kontrol icin olusturuldu.

Kapsam:
- Flutter iOS uygulamasi test/analyze/build kontrolleri
- Debug/log/TODO/FIXME ve release riski tasiyan kod taramalari
- Null safety/null warning ve riskli null kullanim taramalari
- Kullanilmayan veya fazla asset/dosya adaylari
- Yayina cikmadan once duzeltilmesi gereken eksik/bug adaylari

Notlar:
- Bu audit sirasinda kod davranisi degistirilmedi.
- Sadece bu rapor dosyasi eklendi/guncellendi.
- iOS yayin odakli kontrol yapildi; backend testleri de release riski icin calistirildi.
- Xcode test komutu otomatik olarak `frontend/ios/Runner.xcodeproj/project.pbxproj` dosyasinda kosmetik bir diff olusturdu; bu diff geri alindi. Son `git status --short` yalnizca bu rapor dosyasini gosteriyor.

## Ozet

Durum: Yayina cikmadan once duzeltilmesi gereken bulgular var.

Gecen ana kontroller:
- `flutter test`: basarili, 189 test gecti.
- `flutter build ios --release --no-codesign`: basarili, `build/ios/iphoneos/Runner.app` uretildi, 51.4 MB.
- `xcodebuild test` iPhone 16 / iOS 18.6 simulator: basarili, `RunnerTests.testExample()` gecti.

Kalan kritik/riskli bulgular:
- `flutter analyze` fail ediyor: 1 adet line length info var.
- Full backend `dotnet test LexiLink.sln --no-restore` fail ediyor.
- Quest integration testlerinde `IQuestEnergyRewardGuard` test container kaydi eksik.
- API ready health check, eksik DbUp migration nedeniyle 503 donuyor: `payments/Tables/041_EnsureDiamondBundleProducts.sql`.
- Release uygulamada admin route ve admin login metinleri bulunuyor.
- Payment store servisinde release'te StoreKit urun cevabini loglayabilecek `debugPrint` satirlari var.
- `docs/ses` altinda app assetleriyle birebir ayni ses dosyalari ikinci kopya olarak tracked duruyor.

## Ortam

- Tarih: 2026-06-28
- Flutter: 3.41.9 stable
- Dart: 3.11.5
- Xcode workspace: `frontend/ios/Runner.xcworkspace`
- Test simulator: iPhone 16, iOS 18.6
- Bundle id: `com.wordlope.app`
- App display name: `WordLope`
- App version: `1.0.0+1`
- Release API base URL: `https://api.wordlope.com`
- Release AdMob app id: `ca-app-pub-2115638398802394~7914746084`
- Release rewarded ad unit id: `ca-app-pub-2115638398802394/3077352370`
- Release interstitial ad unit id: `ca-app-pub-2115638398802394/4516380950`

## Test Sonuclari

### Flutter / iOS

`flutter pub get`
- Sonuc: Basarili.
- Not: 44 paket icin constraint disinda daha yeni surum var. Cikti: `44 packages have newer versions incompatible with dependency constraints.`

`flutter analyze`
- Sonuc: Basarisiz.
- Bulgu:
  - `lib/features/payments/data/payment_store_service.dart:105:81`
  - Lint: `lines_longer_than_80_chars`
- Null safety hatasi veya fatal analyzer hatasi gorulmedi.

`flutter test`
- Sonuc: Basarili.
- Ozet: `All tests passed`, toplam 189 test.

`flutter build ios --release --no-codesign`
- Sonuc: Basarili.
- Ozet: `Built build/ios/iphoneos/Runner.app (51.4MB)`.
- Not: Kod imzasi kapali oldugu icin App Store signing/provisioning dogrulamaz.

`xcodebuild -list -workspace Runner.xcworkspace`
- Sonuc: Basarili.
- `Runner` scheme mevcut.

`xcrun simctl list devices available`
- Sonuc: Basarili.
- iOS 18.6 ve iOS 26.5 simulator runtime'lari goruldu.

`xcodebuild test -workspace Runner.xcworkspace -scheme Runner -destination 'platform=iOS Simulator,name=iPhone 16,OS=18.6'`
- Sonuc: Basarili.
- Ozet: `** TEST SUCCEEDED **`
- Test: `RunnerTests.testExample()` gecti.
- Not: Bu XCTest hedefi su anda default/placeholder test iceriyor; uygulama akisini gercek cihazda test etmiyor.
- Uyarilar:
  - Birden fazla matching destination var; Xcode ilkini kullandi.
  - Pod target'larinda `IPHONEOS_DEPLOYMENT_TARGET` 9.0/10.0/11.0, desteklenen aralik 12.0-26.5.99 uyarilari var.
  - `Thin Binary` ve `Run Script` build phase'leri dependency analysis kapali oldugu icin her build'de calisiyor.

### Backend / Full Solution

`dotnet test LexiLink.sln --no-restore`
- Sonuc: Basarisiz.
- Full suite calistirildi; cikti buyuk oldugu icin failing projeler ayrica minimal verbosity ile tekrar calistirildi.

`dotnet test src/Modules/Quests/IntegrationTests/LexiLink.Modules.Quests.IntegrationTests.csproj --no-restore --logger "console;verbosity=minimal"`
- Sonuc: Basarisiz.
- Ozet: 3 failed, 13 passed, 16 total.
- Fail listesi:
  - `ClaimQuest_AtThreshold_QueuesQuestClaimedOutboxNotification`
  - `ClaimQuest_BelowThreshold_BreaksBusinessRule`
  - `GetActiveQuests_PrereqClaimed_IssuesDownstream`
- Kök neden:
  - `ClaimQuestCommandHandler` constructor'i `IQuestEnergyRewardGuard` istiyor.
  - Quests integration test container bu interface'i register etmiyor.
  - Hata: `Cannot resolve parameter 'IQuestEnergyRewardGuard energyRewardGuard'`.
- Yayin riski:
  - API runtime registration mevcut olsa bile integration test kirmizi. Test container ve/veya modül composition eksikligi duzeltilmeden temiz release denemez.

`dotnet test src/API/LexiLink.API.Tests/LexiLink.API.Tests.csproj --no-restore --logger "console;verbosity=minimal"`
- Sonuc: Basarisiz.
- Ozet: 1 failed, 59 passed, 60 total.
- Fail:
  - `ReadyHealthCheck_VerifiesPostgreSqlConnectivity`
- Kök neden:
  - PostgreSQL reachable ama migration health check unhealthy.
  - Eksik script: `payments/Tables/041_EnsureDiamondBundleProducts.sql`
  - Expected scripts: 85, applied scripts: 84.
- Yayin riski:
  - Production ready endpoint 503 doner. Deploy oncesi migration uygulanmali veya migration tracking problemi duzeltilmeli.

Backend build/test ciktisinda gorulen warning grubu:
- `src/Modules/Games/Domain/...` icinde cok sayida `CS8618` nullability warning var.
- Ornekler:
  - `Category.cs`: `Id`, `_name`, `_description`, `_language`
  - `Link.cs`: `Id`, `_categoryId`, `_value`, `_description`
  - `Game.cs`: `_puzzle`, `Id`, `_currentLinkId`, `_stepBudget`, `_hintAllowance`
  - `Puzzle.cs`: `CategoryId`, `StartLinkId`, `TargetLinkId`
- Not: Bunlar genelde EF private constructor pattern'inden geliyor olabilir; yine de release oncesi ya bilincli suppress edilmeli ya da null-forgiving/default pattern standardize edilmeli.

## Kod Taramasi

### Debug / Log Bulgulari

`frontend/lib/features/payments/data/payment_store_service.dart`
- Satirlar: 102-108
- Bulgu: `_debugPrintProductResponse` StoreKit urun response detaylarini `debugPrint` ile yaziyor.
- Risk:
  - Release build'de `debugPrint` tamamen no-op degil; store product id, title, price, notFoundIDs ve error loglanabilir.
- Oncelik: Yuksek.

`frontend/lib/shared/ads/mobile_ads_platform.dart`
- Satirlar: 88, 101, 117, 127
- Bulgu: AdMob load/show hatalari ve rewarded adUnitId `debugPrint` ile yaziliyor.
- Risk:
  - Reklam unit id ve hata detaylari release loglarina gidebilir.
- Oncelik: Orta.

`src/Modules/Players/Infrastructure/*Context.cs` ve diger EF context dosyalari
- Bulgu: `EnableSensitiveDataLogging()` satirlari comment olarak duruyor.
- Risk:
  - Aktif degil; dogrudan release riski yok.
- Oncelik: Dusuk.

### Dev / Production Konfigurasyon Bulgulari

`frontend/lib/features/auth/data/guest_player_repository.dart`
- Satir: 46
- Bulgu: Guest token exchange body icinde `externalToken: 'dev:Guest:$deviceId'` gonderiliyor.
- Risk:
  - Backend production token exchange `GuestExternalIdentityVerifier` kabul edecek sekilde ayarli degilse iOS guest girisi kirilir.
  - Backend `DevelopmentExternalToken` production'da validator tarafindan engelleniyor. Bu nedenle production ortaminda guest auth modunun kesin dogrulanmasi gerekiyor.
- Oncelik: Yuksek.

`frontend/lib/features/admin_auth/data/admin_auth_repository.dart` ve l10n metinleri
- Bulgu: Admin login akisi `dev:admin:<email>` metniyle uygulamada duruyor.
- Ornek metinler:
  - `lib/l10n/app_en.arb`
  - `lib/l10n/app_tr.arb`
  - Generated `app_localizations_*.dart`
- Risk:
  - App Store'a giden player uygulamasinda admin/development login bilgisi gorunebilir.
- Oncelik: Yuksek.

`frontend/lib/app/router/app_router.dart`
- Bulgu: `/admin/*` route'lari release uygulamaya dahil.
- Risk:
  - Admin ekranlari public iOS binary icinde. Token guard var ama route ve UI varligi release icin urun/güvenlik karari gerektirir.
- Oncelik: Yuksek.

`frontend/lib/shared/api/api_config.dart`
- Bulgu: Non-web default base URL `https://api.wordlope.com`; web default `http://127.0.0.1:5000`.
- iOS icin dogru gorunuyor.
- Not: Release xcconfig ayrica `LEXILINK_API_BASE_URL=https://api.wordlope.com` dart-define iceriyor.

`frontend/lib/shared/ads/ad_config.dart`
- Bulgu: Kod fallback'i Google test ad unit id'leri.
- Release xcconfig gercek ad unit id dart-define iceriyor.
- Risk:
  - CI veya Xcode archive bu `Release.xcconfig` dart define'larini kullanmadan build alirsa test ad unit id'leriyle release cikabilir.
- Oncelik: Orta.

`src/API/LexiLink.API/appsettings.json`
- Bulgu: Default `Authentication:Mode` `DevelopmentBearer`.
- Validator production'da bunu engelliyor.
- Risk:
  - Production env override eksikse API startup fail eder. Fail-fast iyi, ama deploy oncesi env dosyasi kesin kontrol edilmeli.
- Oncelik: Orta.

### Null / Nullable Bulgulari

Flutter:
- `flutter analyze` ciktisinda null safety/null warning bulunmadi.
- Kodda null assertion (`!`) kullanimlari var; analyzer bunlari hata olarak isaretlemedi.
- Riskli ama intentional gorunen ornekler:
  - `app_router.dart`: `state.pathParameters['gameId']!`
  - `splash_screen.dart`: `lastError!`, `Offset.lerp(...)!`
  - Admin/presentation ekranlarinda state snapshot `!` kullanimi

Backend:
- Full `dotnet test` build asamasinda `CS8618` nullability warning'leri var.
- En yogun grup `src/Modules/Games/Domain`.
- EF private constructor pattern'i olabilir; yine de warning-free release icin temizlenmeli veya bilincli suppress edilmeli.

## Asset ve Fazla Dosya Taramasi

### Flutter App Assets

`frontend/assets/audio`
- Toplam: yaklasik 13 MB.
- `pubspec.yaml` icinde paketleniyor:
  - `assets/audio/sfx/`
  - `assets/audio/music/`
- Taramada tum ses dosyalari kod/test tarafinda referansli gorundu.
- Silme adayi olarak isaretlenmedi.

Buyuk app assetleri:
- `frontend/assets/audio/music/game.wav`: 5.5 MB
- `frontend/assets/audio/music/menu.wav`: 5.5 MB
- `frontend/assets/audio/sfx`: toplam 1.9 MB
- Not: iOS app build icinde bu iki music wav dosyasi paketleniyor. Boyut azaltmak istenirse wav yerine sikistirilmis format degerlendirilebilir.

### Repo Icindeki Fazla / Duplicate Adaylari

`docs/ses`
- Toplam: yaklasik 13 MB.
- `frontend/assets/audio` altindaki seslerle SHA-256 olarak birebir ayni dosyalar.
- Git tarafindan tracked.
- App bundle'a girmiyor, ama repo boyutunu sisiriyor.
- Silme/tasima adayi:
  - `docs/ses/game.wav`
  - `docs/ses/menu.wav`
  - `docs/ses/step.wav`
  - `docs/ses/win.wav`
  - `docs/ses/button_tap.wav`
  - `docs/ses/lose.wav`
  - `docs/ses/reset.wav`
  - `docs/ses/quest_claim.wav`
  - `docs/ses/purchase.wav`
  - `docs/ses/error.wav`
  - `docs/ses/hint.wav`
  - `docs/ses/undo.wav`

`docs/ChatGPT Image 27 Haz 2026 16_19_25.png`
- Boyut: yaklasik 1.4 MB.
- Kod/dokuman referansi bulunmadi.
- Git tarafindan tracked.
- Silme/tasima adayi.

Generated/cache dosyalari:
- `frontend/build`, `frontend/.dart_tool`, `frontend/android/.gradle` altinda cok sayida buyuk generated dosya var.
- `git ls-files` bu klasorlerde tracked dosya gostermedi.
- Yani repo'ya commitli gorunmuyorlar; local disk temizligi icin `flutter clean` / cache temizligi sonra yapilabilir.
- Not: Kullanici "hicbir degisiklik yapma" dedigi icin bu dosyalar temizlenmedi.

### Tracked App Assetleri

Git tarafindan tracked olan app assetleri:
- `frontend/assets/audio/README.md`
- `frontend/assets/audio/music/game.wav`
- `frontend/assets/audio/music/menu.wav`
- `frontend/assets/audio/sfx/*.wav`

Bu dosyalar uygulama tarafinda kullaniliyor.

## Yayin Oncesi Risk Listesi

Yuksek oncelik:
- Backend full test suite kirmizi: Quests integration DI kaydi eksik.
- API ready health check 503: `payments/Tables/041_EnsureDiamondBundleProducts.sql` migration uygulanmamis gorunuyor.
- iOS player app icinde admin route ve development admin login metinleri var.
- Guest login frontend'i `dev:Guest:<deviceId>` external token uretiyor; production backend token exchange modu kesin dogrulanmali.
- Payment store response debug loglari release'e kalmis.

Orta oncelik:
- `flutter analyze` 1 lint nedeniyle fail ediyor.
- Pod deployment target warning'leri var.
- Release build komutu `--no-codesign`; App Store signing/provisioning henuz bu audit kapsaminda dogrulanmadi.
- `ad_config.dart` fallback olarak test ad unit id'leri kullaniyor; release xcconfig dogru ama build pipeline bunu her zaman kullaniyor mu dogrulanmali.
- Package update notu: 44 paket icin constraint disinda yeni surum var.

Dusuk oncelik / temizlik:
- `docs/ses` duplicate ses dosyalari repo'da 13 MB yer kapliyor.
- `docs/ChatGPT Image 27 Haz 2026 16_19_25.png` referanssiz gorunuyor.
- Backend EF private constructor kaynakli `CS8618` warning'leri warning-free release hedefi icin temizlenmeli.

## Calistirilan Komutlar

```bash
flutter --version
flutter pub get
flutter analyze
flutter test
flutter build ios --release --no-codesign
xcodebuild -list -workspace Runner.xcworkspace
xcrun simctl list devices available
xcodebuild test -workspace Runner.xcworkspace -scheme Runner -destination 'platform=iOS Simulator,name=iPhone 16,OS=18.6'
dotnet test LexiLink.sln --no-restore
dotnet test src/Modules/Quests/IntegrationTests/LexiLink.Modules.Quests.IntegrationTests.csproj --no-restore --logger "console;verbosity=minimal"
dotnet test src/API/LexiLink.API.Tests/LexiLink.API.Tests.csproj --no-restore --logger "console;verbosity=minimal"
git diff --check
git status --short
```

## Son Durum

Son `git status --short`:

```text
?? docs/IOS_RELEASE_FINAL_AUDIT_2026-06-28.md
```

Son `git diff --check`:

```text
Temiz.
```
