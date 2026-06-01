# LAUNCH_CHECKLIST.md — Operator action items for go-live

Everything **you** (the operator) need to do to take LexiLink live. The repo
side (Docker image, compose stack, Caddy, prod guest auth) is done in
Sprint GO; this list is the human/credential/server work that can't live in
code. Work top to bottom — sections are ordered by dependency.

Deeper references: `OPERATIONS.md` (config/endpoints), `MOBILE_RELEASE.md`
(store build), `ROADMAP.md > Sprint GO` (plan + decisions). Server hardening,
backups, and a full deploy/rollback runbook arrive in **GO5**
(`docs/DEPLOYMENT.md`); CI/CD via GHCR in **GO6**.

---

## 0. Prerequisites (do first — they block everything else)

- [ ] **Buy a domain.** Required for HTTPS — Let's Encrypt won't issue a
      certificate for a bare IP. ~€10/yr (Cloudflare, Namecheap, etc.).
- [ ] **DNS:** create an **A record** `api.<domain>` → your Hetzner server IP.
      (The apex `<domain>` can stay free for a future site.)
- [ ] **Pick a real app id** (reverse-DNS), e.g. `com.lexilink.app`. Used for
      both stores and AdMob/IAP registration. Avoid `com.example.*`.

## 1. Server first deploy (GO4)

On the Ubuntu box (SSH in):

- [ ] **Install Docker Engine + Compose plugin** (Docker's official apt repo).
- [ ] **Get the code/image on the box** — clone the repo (builds locally) or,
      after GO6, pull the GHCR image.
- [ ] **Create the secrets file** `/opt/lexilink/.env` from `.env.example`,
      then `chmod 600`. Fill in:
  - [ ] `LEXILINK_DOMAIN` = your domain, `LEXILINK_ACME_EMAIL` = your email
  - [ ] `POSTGRES_PASSWORD` = long random value
  - [ ] `ConnectionStrings__LexiLinkDb` — same DB/user/password as the
        `POSTGRES_*` values, `Host=postgres`
  - [ ] `Authentication__Jwt__SigningKey` = 32+ char random secret
        (`openssl rand -base64 48`)
  - [ ] `Administration__Bootstrap__AdminEmails__0` = your admin email
  - [ ] confirm `Authentication__TokenExchange__Mode=GuestDevice` (guest login)
- [ ] **Bring it up:** `docker compose --env-file /opt/lexilink/.env up -d --build`
      (migrator runs first, then the API; Caddy fetches TLS automatically).
- [ ] **Verify:** `curl https://api.<domain>/health/ready` returns healthy;
      then a guest→category smoke from a client/build.

## 2. Seed game content (the prod DB starts empty!)

The deploy creates the **schema** but **no Categories/Links** — players would
see an empty game until you import content.

- [ ] Import at least one category with the `CategoryImporter` against the prod
      DB (Postgres is not exposed publicly, so do this over an SSH tunnel to
      the `postgres` container, or run the importer on the box). See
      `CONTENT_AUTHORING.md` for the command and the JSON format. Starter
      content already in the repo: `docs/category-spor.json` (tr-TR),
      `docs/category-animals-en.json` (en-US).

## 3. Mobile app + store build (GO3 → stores)

In `frontend/` (see `MOBILE_RELEASE.md` for exact commands):

- [ ] Change **Android `applicationId`** (`android/app/build.gradle.kts`
      `namespace` + `applicationId`) and **iOS bundle id**
      (`ios/Runner.xcodeproj/project.pbxproj` `PRODUCT_BUNDLE_IDENTIFIER`) from
      `com.example.*` to your real id.
- [ ] Set the **display name** to `LexiLink` (`android:label`,
      `CFBundleDisplayName`/`CFBundleName`).
- [ ] Bump **version** in `pubspec.yaml` (`0.1.0+1` → `1.0.0+1`).
- [ ] Set up **signing**: Android upload keystore + `key.properties`; iOS
      distribution certificate + provisioning profile.
- [ ] **Build** pointing at production:
      `flutter build appbundle --release --dart-define=LEXILINK_API_BASE_URL=https://api.<domain> ...`
      (and `ipa` for iOS), with real AdMob ad-unit defines.
- [ ] Create the apps in **App Store Connect** + **Google Play Console** under
      the real bundle ids; fill privacy/data-safety, screenshots, descriptions,
      ratings.

## 4. Ads & IAP credentials (operator-owned)

- [ ] **AdMob:** real account → set app ids in `AndroidManifest.xml`
      (`APPLICATION_ID`) + `Info.plist` (`GADApplicationIdentifier`), pass real
      ad-unit ids via `--dart-define`, set backend `Ads__Ssv__Mode=Production`
      with real keys, and point the AdMob **SSV callback** at
      `https://api.<domain>/ads/rewarded/callback`.
- [ ] **IAP:** create consumable products (`diamond_100/550/1200/2500`) in both
      stores; configure backend `Payments:Apple` / `Payments:Google` creds.
- [ ] ⚠️ **Keep real-money IAP OFF until social sign-in exists** — guest
      accounts are device-bound, so a purchase would be lost on device change.

## 5. Verify before/at launch

- [ ] `https://api.<domain>/health/ready` healthy (DB + migrations).
- [ ] A release build logs in as guest and loads categories from production.
- [ ] AdMob test ads work in a test build; rewarded → Diamond lands via the
      real SSV callback once real ids/keys are set.
- [ ] iOS ATT + UMP consent prompts appear on a real device.

---

## What's still on the engineering side (not you — we do these)

- **GO5** — backups (`pg_dump` + restore drill), firewall/SSH hardening,
  container limits, and `docs/DEPLOYMENT.md` deploy/rollback/restore runbook.
- **GO6** — CI/CD: build → push to GHCR → SSH deploy.
- **Follow-ups (post-launch):** real Google/Apple **social sign-in**
  (server-side ID-token verification) — required before enabling real-money
  IAP; and a **production admin verifier** so the admin console is usable in
  production (today `AdminTokenExchange__Mode=Disabled` → admin login 401;
  content is imported via the CLI in the meantime).
