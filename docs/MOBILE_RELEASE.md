# MOBILE_RELEASE.md

Store-build readiness for the LexiLink Flutter app (GO3). The game ships to
the **iOS App Store** and **Google Play** — there is no web build. The backend
runs at `https://api.wordlope.com` (see `ROADMAP.md > Sprint GO`,
`OPERATIONS.md`).

Most items here are **operator-owned** (store accounts, signing, real
credentials) and cannot live in the repo. This is the checklist + the exact
build commands so a release build points at production with the right ids.

---

## 1. Point the app at the production API

The base URL is compile-time via `--dart-define`; the default is `localhost`,
so a **release build must pass it** or the app talks to nothing:

```
LEXILINK_API_BASE_URL   default http://127.0.0.1:5000   →  https://api.wordlope.com
```

(`lib/shared/api/api_config.dart` reads `String.fromEnvironment('LEXILINK_API_BASE_URL')`.)

Google account linking also needs the OAuth web/server client id at build time
so the mobile SDK returns an ID token the backend can verify:

```
GOOGLE_SIGN_IN_SERVER_CLIENT_ID=<google-oauth-client-id>
```

## 2. Real ad ids (AdMob)

Defaults are Google **test** ids (work without an account, never bill). For
production, supply real ids:

| What | Where | How |
| --- | --- | --- |
| Interstitial ad-unit | `--dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=...` | per build; current production iOS interstitial id: `ca-app-pub-2115638398802394/4516380950`. |
| Rewarded ad-unit | `--dart-define=ADMOB_REWARDED_AD_UNIT_ID=...` | per build; current production rewarded id: `ca-app-pub-2115638398802394/3077352370`. |
| AdMob **app** id (Android) | `android/app/src/main/AndroidManifest.xml` → `com.google.android.gms.ads.APPLICATION_ID` | `ca-app-pub-2115638398802394~7914746084`. |
| AdMob **app** id (iOS) | `ios/Runner/Info.plist` → `GADApplicationIdentifier` | `ca-app-pub-2115638398802394~7914746084`. |

Backend SSV (rewarded → Diamond): keep `Ads__Ssv__Mode=Production` in
production. The API verifies AdMob's rotating public keys from
`Ads__Ssv__VerificationKeysUrl` (default:
`https://www.gstatic.com/admob/reward/verifier-keys.json`). In the AdMob
console, enable server-side verification for the rewarded ad unit and point the
callback at `https://api.wordlope.com/ads/rewarded/callback`.

## 3. In-app purchase (IAP) — gated for launch

- Create these **consumable** products in **App Store Connect** and
  **Play Console**. Product ids must exactly match the backend
  `PaymentProduct.StoreProductId` values:

  | Product id | Diamond amount |
  | --- | ---: |
  | `diamond_100` | 100 |
  | `diamond_550` | 550 |
  | `diamond_1200` | 1200 |
  | `diamond_2500` | 2500 |

  If a product is missing in either store, the app keeps the bundle visible
  but marks it unavailable because the localized store price cannot be loaded.
- Configure backend verification creds (`Payments:Apple`, `Payments:Google`);
  these are fail-closed shells until real creds arrive.
- **Do not enable real-money IAP until social sign-in exists.** Guest accounts
  are device-bound (GO-A), so a purchase would be lost on device change. See
  `ROADMAP.md > Sprint GO > Deliberate non-actions`.

## 4. Release build commands

Version is set in `pubspec.yaml` as `1.0.0+1` (`+1` is the build number;
increment every store upload).

Android App Bundle (for Play):

```bash
cd frontend
flutter build appbundle --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.wordlope.com \
  --dart-define=GOOGLE_SIGN_IN_SERVER_CLIENT_ID=<google-oauth-client-id> \
  --dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=<real> \
  --dart-define=ADMOB_REWARDED_AD_UNIT_ID=ca-app-pub-2115638398802394/3077352370
```

iOS (archive in Xcode or):

```bash
cd frontend
flutter build ipa --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.wordlope.com \
  --dart-define=GOOGLE_SIGN_IN_SERVER_CLIENT_ID=<google-oauth-client-id> \
  --dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=ca-app-pub-2115638398802394/4516380950 \
  --dart-define=ADMOB_REWARDED_AD_UNIT_ID=ca-app-pub-2115638398802394/3077352370
```

> Tip: keep the `--dart-define` set in a build script or `--dart-define-from-file`
> JSON so the production values aren't retyped (and stay out of git).

---

## Store-readiness checklist

**Identity / branding:**

- [x] **Android application id** — `android/app/build.gradle.kts`
      `namespace` + `applicationId` are `com.wordlope.app`; Kotlin
      `MainActivity` package moved to match.
- [x] **iOS bundle identifier** — `ios/Runner.xcodeproj/project.pbxproj`
      `PRODUCT_BUNDLE_IDENTIFIER` is `com.wordlope.app` (tests use
      `com.wordlope.app.RunnerTests`).
- [x] **App display name** — `android:label` (`AndroidManifest.xml`) and
      `CFBundleDisplayName`/`CFBundleName` (`Info.plist`) are `LexiLink`.
- [x] **Version** — `pubspec.yaml` `version:` is `1.0.0+1`; increment the
      build number per upload.

**Signing:**

- [ ] Android upload/release **keystore** configured (`android/key.properties`
      from `android/key.properties.example`; keystore kept out of git). Release
      builds fail if this file is missing.
- [ ] iOS distribution **certificate** + **provisioning profile** (Apple
      Developer account); automatic signing in Xcode or fastlane.

**Config / permissions:**

- [ ] Real AdMob app ids + ad-unit ids wired (section 2).
- [ ] `INTERNET` permission present in the Android release manifest.
- [ ] iOS **ATT** string (`NSUserTrackingUsageDescription`) present (it is) and
      the UMP consent flow verified on device (shipped in AD6).
- [ ] App points at `https://api.wordlope.com` (section 1) — verify a release
      build logs in as guest and loads categories against production.

**Store listings / compliance:**

- [ ] App Store Connect + Play Console apps created under the real bundle ids.
- [ ] Privacy: data-collection disclosures (ads + IAP) — App Privacy labels /
      Play Data safety form.
- [ ] Screenshots, descriptions, age rating, content rating.

---

## Out of scope (operator-owned)

Store accounts, signing material, real AdMob/Apple/Google credentials, store
listing content, and device/sandbox verification are all done outside this
repo. This document is the checklist + build commands; it does not (and cannot)
contain credentials.
