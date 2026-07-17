# Android Upload Signing Runbook

Operator-owned procedure for the WordLope Google Play upload key. Never commit
the keystore, `key.properties`, passwords, recovery codes, or password-manager
exports. The CI key is ephemeral and must never be used for a Play upload.

## 1. Generate the dedicated upload key

Run this locally from `frontend/android`. `keytool` will prompt for a strong,
unique keystore password and the certificate identity. Store the password in
the operator's password manager; do not put it in shell history.

```bash
keytool -genkeypair -v \
  -keystore upload-keystore.jks \
  -alias upload \
  -keyalg RSA \
  -keysize 4096 \
  -validity 10000
```

Create ignored `frontend/android/key.properties` from
`key.properties.example` and replace every placeholder locally:

```properties
storePassword=<password-manager value>
keyPassword=<password-manager value>
keyAlias=upload
storeFile=upload-keystore.jks
```

Confirm that Git ignores both files before continuing:

```bash
git check-ignore android/key.properties android/upload-keystore.jks
```

## 2. Record and export the upload certificate

Display the SHA-256 fingerprint and save it with the release record:

```bash
keytool -list -v -keystore upload-keystore.jks -alias upload
```

Export the public certificate for Play Console. This `.pem` file is public and
contains no private key, but keep it with the signing records rather than in
the application repository.

```bash
keytool -export -rfc \
  -keystore upload-keystore.jks \
  -alias upload \
  -file upload-certificate.pem
```

## 3. Back up before the first upload

Keep two independently recoverable, operator-controlled copies:

1. Encrypted primary storage containing the JKS and a password-manager record
   for alias/passwords, certificate fingerprint, creation date, and app id.
2. A separate encrypted offline or secondary-provider backup, then perform a
   test restore and compare the SHA-256 fingerprint.

Do not use source control, chat, email attachments, issue trackers, shared
drives without client-side encryption, or the repository's deployment secrets
as the only backup.

## 4. Enable Play App Signing

When creating the first Play release, accept Play App Signing and upload the
bundle with this upload key. In **Setup → App integrity**, verify that:

- the app-signing certificate is managed by Google Play;
- the upload certificate SHA-256 matches the locally recorded fingerprint;
- the package is `com.wordlope.app`.

Google protects the app-signing key; this local key authorizes future uploads.
If the upload key is lost or compromised, use Play Console's upload-key reset
process. Losing the local key without a verified Play app/signing setup delays
future releases, which is why the tested backups are a release gate.

## 5. Build and verify the real candidate

From `frontend`:

```bash
flutter pub get
flutter analyze
flutter test
flutter build appbundle --release \
  --dart-define-from-file=config/android-production.json
cd ..
./scripts/verify-android-aab.sh
```

Before upload, record the AAB SHA-256, file size, version, upload-certificate
fingerprint, Git commit, and build date in the release record:

```bash
shasum -a 256 frontend/build/app/outputs/bundle/release/app-release.aab
```
