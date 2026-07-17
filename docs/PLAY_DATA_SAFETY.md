# Google Play Data Safety — WordLope Android v1

Operator worksheet for the first Android submission of **WordLope**
(`com.wordlope.app`, current baseline `1.0.1+2`). Copy these answers into Play
Console only after checking them against the exact uploaded AAB. If build flags,
SDK versions, permissions, authentication, ads, or IAP change, re-audit this
document before updating the form.

## Submitted-build contract

```text
LEXILINK_API_BASE_URL=https://api.wordlope.com
LEXILINK_ENABLE_ADS=false
LEXILINK_ENABLE_REWARDED_ADS=false
LEXILINK_ENABLE_IAP=false
```

- The app shows no ads and exposes no IAP route in this version.
- The Google Mobile Ads and Play Billing SDKs are still bundled. In particular,
  the merged Android manifest contains the Mobile Ads initialization provider,
  Advertising ID/AdServices permissions, and Billing permission.
- Google's Mobile Ads disclosure says the SDK automatically collects and shares
  IP-derived approximate location, user product interactions, diagnostics, and
  device/account identifiers. The conservative answers below therefore disclose
  those SDK practices even though WordLope does not request or display an ad.
- Before submission, inspect the **release** merged manifest/AAB again; the
  current evidence was also reproduced from the debug merged manifest.

## Top-level Play Console answers

| Play Console question | Answer for v1 | Evidence / note |
| --- | --- | --- |
| Does the app collect or share required user data types? | **Yes** | Account/game backend plus bundled Mobile Ads SDK. |
| Is all collected user data encrypted in transit? | **Yes** | Production app/API and public pages use HTTPS/TLS. Reject any release build pointing to HTTP. |
| Can users request deletion? | **Yes** | In-app Profile deletion and `https://wordlope.com/account-deletion/`. |
| Account creation method | **In app** | Guest account creation; optional Apple account linking. |
| Independent security review | **No** | No qualifying external review has been completed. |
| Does the app contain ads? | **No for v1** | Ads and rewarded ads are feature-flagged off and have no UI entry. Re-answer **Yes** before enabling ads. |
| Does the app use Advertising ID? | **Yes / SDK use** | Merged manifest includes `com.google.android.gms.permission.AD_ID` through Google Mobile Ads. |

## Target audience and children

- Select **13–15**, **16–17**, and **18 and over** for the first release.
- Do not select an age group below 13 and do not enroll the app in the Designed
  for Families program. The app and store creative must not be directed
  primarily at children.
- The public privacy policy states that WordLope is not directed to children
  under 13. If product positioning, creative, audience selection, or regional
  requirements change, repeat the Families, ads, SDK, and consent review before
  submission.

## Data types to declare

“Shared” below uses Play's third-party definition. Reconfirm how Play classifies
Google Mobile Ads for the SDK version in the uploaded bundle.

| Play data type | Collected | Shared | Required? | Purposes | Source / behavior |
| --- | --- | --- | --- | --- | --- |
| **Personal info → Name** | Yes | No | Required | App functionality, account management | Player display name/handle. A default guest name is still linked to the player account. |
| **Personal info → Email address** | Yes | No | Optional | App functionality, account management | Returned only when a user chooses Apple linking and Apple supplies it. |
| **Personal info → User IDs** | Yes | No | Required | App functionality, account management, fraud prevention/security | Player UUID, guest identifier, optional Apple subject identifier. |
| **Location → Approximate location** | Yes | Yes | Required/automatic SDK behavior | Advertising or marketing, analytics, fraud prevention/security | Google Mobile Ads may infer approximate location from IP. WordLope does not request Android location permission. |
| **App activity → App interactions** | Yes | Yes | Required/automatic SDK behavior | Advertising or marketing, analytics, fraud prevention/security | Mobile Ads SDK product interactions such as launches/taps; no ads are shown in v1. |
| **App activity → Other actions** | Yes | No | Required | App functionality, personalization | Gameplay steps, completion, score, quests, rewards, inventory and language preference. |
| **App info and performance → Diagnostics** | Yes | Yes | Required/automatic SDK behavior | Analytics, fraud prevention/security | Mobile Ads SDK performance/diagnostic information. No first-party crash analytics SDK is installed. |
| **Device or other IDs → Device or other IDs** | Yes | Yes | Required | App functionality, account management; SDK advertising/analytics/fraud prevention | App-generated guest device identifier plus Mobile Ads advertising ID/app set ID or related identifiers. |

## Do not select for the v1 build

Based on the audited code and disabled monetization routes, do not select the
following unless the exact uploaded artifact or Play SDK declaration shows
otherwise:

- Precise location, physical address, phone number, contacts, messages, photos,
  videos, audio files, calendar, health/fitness, files/documents, installed apps,
  web browsing history, in-app search history, credit score, or payment-card data.
- Crash logs (there is no crash-reporting SDK); keep **Diagnostics** selected for
  Mobile Ads.
- Purchase history for v1 because `LEXILINK_ENABLE_IAP=false` removes every IAP
  entry point. Add **Purchase history** before enabling IAP or distributing any
  active artifact that can purchase.

## Security, retention, and deletion evidence

- Access tokens and guest identifiers use local storage; the bearer token and
  player/session identifiers are cleared on account deletion. Sensitive account
  credentials are never committed to the repository.
- Server-side deletion runs in one PostgreSQL transaction. Auth identities and
  operational gameplay data are deleted; the player becomes a randomized,
  banned tombstone so old JWTs are rejected.
- Purchase/security audit records may be retained for fraud, accounting, legal,
  and dispute obligations only after the player identifier is replaced/redacted.
- The public policy describes data categories, purposes, processors, security,
  retention/deletion, international processing, children, and contact details.

## Operator submission checklist

- [ ] Upload the candidate to Internal testing and download Play's generated
      device APKs.
- [ ] Inspect App bundle explorer permissions and SDK list.
- [ ] Confirm all three monetization defines are `false` in the candidate.
- [ ] Re-open Google's current Mobile Ads Data Safety disclosure for the bundled
      SDK version and compare every automatic data type/purpose.
- [ ] Enter the top-level and per-data-type answers above.
- [ ] Select only the documented 13+ target age groups and verify that the
      listing/creative is not child-directed.
- [ ] Use `https://wordlope.com/privacy/` as Privacy policy URL.
- [ ] Use `https://wordlope.com/account-deletion/` as Account deletion URL.
- [ ] Confirm both URLs return 200 without login, geo restriction, or PDF/file
      download.
- [ ] Save the Play preview/screenshots of the submitted Data Safety form with
      the release record.

## Mandatory re-audit triggers

- Enabling ads, rewarded ads, or IAP.
- Adding analytics, crash reporting, push notifications, social sign-in, or a
  new third-party SDK.
- Changing Android permissions, authentication data, backend logging, data
  retention/deletion, or supported account/profile fields.
- Updating Google Mobile Ads/Play Billing across a material disclosure change.
