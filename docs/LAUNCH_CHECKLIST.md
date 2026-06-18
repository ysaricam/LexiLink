# LAUNCH_CHECKLIST.md — Canlıya alma için operatör görevleri

LexiLink'i canlıya almak için **senin** (operatör) yapman gereken her şey.
Repo tarafı (Docker imajı, compose stack, Caddy, prod guest auth) Sprint GO'da
tamamlandı; bu liste koda giremeyecek olan insan/kimlik-bilgisi/sunucu işleri.
Yukarıdan aşağı ilerle — bölümler bağımlılık sırasına göre dizildi.

Detay referanslar: `OPERATIONS.md` (config/endpoint'ler), `MOBILE_RELEASE.md`
(store build), `ROADMAP.md > Sprint GO` (plan + kararlar) ve
`DEPLOYMENT.md` (deploy/rollback/backup/restore/hardening/CI-CD runbook'u).

---

## 0. Önkoşullar (önce bunlar — gerisini bloklar)

- [ ] **Domain al.** HTTPS için şart — Let's Encrypt çıplak IP'ye sertifika
      vermez. ~€10/yıl (Cloudflare, Namecheap vb.).
- [x] **DNS:** `api.wordlope.com` için bir **A kaydı** oluştur → Hetzner sunucu
      IP'n. (Apex `<domain>` ileride bir site için boş kalabilir.)
- [x] **Gerçek bir app id seç** (reverse-DNS): `com.wordlope.app`. Hem
      store'lar hem AdMob/IAP kaydı için kullanılır. `com.example.*` olmaz.

## 1. Sunucuda ilk deploy (GO4)

Ubuntu kutusunda (SSH ile bağlan):

- [ ] **Docker Engine + Compose plugin kur** (Docker'ın resmi apt deposu).
- [ ] **Kodu kutuya getir** — repo'yu `/opt/lexilink/app`'e clone'la (yerinde
      build eder) ya da GO6'dan sonra GHCR imajını çek.
- [ ] **Secret dosyasını oluştur** — repo klasöründe (`docker-compose.yml`
      ile **aynı dizinde**) `cp .env.example .env`, sonra `chmod 600 .env`.
      Doldur:
  - [ ] `LEXILINK_DOMAIN` = domain'in, `LEXILINK_ACME_EMAIL` = e-postan
  - [ ] `POSTGRES_PASSWORD` = uzun rastgele bir değer
  - [ ] `ConnectionStrings__LexiLinkDb` — `POSTGRES_*` değerleriyle aynı
        DB/kullanıcı/şifre, `Host=postgres`
  - [ ] `Authentication__Jwt__SigningKey` = 32+ karakter rastgele secret
        (`openssl rand -base64 48`)
  - [ ] `Administration__Bootstrap__AdminEmails__0` = admin e-postan
  - [ ] `Authentication__TokenExchange__Mode=GuestDevice` olduğunu doğrula
        (guest login)
  - [ ] Browser admin paneli için
        `Authentication__AdminTokenExchange__Mode=AdminSharedSecret` ve
        `Authentication__AdminTokenExchange__SharedSecret` = 32+ karakter
        rastgele secret ayarla. Admin login'de bu değer "External token"
        olarak girilir; email aktif bootstrapped admin olmalı.
  - [ ] Admin web build ayrı bir origin'den servis edilecekse
        `Cors__AllowedOrigins__0` değerini o exact origin yap
        (örn. `https://admin.wordlope.com` veya lokal testte
        `http://localhost:8080`).
  - [ ] `ADMIN_WEB_ROOT` admin web build çıktısındaki `index.html` dosyasını
        içeren klasörü göstersin. Default: `./frontend/build/web`; lokalden
        upload edersen örn. `/opt/lexilink/admin-web`.
- [ ] **Ayağa kaldır:** repo klasöründe `./scripts/deploy.sh` (build → migrator
      → API → Caddy TLS; API sağlıklı olunca durur ve public health komutunu
      yazdırır). Manuel istersen: `docker compose up -d --build`.
- [x] **Doğrula:** `curl https://api.wordlope.com/health/ready` healthy dönsün;
      ardından bir client/build ile guest→category smoke.

## 2. Oyun içeriğini yükle (prod DB boş başlar!)

Deploy yalnızca **şemayı** oluşturur, **hiç Category/Link yoktur** — sen
içerik yüklemezsen oyuncu boş bir oyun görür.

- [ ] Repo klasöründe en az bir kategoriyi import et:
      `./scripts/seed-content.sh docs/category-spor.json` (istersen ek olarak
      `docs/category-animals-en.json`). Script, compose ağına bağlı tek
      seferlik bir .NET SDK container'ı ile importer'ı çalıştırır (host'a .NET
      kurmaya gerek yok). Detay: `CONTENT_AUTHORING.md`.

## 3. Mobil uygulama + store build (GO3 → store'lar)

`frontend/` içinde (tam komutlar için `MOBILE_RELEASE.md`):

- [x] **Android `applicationId`** (`android/app/build.gradle.kts`
      `namespace` + `applicationId`) ve **iOS bundle id**
      (`ios/Runner.xcodeproj/project.pbxproj` `PRODUCT_BUNDLE_IDENTIFIER`)
      değerlerini `com.wordlope.app` yap.
- [x] **Görünen adı** `LexiLink` yap (`android:label`,
      `CFBundleDisplayName`/`CFBundleName`).
- [x] `pubspec.yaml` **sürümünü** yükselt (`0.1.0+1` → `1.0.0+1`).
- [ ] **Signing** kur: Android upload keystore + `key.properties`; iOS
      distribution sertifikası + provisioning profile.
- [x] Production'a bakacak şekilde **Android build al:**
      `flutter build appbundle --release --dart-define=LEXILINK_API_BASE_URL=https://api.wordlope.com ...`
      (`build/app/outputs/bundle/release/app-release.aab`, 50.3MB). iOS için
      `ipa` hâlâ Xcode/CocoaPods/signing sonrası alınacak.
- [ ] Uygulamaları gerçek bundle id'lerle **App Store Connect** + **Google
      Play Console**'da oluştur; gizlilik/data-safety, ekran görüntüleri,
      açıklamalar, yaş sınırını doldur.

Durum: Android appbundle build geçti. iOS toolchain temiz, ancak IPA build
`com.wordlope.app` için Xcode Signing & Capabilities altında Team /
certificate / provisioning eksik olduğu için bloklu.

## 4. Reklam & IAP kimlik bilgileri (operatör-sahipli)

- [ ] **AdMob:** gerçek hesap → app id'leri `AndroidManifest.xml`
      (`APPLICATION_ID`) + `Info.plist` (`GADApplicationIdentifier`) içine yaz,
      gerçek ad-unit id'lerini `--dart-define` ile geç, backend
      `Ads__Ssv__Mode=Production` + gerçek anahtarları ayarla ve AdMob **SSV
      callback**'ini `https://api.wordlope.com/ads/rewarded/callback` yap.
- [ ] **IAP:** consumable ürünleri (`diamond_100/550/1200/2500`) iki store'da
      oluştur; backend `Payments:Apple` / `Payments:Google` creds'lerini
      ayarla.
- [ ] ⚠️ **Gerçek-para IAP'ı social sign-in gelene kadar KAPALI tut** — guest
      hesaplar cihaza bağlı, cihaz değişince satın alım kaybolur.

## 5. Launch öncesi/sırasında doğrulama

- [ ] `https://api.wordlope.com/health/ready` healthy (DB + migration'lar).
- [ ] Browser admin smoke: `/admin/login` açılır, bootstrapped admin email +
      shared external token ile giriş yapılır, `/admin/whoami` ve en az bir
      admin ekranı (Content veya Players) 200 döner.
- [ ] Bir release build guest olarak giriş yapıp production'dan kategorileri
      yüklesin.
- [ ] Test build'de AdMob test reklamları çalışsın; gerçek id/anahtar
      ayarlanınca rewarded → Diamond gerçek SSV callback'iyle düşsün.
- [ ] Gerçek cihazda iOS ATT + UMP consent ekranları çıksın.

---

## 6. Sunucu yedekleme + hardening (GO5)

Tam komutlar için `DEPLOYMENT.md`.

- [x] `./scripts/backup-db.sh` ile manuel backup al.
- [x] Backup dosyasını sunucu dışına kopyala.
- [x] Nightly cron'u kur:
      `17 2 * * * cd /opt/lexilink/app && BACKUP_DIR=/opt/lexilink/backups/postgres RETENTION_DAYS=14 ./scripts/backup-db.sh >> /var/log/lexilink-backup.log 2>&1`
- [x] En az bir restore drill yap veya disposable host üzerinde doğrula.
- [x] `ufw` ile sadece SSH/HTTP/HTTPS açık kalacak şekilde firewall'u aç.
- [x] SSH key-only login doğrulandıktan sonra root/password login'i kapat.
- [x] `docker compose ps`, `docker stats --no-stream`,
      `curl -fsS https://api.wordlope.com/health/ready` ile son kontrol yap.

## Mühendislik tarafında kalanlar (sen değil — biz yapacağız)

- **Takipler (launch sonrası):** gerçek Google/Apple **social sign-in**
  (server-side ID-token doğrulama) — gerçek-para IAP'ı açmadan önce gerekli.
  Browser admin panel için ilk production verifier
  `AdminSharedSecret` olarak eklendi; ileride gerçek SSO ile değiştirilebilir.
