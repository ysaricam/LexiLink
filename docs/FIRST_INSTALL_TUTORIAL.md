# First Install Tutorial

LexiLink has two separate first-install paths:

- **Local development**: run PostgreSQL, apply DbUp migrations, start the API,
  import content, then run the Flutter client.
- **Production first deploy**: prepare a VPS, fill `.env`, build the admin web
  bundle, run Docker Compose through `scripts/deploy.sh`, then seed content.

Use this guide as the first pass. For deeper operational detail, see
`OPERATIONS.md`, `DEPLOYMENT.md`, `CONTENT_AUTHORING.md`,
`MOBILE_RELEASE.md`, and `LAUNCH_CHECKLIST.md`.

---

## 1. Local Development Install

### 1.1. Prerequisites

Install these locally:

- .NET 10 SDK
- Flutter SDK
- Docker, or a local PostgreSQL 17 instance
- `curl`

The development database defaults are already in
`src/API/LexiLink.API/appsettings.Development.json`:

```text
Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852
```

### 1.2. Start local PostgreSQL

If you do not already have a matching local database, start one with Docker:

```bash
docker run --name lexilink-postgres \
  -e POSTGRES_DB=lexilink \
  -e POSTGRES_USER=lexiadmin \
  -e POSTGRES_PASSWORD=0852 \
  -p 5432:5432 \
  -d postgres:17
```

If the container already exists:

```bash
docker start lexilink-postgres
```

### 1.3. Build the solution

From the repository root:

```bash
dotnet build LexiLink.sln --disable-build-servers -v minimal -m:1
```

### 1.4. Apply database migrations

```bash
export ConnectionStrings__LexiLinkDb='Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852'

dotnet run \
  --project src/Database/LexiLink.DatabaseMigrator/LexiLink.DatabaseMigrator.csproj \
  -- \
  "$ConnectionStrings__LexiLinkDb" \
  src/Database/LexiLink.Database/Structure
```

The migrator is safe to re-run. It applies only pending scripts and records
them in `public.MigrationsJournal`.

### 1.5. Import first game content

The schema does not include playable Categories or Links. Import at least one
category:

```bash
dotnet run --project src/Tools/LexiLink.Tools.CategoryImporter/LexiLink.Tools.CategoryImporter.csproj -- \
  "$ConnectionStrings__LexiLinkDb" \
  docs/category-spor.json
```

Optional English content:

```bash
dotnet run --project src/Tools/LexiLink.Tools.CategoryImporter/LexiLink.Tools.CategoryImporter.csproj -- \
  "$ConnectionStrings__LexiLinkDb" \
  docs/category-animals-en.json
```

Re-importing the same file is idempotent.

### 1.6. Run the API

```bash
ASPNETCORE_ENVIRONMENT=Development \
dotnet run --project src/API/LexiLink.API/LexiLink.API.csproj
```

Check health from another terminal:

```bash
curl -fsS http://localhost:5000/health/live
curl -fsS http://localhost:5000/health/ready
```

If your local API binds to another port, use the port printed by `dotnet run`.

### 1.7. Run the Flutter client

```bash
cd frontend
flutter pub get
flutter analyze
flutter test
flutter run -d chrome --dart-define=LEXILINK_API_BASE_URL=http://localhost:5000
```

Adjust the URL if the API is running on a different local port.

### 1.8. Local smoke shortcut

For a production-mode API smoke check against local PostgreSQL:

```bash
./scripts/smoke.sh
```

Override when needed:

```bash
ConnectionStrings__LexiLinkDb='Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852' \
LEXILINK_SMOKE_PORT=5089 \
./scripts/smoke.sh
```

---

## 2. Production First Deploy

Run this on the Ubuntu VPS from the repository root. The expected path is:

```text
/opt/lexilink/app
```

### 2.1. Production prerequisites

Prepare these before deploy:

- A real domain.
- DNS `A` records for `api.<domain>` and `admin.<domain>` pointing to the VPS.
- Docker Engine and the Docker Compose plugin.
- A git checkout of this repo at `/opt/lexilink/app`.
- A bootstrapped admin email.
- Strong random secrets for PostgreSQL, JWT signing, and admin login.

Generate secrets with:

```bash
openssl rand -base64 48
```

### 2.2. Build the admin web bundle

`scripts/deploy.sh` refuses to deploy if the admin web `index.html` is missing.
Build it before the first deploy:

```bash
cd /opt/lexilink/app/frontend
flutter pub get
flutter build web --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.<domain>
```

The default `ADMIN_WEB_ROOT` is:

```text
./frontend/build/web
```

### 2.3. Create `.env`

From `/opt/lexilink/app`:

```bash
cp .env.example .env
chmod 600 .env
```

Edit `.env` and replace every `CHANGE_ME`.

Minimum production values:

```bash
LEXILINK_DOMAIN=<domain>
LEXILINK_ACME_EMAIL=<email>
ADMIN_WEB_ROOT=./frontend/build/web

POSTGRES_DB=lexilink
POSTGRES_USER=lexilink
POSTGRES_PASSWORD=<strong-random-password>

ConnectionStrings__LexiLinkDb=Host=postgres;Port=5432;Database=lexilink;Username=lexilink;Password=<same-postgres-password>

ASPNETCORE_ENVIRONMENT=Production
Authentication__Mode=ProductionJwt
Authentication__Jwt__Issuer=LexiLink
Authentication__Jwt__Audience=LexiLink.Api
Authentication__Jwt__SigningKey=<32-plus-character-secret>
Authentication__TokenExchange__Mode=GuestDevice

Authentication__AdminTokenExchange__Mode=AdminSharedSecret
Authentication__AdminTokenExchange__SharedSecret=<32-plus-character-admin-secret>
Administration__Bootstrap__AdminEmails__0=<admin-email>

Cors__AllowedOrigins__0=https://admin.<domain>
```

Important:

- `ConnectionStrings__LexiLinkDb` must use `Host=postgres`, not `localhost`.
- `POSTGRES_PASSWORD` and the password in `ConnectionStrings__LexiLinkDb` must
  match.
- `Authentication__TokenExchange__Mode=GuestDevice` is required for guest
  players. `Disabled` means players cannot log in.
- The admin login External token is
  `Authentication__AdminTokenExchange__SharedSecret`.

### 2.4. Deploy the stack

From `/opt/lexilink/app`:

```bash
./scripts/deploy.sh
```

The script:

1. Builds or pulls the API image.
2. Starts PostgreSQL.
3. Runs the DbUp migrator once.
4. Starts the API.
5. Starts Caddy for HTTPS.
6. Waits for `/health/ready`.

Verify publicly:

```bash
curl -fsS https://api.<domain>/health/ready
```

Open the admin console:

```text
https://admin.<domain>/admin/login
```

Use the bootstrapped admin email and the admin shared secret as the External
token.

### 2.5. Seed production content

Production starts with schema only. Players see an empty game until content is
imported.

From `/opt/lexilink/app`:

```bash
./scripts/seed-content.sh docs/category-spor.json
```

Optional:

```bash
./scripts/seed-content.sh docs/category-animals-en.json
```

Verify:

```bash
curl -fsS 'https://api.<domain>/categories?locale=tr-TR'
curl -fsS 'https://api.<domain>/categories?locale=en-US'
```

### 2.6. Production first-check checklist

- `docker compose ps` shows `postgres`, `api`, and `caddy` running.
- `curl -fsS https://api.<domain>/health/ready` returns healthy.
- `https://admin.<domain>/admin/login` opens.
- Admin login succeeds with the bootstrapped email and shared secret.
- `/admin/content` shows the seeded category.
- A mobile or web preview build can guest-login and list categories.
- A new game can be created and started.

---

## 3. Mobile Release Build Notes

The Flutter app default API URL is local. Every production build must pass the
production API URL:

```bash
cd frontend
flutter build appbundle --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.<domain> \
  --dart-define=GOOGLE_SIGN_IN_SERVER_CLIENT_ID=<google-oauth-client-id> \
  --dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=<real> \
  --dart-define=ADMOB_REWARDED_AD_UNIT_ID=<real>
```

iOS:

```bash
cd frontend
flutter build ipa --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.<domain> \
  --dart-define=GOOGLE_SIGN_IN_SERVER_CLIENT_ID=<google-oauth-client-id> \
  --dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=<real> \
  --dart-define=ADMOB_REWARDED_AD_UNIT_ID=<real>
```

Store signing, provisioning profiles, real AdMob ids, store accounts, and IAP
credentials are operator-owned.

Do not enable real-money IAP until social sign-in is live. Guest accounts are
device-bound, so purchases can be lost when a player changes devices.

---

## 4. Troubleshooting

### `.env not found`

Run deploy from the repo root and keep `.env` next to `docker-compose.yml`:

```bash
cd /opt/lexilink/app
ls .env docker-compose.yml
```

### `admin web build not found`

Build the Flutter web admin bundle:

```bash
cd /opt/lexilink/app/frontend
flutter build web --release \
  --dart-define=LEXILINK_API_BASE_URL=https://api.<domain>
```

Then confirm:

```bash
ls /opt/lexilink/app/frontend/build/web/index.html
```

### `/health/ready` is unhealthy

Check container status and logs:

```bash
docker compose ps
docker compose logs --tail=120 migrate api
```

Common causes:

- Bad `ConnectionStrings__LexiLinkDb`.
- `ConnectionStrings__LexiLinkDb` uses `localhost` instead of `postgres` in
  Compose.
- PostgreSQL password mismatch.
- A migration failed.
- Required production auth settings are missing.

### Admin login returns 401 or 404

Check:

- `Authentication__AdminTokenExchange__Mode=AdminSharedSecret`.
- The External token equals `Authentication__AdminTokenExchange__SharedSecret`.
- The email equals `Administration__Bootstrap__AdminEmails__0`.
- The API was restarted after changing `.env`.

### Browser admin CORS failure

Set the exact browser origin:

```bash
Cors__AllowedOrigins__0=https://admin.<domain>
```

Then redeploy:

```bash
./scripts/deploy.sh
```

### Categories are empty

Seed content:

```bash
./scripts/seed-content.sh docs/category-spor.json
```

Then verify the locale used by the client. Turkish content is `tr-TR`; English
Animals content is `en-US`.

### Migration failed

Do not start the new API version. Save the logs:

```bash
docker compose logs --tail=200 migrate
```

For shared or production environments, recover through a backup restore or a
forward-only corrective migration. Do not edit scripts that may already be
journaled in `public.MigrationsJournal`.

---

## 5. Follow-up Operations

After the first healthy deploy:

```bash
./scripts/backup-db.sh
docker compose ps
docker stats --no-stream
```

Then complete the production hardening tasks from `DEPLOYMENT.md`:

- Nightly database backup cron.
- Offsite backup copy.
- Restore drill.
- Firewall: only SSH, HTTP, HTTPS.
- SSH key-only login, then disable root/password login.

