# frontendActiveContext.md

Frontend'in o anki yonu ve en yakin sira. Backend tarafinin aktif hafizasi
`activeContext.md`, frontend teslim gecmisi `frontendProgress.md`, frontend
plani `frontendRoadmap.md` icindedir.

> Last updated: 2026-05-23 (Admin F1–F6 closed; manual-test stack stabilized; Sprint Q1 frontend reshape **planned, not yet started**)

---

## Active Direction

Backend tarafinda Kamil Grzybek'in modular-monolith yaklasimi referans alindi.
Frontend tarafinda idol/referans cizgisi:

1. **Flutter official app architecture** — temel ayrim: UI layer, data layer,
   repositories, services, view models/use-cases ihtiyaca gore.
2. **Very Good Ventures / Very Good CLI** — production-grade Flutter proje
   disiplini: scalable template, flavors, lint, tests, CI, localization.
3. **Bloc/Cubit architecture** — presentation, business logic ve data katmanini
   net ayirma; UI state'ini deterministik ve test edilebilir tutma.

Bu uclu, LexiLink frontend'in Kamil karsiligi olarak kabul edilecek. Birebir
kopyalama degil; karar ve dosya duzeni bu prensiplere gore alinacak.

---

## Product Target

LexiLink frontend tek codebase ile su platformlari hedefler:

- iOS
- Android
- Web browser

Ilk Flutter iskeleti tamamlandi. Güncel frontend hedefi gerçek backend'e bağlı
oynanabilir MVP'yi iOS, Android ve web hedeflerini koruyarak genişletmektir.

---

## Architecture Principles

- Feature-first folder structure kullan.
- Flutter UI, backend domain'in sahibi degildir; frontend domain modeli minimal
  tutulur.
- Backend module kavrami frontend'de feature'a cevrilir.
- Backend command/query dusuncesi frontend'de Cubit/Bloc action + repository
  call olarak modellenir.
- DTO'lar backend contract'ina yakin tutulur; UI modelleri gerekiyorsa ayrilir.
- API client, repository ve UI state birbirine karistirilmaz.
- Global state sadece gercekten global olan auth/session/theme gibi konularda
  tutulur.
- Her yeni feature icin en azindan happy-path Cubit/Bloc testi veya widget
  smoke testi hedeflenir.
- Platform ayrimi gerekiyorsa izole edilir; feature koduna dagitilmaz.

---

## Proposed Structure

```text
frontend/
  lib/
    app/
      app.dart
      router.dart
      theme/
    features/
      auth/
        presentation/
        application/
        data/
      categories/
        presentation/
        application/
        data/
      game/
        presentation/
        application/
        data/
      profile/
        presentation/
        application/
        data/
    shared/
      api/
      errors/
      widgets/
      storage/
```

Bu yapi repo gercegine gore kucultulebilir. Leaderboard şimdilik Stats/Profile
surface'inin parcasi olarak `features/profile` altinda ilerler; ayrı feature'a
ancak ekran/ownership büyürse ayrılır. Ana prensip feature ownership ve katman
ayrimidir.

---

## Active Constraints

- Flutter + Dart secimi kabul edildi.
- iOS/Android/web destekleri ilk gunden korunacak.
- UI tarafinda VGV/Bloc disiplini izlenecek; rastgele state management
  eklenmeyecek.
- Backend contract'lari degistirmek gerekirse once backend dokumanlari ve API
  contract netlestirilecek.
- Baslangicta gercek backend'e baglanma zorunlu degil; fake repository ile UI
  akisi kurulabilir.
- Frontend bilgisi sifirdan ilerleyecegi icin her slice kucuk, calisir ve
  aciklanabilir olacak.

---

## Next Action

Son tamamlanan frontend implementation slice:

**Frontend Bootstrap** — Flutter toolchain kuruldu, iOS/Android/web platform
scaffold'u uretildi, VGV/Bloc prensiplerine uygun app/router/theme kabugu ve
ilk Bootstrap feature'i eklendi. `flutter analyze`, `flutter test` ve
`flutter build web` basarili.

Aktif frontend slice: **Design System Baseline**.

Tamamlanan alt adim: color palette. Palet hedefi goz yormayan ama zihni uyanik
tutan bir denge: soft mint/off-white zemin, koyu petrol metin, sakin teal
primary, dusuk doygunluklu amber focus/accent.

Tamamlanan alt adim: typography/button/screen shell baseline. Sistem fontu,
fixed text sizes, 8px radius, 48px minimum button height, scroll destekli
`AppScreen`, `AppPrimaryButton`, `AppSecondaryButton` ve `AppDangerButton`
eklendi.

Tamamlanan alt adim: loading/error/empty state widget'lari ve ilk
game-specific tile/link/card gorsel dili. `AppLoadingState`, `AppErrorState`,
`AppEmptyState`, `LinkTile` ve `GameInfoCard` eklendi.

Tamamlanan alt adim: responsive layout baseline. `AppBreakpoints`,
`AppSpacing`, `AppLayout` ve `AppScreenSize` eklendi. Kurallar: mobile < 600,
desktop >= 1024; mobile padding 16, tablet/web padding 24; compact 560,
standard 720, game 840, wide 960 max width.

Design System Baseline tamamlandi.

Tamamlanan frontend slice: **API Client And Session Foundation**.

Tamamlanan alt adim: API/session foundation baseline. `ApiConfig`, `ApiClient`,
`ApiProblemDetails`/`ApiException`, `InMemoryTokenStore` ve `SessionCubit`
eklendi. Token header ekleme, ProblemDetails parse, 401 mapping ve session
state davranisi testlerle kilitlendi.

Aktif frontend slice: **Auth / Guest Entry**.

Tamamlanan alt adim: guest entry dev flow. `POST /players/guest` contract'ina
baglanan `GuestPlayerRepository`, `GuestEntryCubit` ve `/guest` ekrani eklendi.
DevelopmentBearer stratejisi icin backend'in dondurdugu guest player id gecici
access token olarak session'a yaziliyor.

Tamamlanan alt adim: guest session restart ve reset local session karari.
Guest session iOS/Android/web uzerinde `shared_preferences` ile kalici
saklanir. App restart sonrasi token varsa `SessionCubit` authenticated state'e
doner ve guest ready ekrani acilir. Reset aksiyonu lokal token'i temizler,
session'i unauthenticated yapar ve guest start ekranina geri dondurur. Bu
kalicilik sadece local/dev guest bearer icindir; native sign-in, production
token exchange ve secure storage daha sonraki auth alt adimlarina ertelendi.

Tamamlanan alt adim: guest flow real backend smoke. Local PostgreSQL migrator
0 pending script ile basarili calisti; API DevelopmentBearer modda
`http://127.0.0.1:5099` uzerinde baslatildi. `POST /players/guest` gercek
backend'den id dondurdu, ayni id `Authorization: Bearer <playerId>` olarak
`GET /players/{id}` protected endpoint'inde dogrulandi. Flutter web build
`LEXILINK_API_BASE_URL=http://127.0.0.1:5099` ile yeniden uretildi.

Backend'de browser smoke icin gerekli CORS eksigi giderildi. Development
config sadece `http://127.0.0.1:5173` ve `http://localhost:5173` origin'lerini
acar; production config'te origin verilmezse policy etkisiz kalir.

Aktif frontend slice: **Category Selection**.

Tamamlanan alt adim: category list baseline. Backend `GET /categories`
contract'i okundu; endpoint authenticated ve `{ id, name }` listesi donduruyor.
Flutter tarafinda `Category`, `CategoryRepository`, `CategoryListCubit` ve
`/categories` route'u eklendi. Guest ready ekranindan category ekranina gecis
var. Ekran loading/error/empty state'leri kullaniyor; liste doluysa kategori
tile'ina basinca secili state'e geciyor. Local backend smoke'ta `GET
/categories` 200 dondu, mevcut dev DB'de liste bos oldugu icin UI empty state
gosterir.

Tamamlanan alt adim: Spor content import. `docs/category-spor.json` dosyasi
okundu; 157 benzersiz link ve 1234 benzersiz edge dogrulandi. `CategoryImporter`
tool'u eklendi ve local PostgreSQL'e deterministic id'lerle Spor kategorisini
import etti. API dogrulamasi: `/categories` Spor'u donduruyor,
`/categories/{id}` `linkCount: 157` donduruyor, `/links?categoryId=...` 157
link donduruyor.

Tamamlanan alt adim: Start Game flow baseline. Category selection ekraninda
secili kategori icin `Start easy game` aksiyonu eklendi. Frontend
`POST /games`, `POST /games/{id}/start`, `GET /games/{id}` akisini
`GameRepository` ve `GameStartCubit` ile yurutuyor. `/games/:gameId` route'u ve
ilk `GameScreen` eklendi; start/current/target kelimeleri, steps left ve hints
sayaci gosteriliyor.

Tamamlanan alt adim: outgoing link choices ve make-step. `GameScreen` artik
current link icin `/links/{currentLinkId}/outgoing` listesini yukluyor, aktif
secenekleri tile olarak gosteriyor ve secimde `POST /games/{id}/steps`
cagrisi yapiyor. Basarili adimdan sonra oyun detayi ve yeni outgoing secenekler
yeniden yukleniyor. Canli API smoke'ta `lig -> kulup` adimi atildi; game
details `currentWord: kulup`, `stepsTaken: 1` dondurdu.

Tamamlanan alt adim: hint/undo/reset/abandon ve result state baseline.
`GameScreen` artik Hint, Undo, Reset ve Abandon aksiyonlarini gosteriyor.
Butonlar backend allowance sayaclarina ve busy/finished state'e gore disable
oluyor. Hint sonucu onerilen outgoing tile'i vurguluyor. Undo/reset/abandon
sonrasi game details ve outgoing choices yeniden yukleniyor. Completed, Failed
ve Abandoned state'leri icin result paneli eklendi. Canli API smoke'ta hint,
undo, reset ve abandon dogrulandi; abandon sonrasi state `Abandoned` dondurdu.

Tamamlanan alt adim: completed/failed real flow smoke ve UI polish. `GameScreen`
artik backend `score` alanini ve oynanan path history'yi gosteriyor.
`GameDetails` score/history parse eder ve completed game detayi testle
kilitlendi. Canli API smoke'ta `boks -> dövüş sporu -> spor -> antrenman`
rotasi `Completed`, `score: 300`, `stepsTaken: 3` dondurdu. Ayrica
`pota -> basket -> ekipman -> ayakkabı -> bisiklet -> ayakkabı -> bisiklet ->
ayakkabı -> bisiklet` rotasi max step sonunda `Failed`, `stepsTaken: 8`
dondurdu.

Game Screen MVP oynanabilir baseline tamamlandi.

Aktif frontend slice: **Leaderboard And Profile**.

Tamamlanan alt adim: Stats contract data layer. Backend contract'lari okundu:
`GET /stats/players/{playerId}` ve `GET /stats/leaderboard`. Flutter tarafinda
`PlayerStats`, `LeaderboardEntry`, `LeaderboardQuery` ve
`PlayerStatsRepository` eklendi. Repository testleri player stats parse,
leaderboard query parametreleri ve leaderboard parse davranisini kilitliyor.
Canli API smoke'ta player stats 200 dondu; leaderboard all-time listesi de
beklenen camelCase contract ile geldi.

Tamamlanan alt adim: profile summary presentation baseline.
`ProfileSummaryCubit` token store'dan player id'yi okuyup
`PlayerStatsRepository` uzerinden player stats yukluyor; loading/success/failure
state'leri yonetiliyor. `/profile` route'u ve `ProfileSummaryScreen` eklendi;
ekran handle/provider/locale ozeti, games completed, best score, total score
ve last completed alanlarini gosteriyor; shared loading/error/empty
widget'larini yeniden kullaniyor. Guest ready ekrani `View profile` ile bu
ekrana gecis veriyor. Cubit testleri session-present success, missing-session
ve API error path'lerini kilitliyor. `flutter analyze`, `flutter test` (31/31)
ve `flutter build web` basarili.

Tamamlanan alt adim: all-time leaderboard presentation baseline.
`LeaderboardCubit` `PlayerStatsRepository.getLeaderboard` cagrisi ile varsayilan
all-time/bestScore listesini yukluyor; `LeaderboardQuery` state'te tutuluyor.
`/leaderboard` route'u ve `LeaderboardScreen` eklendi; rank, handle, games
completed, total score ve best score gosteriliyor; loading/error/empty
state'leri shared widget'lardan geliyor. Guest ready ekrani ve profile summary
ekrani leaderboard'a navigation veriyor. Cubit testleri parsed entries,
empty success ve ApiException path'lerini kilitliyor. Canli API smoke
`GET /stats/leaderboard?orderBy=bestScore&period=allTime` ile 2 entry dondurdu;
yeni guest player session da listede. `flutter analyze`, `flutter test`
(34/34) ve `flutter build web` basarili.

Tamamlanan alt adim: daily/weekly leaderboard period selector.
`LeaderboardQuery` `Equatable` + `copyWith` ile guncellendi. `LeaderboardCubit`
`changePeriod(LeaderboardPeriod)` metodu eklendi; ayni period zaten yuklenmisse
no-op, aksi halde `state.query.copyWith(period: ...)` ile reload yapiyor.
Loading/failure state'leri artik aktif query'i tasiyor. Ekrana Material
`SegmentedButton` (All-time / Daily / Weekly) eklendi; loading sirasinda
disable oluyor. Subtitle ve empty mesaji secime gore degisiyor. Cubit testleri
changePeriod reload ve no-op path'lerini kilitliyor. Canli API smoke 3 period
icin de dogrulandi: allTime 2, daily 0, weekly 2. `flutter analyze`,
`flutter test` (36/36) ve `flutter build web` basarili.

Slice 7 Leaderboard And Profile tamamlandi. Sıradaki onerilen alt adim:
profile/leaderboard ekranlari icin empty/error state'lerin bilincli polish'i
veya yeni bir slice (ornegin oyun sonu UI iyilestirmeleri). Roadmap'te kalan
"Empty/error states" kalemi de mevcut implementasyonla buyuk olcude karsilandi.

Tamamlanan slice: **Energy badge on frontend**.
`features/energy/` altinda `PlayerEnergy` DTO, `EnergyRepository.getMe()`,
`EnergyCubit` ve `EnergyBadge` widget'i eklendi. Guest ready ekrani
session authenticated olunca `loadEnergy()` tetikliyor; category secim
ekrani mount sirasinda yukluyor ve `GameStartCubit.failure` durumunda
yeniden cekiyor. `GameStartCubit` insufficient energy mesajini backend
rule'unun text'i ile zaten propagate ediyor; ek is yok. `flutter analyze`,
`flutter test` (40/40) ve `flutter build web` basarili.

Sıradaki adim somut olarak ilan edilmedi; potansiyel adaylar: game ekraninda
energy gosterimi/decrement animasyonu, full-screen energy snapshot route
(countdown timer ile), Apple/Google native sign-in UI (provider credential'lar
gelince).

Tamamlanan slice: **Quests on frontend (2026-05-15)**.
`features/quests/` altinda `PlayerQuest` DTO + `QuestState` enum,
`QuestRepository.getMe()` + `claim(id)`, `QuestsCubit` (load + claim;
claimingId per-tile spinner; claim success → reload + reward queued
mesaji), `/quests` route ve `QuestsScreen` (state'e gore sirali tile listesi:
Ready → Active → Claimed → Expired; ReadyToClaim icin Claim butonu;
SnackBar ile reward queued bilgisi). Guest ready ekranindan "View quests"
girisi eklendi.

Reward async geliyor (backend outbox → Energy GrantBonus, ~5s polling);
UI claim sonrasi quest listesi reload yapiyor ve "Reward queued — your
energy will update in a few seconds" mesaji gosteriyor. Aktif energy
polling/animasyonu yok; kullanici energy badge'i bir sonraki ekran
ziyaretinde guncel goruyor (badge mevcut sekliyle son fetch'i gosteriyor).

Verification: `flutter analyze` 0 issue, `flutter test` 46/46 (eski 40 + 6
yeni Quests), `flutter build web` basarili. Live smoke: yeni guest icin
`GET /quests/me` 200 `[]` dondurdu; `POST /quests/{fake-id}/claim` 404
ProblemDetails; bearer-less claim 401.

Tamamlanan slice: **Home Landing UX (2026-05-16)**.

Eski Bootstrap design-preview ekrani ve `/guest` interstitial'i kaldirildi.
Splash + ana akis tek hat: `/` → `SplashScreen` (sand-pour LexiLink wordmark
animasyonu) → `/home` (yeni). Sand-pour animasyon harf bazli stagger ile
yukseklik ve opaklik tween'liyor; her harfin uzerinde deterministic seed'li
`CustomPaint` partikulleri akiyor.

Yeni `HomeScreen` `Align(Alignment(0, -0.18))` + `ConstrainedBox(maxWidth: 420)`
+ `LayoutBuilder` ile kategori `cardSize = maxWidth * 0.82` hesabini paylasiyor.
Logo `FittedBox(BoxFit.fitWidth)` icinde `cardSize * 0.72` genislikte;
PageView slot height da `cardSize` (kare kart). Swipeable kategori carousel
`PageController(viewportFraction: 0.82)` + `ScrollConfiguration` ile mouse/touch
drag aktif. Her kart gradient + emoji + isim overlay'i; `_PageDots`
secili sayfada genisliyor. Start butonu seçili kategori ile
`GameStartCubit.startGame`'i tetikliyor. Top-right `EnergyBadge`, top-left
profile + quests `_SideIconButton` (Material elevation 1 yuvarlak buton).

Sahte content: development DB bos oldugu icin 3 kategori (Hayvanlar, Yemekler,
Doğa) `POST /categories` ile seed edildi. Backend `Category` modelinde image
alani yok; frontend isim → emoji + gradient `_categoryVisuals` map'i ile
gorsel disardan veriliyor (gercek `imageUrl` ihtiyaca gore sonraki slice).

`AppBackBar` ortak widget'i eklendi (yuvarlak geri butonu + baslik;
`fallbackRoute` default `/home`). Profile/Quests/Leaderboard ekranlari ust
basligi `AppBackBar`'a cevirdi, alttaki "Back" butonlari emekli. Profile
ekrani avatar gradient + handle + provider/locale ozet + 16px-radius stats
karti formatina alindi; Quests tile'larina `LinearProgressIndicator` eklendi,
"+N⚡" reward badge'i ile birlikte spaceBetween yerlesim. Leaderboard ust
ozet basligi sadelesti.

`/guest` route'u silindi; `lib/features/auth/presentation/guest_entry_screen.dart`
ve `lib/features/bootstrap/` tamamen kaldirildi. `GuestEntryCubit` +
`GuestPlayerRepository` HomeScreen'in silent auto-guest auth akisi icin yerinde
kaldi (`SessionStatus.unauthenticated` → `continueAsGuest` BlocListener).
`test/app/app_test.dart` artik SplashScreen render eder durumu kontrol ediyor.

Verification: `flutter analyze` 0 issue (1 info — `prefer_int_literals`),
`flutter test` 45/45, web build ve Chrome smoke `localhost:5173` uzerinde
manuel goruldu (splash + carousel swipe + side icon navigation +
AppBackBar). `/categories` route'u (eski liste ekrani) henuz silinmedi; bir
sonraki temizlikte ele alinacak.

Tamamlanan slice: **Game Screen Polish (2026-05-17)**.

Slice 10 ile gelen tasarim dili oyun ekranina tasindi. `GameRepository`
artik tek path olarak `getOptions(gameId)` kullaniyor; legacy
`getOutgoingLinks(linkId)` silindi. `GameDetails` DTO `startLinkId` +
`targetLinkId` parse ediyor; previous-link tespiti id-eslestirmesi ile.
`AppBackBar` opsiyonel `onBack` + `trailing` parametrelerini destekliyor.

Yeni `GameScreen` yapisi: `AppBackBar`(back = "Quit game?" diyalog →
abandon, trailing = Reset PopupMenu) → start anchor + step dots progress
rail + target anchor → gradient current hero card → breadcrumb (son 3
kelime) → Steps/Hints/Score status chips → 2x3 outlink `GridView`
(hint-recommended sandy gold ring, previous-link muted + `Icons.undo`
rozeti, long-word `FittedBox`) → Hint/Undo secondary butonlari.
Completed/Failed/Abandoned sonucu `showModalBottomSheet` ile: outcome
icon/title + summary stats (Score/Steps/Hints used) + full path + "Back
to home". Eskimiş `widgets/link_tile.dart` ve `widgets/game_info_card.dart`
silindi.

Verification: `flutter analyze` 0 yeni issue (yalnizca Slice 10'dan kalan
splash `prefer_int_literals` info), `flutter test` 45/45. Live API smoke
Spor kategorisinde 9-step `kış sporu` baslangici icin 6 deterministik
option dondurdu. Tarayici smoke kullanici tarafinda.

Sıradaki onerilen alt adim somut olarak ilan edilmedi; potansiyel adaylar:

- Sign-out / Reset session UI — Slice 10'da bootstrap reset ekrani
  kaldirildiginda `SessionCubit.signOut()` UI butonu ile bir baglantisi
  kalmadi. Profile ekraninda kucuk bir "Reset session" satiri eklemek
  test/dev rahatligi acisindan en dusuk maliyetli adim.
- "Play again" tek tikla yeni oyun — Result sheet'inde category id
  bilindigi icin `POST /games` + `POST /games/{id}/start` ile direkt yeni
  gameId'ye yonlendirilebilir. Su an sheet sadece `/home`'a donuyor.
- Energy badge'i oyun ekraninda gosterip kalan enerji ile "Play again"
  CTA disable'lamak. Su an game ekraninda energy yok (bilincli karar).
- `/categories` legacy route'unun silinmesi — Slice 10'dan kalan
  cleanup; route guard ve test temizligi gerektirir.

---

## Admin frontend sprint (F1–F6) ✅ closed (2026-05-22…23)

Backend Administration sprint (B1–B10) kapandıktan sonra 6 slice'lık
admin shell. Tüm slice'lar kapandı; teslimat detayı
`frontendProgress.md > Admin frontend sprint (F1–F6)` içinde.

Özet:

- **F1** — Admin login + ayrı session (commit 70031bf). `admin_auth`
  feature, `SharedPreferencesAdminTokenStore` (key
  `lexilink.admin.accessToken`, player session ile orthogonal),
  `AdminSessionCubit` state machine, `/admin/login` route.
- **F2** — Admin shell + ShellRoute nav (commit f57cede). `AppAdminShell`
  NavigationRail / NavigationDrawer + sign-out. `/admin` →
  `/admin/quests` redirect + router-level admin auth guard.
- **F3** — Admin quests catalog CRUD (commit c7847ae). List / create /
  edit (form dialog) / deactivate. `ApiClient.putJson` eklendi.
- **F4** — Admin player console (commit a31622b). Lookup-by-GUID +
  detail card + ban dialog (reason) + unban confirm. Search-by-handle
  is a backend follow-up, intentionally not smuggled into frontend.
- **F5** + **B11** — Admin energy console (commit 45f8ac0). Snapshot
  lookup + Set / Grant / Reset action dialogs. B11 backend GET added
  by reusing existing query handler.
- **F6** — Admin audit log (commit 5d8e75b). Filter row + paged list +
  payload JSON dialog.

Quality gate at sprint close: 103/103 frontend tests pass, flutter
analyze 5 pre-existing infos, flutter build web ok.

### Manual-test follow-on fixes (2026-05-22…23, uncommitted at doc time)

These ship as part of an upcoming "frontend stabilization" commit OR
get absorbed into Sprint Q1 if the change is structural enough:

- **Splash deep-link fix.** `usePathUrlStrategy()` added in
  `lib/main.dart` so address-bar `/admin/login` resolves directly
  instead of bouncing through splash → `/home`. `flutter_web_plugins`
  added to pubspec.
- **Player guest flow now exchanges a JWT.**
  `GuestPlayerRepository.registerGuest` now calls
  `POST /auth/token` after `POST /players/guest` (provider=Guest,
  externalToken=`dev:Guest:{deviceId}`) and returns
  `GuestSession(playerId, accessToken)`. `SessionCubit.setAuthenticated`
  takes both and persists separately. This unblocks the API running in
  `ProductionJwt` mode.
- **TokenStore.savePlayerId / readPlayerId.** Separate persisted key
  `lexilink.playerId`. `GameStartCubit` and `ProfileSummaryCubit` now
  read `playerId` from the dedicated API; the prior anti-pattern of
  reading the access token as the player ID worked only in
  `DevelopmentBearer` mode and silently broke in `ProductionJwt`.
- **`useRootNavigator: false` on every admin showDialog.** go_router
  16 + `ShellRoute` makes `Navigator.pop` from a root-navigator dialog
  trip the "popped last page" assertion → blank shell. Override
  applied across quests / players / energy / audit.
- **Admin energy card stays visible during saving.** Old behavior
  replaced the entire card with a CircularProgressIndicator so the
  operator never saw the value flip; the new build keeps the card and
  overlays a dimmed Stack + spinner during `saving`.
- **Quest create dropdown shows "(exists)" for taken types.** Type
  dropdown disables types that already have a definition; if all are
  taken, an inline message points the operator to Edit / Deactivate /
  Reactivate instead. Will be moot in Q1 once `QuestType` enum is
  replaced by free-text `Name` (admin invents the identity).
- **`QuestType.Custom1/2/3` placeholder slots** (backend) +
  corresponding `AdminQuestType.custom1/2/3` (frontend) added so the
  Create flow can be exercised against types that don't already have
  a definition. Will be removed with the enum itself in Q1.1 / Q1.6.
- **Quest reactivate button.** Inactive rows now show a power-icon
  reactivate button instead of a disabled deactivate button. Wired to
  backend B12 (`POST /admin/quests/definitions/{id}/reactivate`).

---

## Next planned — Sprint Q1.6 (Quests redesign — frontend reshape)

Frontend leg of the data-driven Quests redesign documented in
`ROADMAP.md > Sprint Q1 — Quests Module Redesign`. Q1.6 details
copied into `frontendRoadmap.md > Slice Q1.6 — Quests redesign
(frontend reshape)`.

Briefly: remove `AdminQuestType` enum, replace with free-text `Name`
+ `QuestTrigger` enum (3 values). Admin form gains `Name`,
`Description`, `Trigger`, `Threshold`, `Reward`, `ProgressBaseline`,
`Prerequisite` (dropdown of other definitions). Player quest screen
renders `Name` + `Description` + computed progress.

Pre-Q1 the operator may choose to ship the in-progress fixes above as
an interim commit (small "frontend stabilization" bundle). Otherwise
those fixes merge into the Q1.6 commit naturally.
