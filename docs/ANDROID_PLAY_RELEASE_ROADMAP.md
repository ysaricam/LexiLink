# Android / Google Play Release Roadmap

This is the execution plan for publishing **WordLope** (`com.wordlope.app`) on
Google Play. Work is ordered by dependency and review risk: policy blockers
first, then release engineering, store configuration, testing, and rollout.

Status legend: `⬜ Not started`, `🟡 In progress`, `✅ Done`, `⛔ Blocked`.

## Release decisions

| Decision | Current choice |
| --- | --- |
| Product/store name | **WordLope** |
| Android application ID | `com.wordlope.app` (locked after first Play upload) |
| First production release | Ads **off**, rewarded ads **off**, real-money IAP **off** until the related gates pass |
| Production API | `https://api.wordlope.com` |
| Distribution artifact | Signed Android App Bundle (`.aab`) |
| Rollout strategy | Internal → closed → production staged rollout |
| Play developer account | Personal, newly created in 2026; first application |
| Initial distribution | All countries/regions available and eligible in Play Console |
| Store listing languages | Turkish and English |

## Milestones

| ID | Status | Owner | Outcome |
| --- | --- | --- | --- |
| **PR0** | ✅ | Product/operator | Lock product name, monetization scope, developer-account type, and target markets. |
| **PR1** | ✅ | Engineering | Account and associated-data deletion works in app, API, and public web. |
| **PR2** | ✅ | Engineering/product | Privacy policy and Play Data safety inventory match the exact submitted build. |
| **PR3** | ✅ | Engineering/operator | Release configuration and signing were accepted by Play Console. |
| **PR4** | 🚧 | Product/design/operator | Play Console app exists; listing assets and App content declarations still need completion/verification. |
| **PR5** | 🚧 | Engineering/QA | Signed production AAB is uploaded; Play-installed device and pre-launch gates remain. |
| **PR6** | 🚧 | Operator/testers | Tester enrollment is in progress; the continuous 14-day closed-test window remains. |
| **PR7** | ⬜ | Operator/engineering | Staged production launch is monitored and either expanded or rolled back safely. |
| **PR8** | ⬜ | Engineering/operator | Ads and IAP are enabled only in separate, policy-complete follow-up releases. |

---

## PR0 — Lock release scope

**Purpose:** prevent identity, policy, and store configuration from changing
mid-release.

- [x] Confirm the public product name: **WordLope**.
- [x] Make Android label, in-app title, website, privacy/support pages, AdMob,
      and Play listing use the confirmed name.
- [x] Confirm that first release ships with ads and real-money IAP disabled.
- [x] Record the Play developer account: newly created personal account in
      2026; WordLope will be its first application.
- [x] Select first-release listing languages: Turkish and English.
- [x] Select first-release distribution: all countries/regions available and
      eligible in Play Console.
- [x] Confirm `support@wordlope.com` and `privacy@wordlope.com` are monitored.

**Gate: ✅ Passed (2026-07-16).** Product name, monetization scope, account
type, listing languages, distribution and monitored contact channels are
locked; conflicting Android release documentation was corrected.

## PR1 — Account deletion (policy blocker)

**Purpose:** satisfy Google Play account-deletion policy before submitting a
build that offers Apple account creation/linking.

- [x] Define deletion behavior and legitimate retention rules for purchases,
      fraud prevention, audit, and legal records.
- [x] Add an authenticated player self-deletion API/application flow.
- [x] Delete or anonymize player data across every affected module without
      breaking purchase/audit integrity.
- [x] Make deletion idempotent and reject unauthorized cross-account deletion.
- [x] Add an in-app deletion path under Profile/Settings with explicit
      confirmation and localized copy.
- [x] Clear local tokens, guest identifiers, and cached player data after a
      successful deletion.
- [x] Add a public account-deletion page at
      `https://wordlope.com/account-deletion/` that lets an uninstalled user
      initiate a request.
- [x] Add API integration and Flutter client coverage for the deletion path;
      existing authorization policy covers cross-account access and the
      database operation is idempotent.

**Gate: ✅ Passed (2026-07-16).** A user can initiate deletion in-app or through
the public request page. One database transaction removes operational data,
unlinks retained payment/audit records, creates a non-identifying banned
tombstone, and clears the client session/device credential. The public URL must
be smoke-tested after the next production deploy.

## PR2 — Privacy and Data safety

**Purpose:** make public disclosures match the submitted binary and backend.

- [x] Inventory first-party data: guest/device ID, player ID, Apple identity,
      optional email/name, handle, locale, gameplay/progress, inventory,
      quests, purchases, support and security data.
- [x] Inventory SDK data and permissions, including AdMob/Advertising ID,
      Play Billing, secure storage, shared preferences, and audio dependencies.
- [x] Update the privacy policy with purposes, processors/third parties,
      sharing, security, retention, deletion, children/target audience, and
      contact information.
- [x] Add the privacy-policy and account-deletion links inside the app.
- [x] Prepare a version-controlled Data safety answer sheet for the operator.
- [x] Ensure the ads declaration matches the build flags exactly.
- [x] Decide the target age group and complete child-directed treatment checks.

**Gate: ✅ Passed (2026-07-16).** The first-release contract explicitly disables
ads, rewarded ads, and IAP; the app links to the public policy/deletion pages;
and `PLAY_DATA_SAFETY.md` records first-party plus bundled-SDK disclosures and
the 13+ audience decision. Recheck the release AAB's generated SDK/permission
inventory and both public URLs before copying answers into Play Console.

## PR3 — Android release engineering and signing

**Purpose:** produce an installable, upgrade-safe, Play-compliant release AAB.

- [x] Add `android.permission.INTERNET` to the main release manifest.
- [x] Confirm `applicationId`/namespace remain `com.wordlope.app`.
- [x] Confirm release `targetSdkVersion` is API 36 and `minSdkVersion` is 24.
- [x] Verify 16 KB page-size compatibility for all bundled native libraries.
- [x] Generate a dedicated upload keystore and `key.properties`.
- [ ] Store keystore, aliases, passwords, and recovery instructions in two
      secure operator-controlled backup locations; never commit them.
- [x] Enable Play App Signing and register the upload certificate.
- [x] Make production Dart defines reproducible without committing secrets.
- [x] Remove stale Google sign-in defines/docs unless Google sign-in is
      actually implemented.
- [x] Increment `versionCode` for every upload and keep `versionName` intentional.
- [x] Add a CI/release check for analyze, tests, manifest essentials, and AAB
      creation without exposing signing secrets.

**Gate: ✅ Passed (2026-07-17).** Package `com.wordlope.app`, version `1.0.1+2`,
min/target API 24/36, INTERNET permission, signature presence, production
configuration, and every bundled native library's 16 KB ELF alignment were
verified. The operator then uploaded the real signed AAB successfully to Play
Console and began tester enrollment, confirming the upload-signing path. The
second tested keystore backup remains an operator security safeguard and must
be completed if it has not already been done.

## PR4 — Play Console and store listing

**Purpose:** complete everything Google needs to review and present the app.

- [ ] Create the Play Console app with `com.wordlope.app`.
- [ ] Complete developer identity, contact, merchant/tax/bank verification as
      applicable.
- [ ] Create Turkish and English listings: title, short description, full
      description and release notes.
- [ ] Supply a 512×512 Play icon, 1024×500 feature graphic, and representative
      phone screenshots; add tablet assets if tablet distribution is enabled.
- [ ] Set Game category/subcategory, countries, pricing, and distribution.
- [ ] Complete Privacy policy, Data safety, Ads, App access, Target audience,
      Content rating, Data deletion, Advertising ID and all applicable
      declarations.
- [ ] Give reviewers precise guest-login/access instructions.
- [ ] Link the Play listing to the public privacy, support and deletion pages.

**Gate:** Play Console reports no unfinished setup task or blocking declaration.

## PR5 — Release verification

**Purpose:** validate the exact signed artifact, not only debug builds.

- [ ] Run `flutter analyze` and `flutter test`.
- [ ] Build the signed release AAB with production API and monetization flags.
- [ ] Upload to Internal testing and install from Google Play (not via adb).
- [ ] Smoke test first launch, guest login, category loading, game lifecycle,
      locale switching, profile, account linking and account deletion.
- [ ] Test offline, slow network, server error, background/resume and upgrade
      from the previous test version.
- [ ] Test on representative API 24, mid-range and current Android devices.
- [ ] Review Play pre-launch report, Android vitals, accessibility and large
      screen warnings; fix blockers/high-severity findings.
- [ ] Verify no admin surface or secret is reachable in the mobile release.
- [ ] Verify privacy/support/deletion links and production health endpoints.

**Gate:** zero release-blocking crash/ANR/policy issue; all critical smoke cases
pass using the Play-installed signed build.

## PR6 — Closed test and production access

**Purpose:** gather real feedback and meet account-specific Play requirements.

**Current status (2026-07-17):** The signed release has been moved to Closed
testing. Record day 1 only when at least 12 testers have opted in; keep evidence
of the opt-in count, dates, feedback, and fixes for the production-access form.

- [x] Create the Closed testing track and begin tester enrollment.
- [ ] Finalize a clear test script and feedback channel.
- [ ] Keep at least 12 testers opted into Closed testing continuously for at
      least 14 days (required: this is a new personal developer account).
- [ ] Collect and triage functional, device, UX and policy feedback.
- [ ] Fix blocking findings, upload a higher `versionCode`, and repeat the
      affected gates.
- [ ] Apply for production access and answer Play's testing-readiness questions.

**Gate:** production access is granted and no unresolved P0/P1 issue remains.

## PR7 — Production rollout

**Purpose:** limit launch risk and preserve a fast stop/repair path.

- [ ] Take/verify a current database backup and production health check.
- [ ] Prepare support responses, monitoring owners, rollback criteria, and the
      next emergency `versionCode`.
- [ ] Submit the approved AAB for a staged production rollout.
- [ ] Monitor review status, crashes, ANRs, API health, authentication errors,
      ratings and support mail.
- [ ] Halt rollout on the agreed thresholds; otherwise expand gradually to 100%.
- [ ] Tag the exact released commit and record Play version/build numbers.

**Gate:** rollout reaches 100% with stable vitals and no active policy warning.

## PR8 — Monetization follow-ups (not part of first launch)

### Ads release

- [ ] Update privacy/Data safety/Contains ads declarations before enabling ads.
- [ ] Configure Android AdMob units, UMP messages, SSV and app readiness.
- [ ] Verify `app-ads.txt`, consent-before-request behavior and real-device ads.
- [ ] Enable ads through explicit production flags in a new version.

### IAP release

- [ ] Prove recoverable account identity on Android before selling consumables.
- [ ] Create and activate all four Play products with exact IDs and prices.
- [ ] Configure Google Play Developer API/service-account backend credentials.
- [ ] Test purchase, pending, cancel, duplicate, consume, refund and revoke flows
      with license testers.
- [ ] Update privacy/Data safety and support/refund instructions.
- [ ] Enable IAP in a separate staged release.

**Gate:** monetization never becomes active through IDs/flags alone; policy,
backend verification and end-to-end store tests must all pass first.

## Standard verification commands

```bash
cd frontend
flutter pub get
flutter analyze
flutter test
flutter build appbundle --release \
  --dart-define-from-file=config/android-production.json
```

The final command also requires the operator-owned upload keystore. Keep all
three monetization flags explicit; change them only in the separately gated
milestone that enables those features.

## Current baseline (2026-07-16)

- Flutter analyze: clean.
- Flutter tests: 219/219 passing.
- Release AAB: built successfully and uploaded to Play Console.
- Package: `com.wordlope.app`; version: `1.0.1+2`; target SDK: 36; min SDK: 24.
- Production API, privacy, support and `app-ads.txt`: reachable.
- Release upload signing: accepted by Play Console; verify two secure backups.
- Account deletion: implemented in app/API/public web; smoke-test public URL
  after deployment.
- Play Console app exists and tester enrollment is in progress; listing,
  declarations, pre-launch report, and Play-installed device verification
  remain.
