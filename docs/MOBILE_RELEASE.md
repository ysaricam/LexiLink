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

## 2. Real ad ids (AdMob)

Defaults are Google **test** ids (work without an account, never bill). For
production, supply real ids:

| What | Where | How |
| --- | --- | --- |
| Interstitial ad-unit | `--dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=...` | per build |
| Rewarded ad-unit | `--dart-define=ADMOB_REWARDED_AD_UNIT_ID=...` | per build |
| AdMob **app** id (Android) | `android/app/src/main/AndroidManifest.xml` → `com.google.android.gms.ads.APPLICATION_ID` | edit (currently the test id `ca-app-pub-3940256099942544~3347511713`) |
| AdMob **app** id (iOS) | `ios/Runner/Info.plist` → `GADApplicationIdentifier` | edit (currently the test id) |

Backend SSV (rewarded → Diamond): set `Ads__Ssv__Mode=Production` with real
AdMob keys, and in the AdMob console point the **server-side verification
callback** at `https://api.wordlope.com/ads/rewarded/callback`.

## 3. In-app purchase (IAP) — gated for launch

- Create the consumable products in **App Store Connect** and **Play Console**
  matching the backend `PaymentProduct` ids (`diamond_100`, `diamond_550`,
  `diamond_1200`, `diamond_2500`).
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
  --dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=<real> \
  --dart-define=ADMOB_REWARDED_AD_UNIT_ID=<real>
```

iOS (archive in Xcode or):

```bash
cd frontend
flutter build ipa --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.wordlope.com \
  --dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=<real> \
  --dart-define=ADMOB_REWARDED_AD_UNIT_ID=<real>
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

- [ ] Android upload/release **keystore** configured (`key.properties` +
      `signingConfigs`); keystore kept out of git.
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
