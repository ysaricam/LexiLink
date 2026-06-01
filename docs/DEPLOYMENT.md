# Deployment And Operations Runbook

Production host: single Ubuntu VPS running Docker Compose.

Production API: `https://api.wordlope.com`

Run all commands on the server from the repo root, expected at
`/opt/lexilink/app`, unless noted.

## Daily Health

```bash
docker compose ps
curl -fsS https://api.wordlope.com/health/ready
docker compose logs --tail=80 api
```

`/health/live` checks the API process. `/health/ready` checks readiness,
including the database/migration state.

## Deploy

Manual server deploy:

```bash
cd /opt/lexilink/app
git pull --ff-only
./scripts/deploy.sh
curl -fsS https://api.wordlope.com/health/ready
```

By default, `deploy.sh` rebuilds the image locally, runs the DbUp migrator
one-shot, starts the API and Caddy, then waits for readiness.

Prebuilt image deploy:

```bash
cd /opt/lexilink/app
git pull --ff-only
LEXILINK_IMAGE=ghcr.io/ysaricam/lexilink:<tag-or-sha> ./scripts/deploy.sh
curl -fsS https://api.wordlope.com/health/ready
```

When `LEXILINK_IMAGE` is not `lexilink:local`, `deploy.sh` pulls the image and
starts Compose with `--no-build`.

## CI/CD

GitHub Actions workflow:

```text
.github/workflows/deploy-production.yml
```

Triggers:

- `workflow_dispatch` for a manual production deploy.
- Git tag push matching `v*` for release deploys.

Pipeline:

1. Build the Docker image from `Dockerfile`.
2. Push `ghcr.io/ysaricam/lexilink:<git-sha>` and `latest`.
3. SSH to the VPS.
4. Check out the exact deployed git SHA in `/opt/lexilink/app`.
5. Run `LEXILINK_IMAGE=<sha-image> ./scripts/deploy.sh`.
6. Verify `https://api.wordlope.com/health/ready`.

Required GitHub repository secrets:

| Secret | Notes |
| --- | --- |
| `PROD_SSH_HOST` | VPS host/IP. |
| `PROD_SSH_USER` | SSH user with access to `/opt/lexilink/app` and Docker. |
| `PROD_SSH_KEY` | Private SSH key for that user. |
| `PROD_SSH_PORT` | Optional; defaults to `22`. |
| `GHCR_USER` | Optional; defaults to the GitHub actor. |
| `GHCR_TOKEN` | Required if the GHCR package is private. Use a PAT with `read:packages` for the server-side `docker login`. Public packages can omit it. |

Server prerequisites:

- `/opt/lexilink/app` is a git checkout of this repo.
- `.env` exists next to `docker-compose.yml`.
- Docker Engine + Compose plugin are installed.
- The SSH user can run Docker commands.
- If the GHCR package is private, either set `GHCR_TOKEN` in GitHub secrets or
  pre-login on the server with a token that has `read:packages`.

Rollback with CI/CD still uses the same operational rule: redeploy a known-good
commit/image, and restore the database only when the incident is migration/data
related.

## Content Import

Production starts with schema only. Import at least one content graph:

```bash
./scripts/seed-content.sh docs/category-spor.json
./scripts/seed-content.sh docs/category-animals-en.json
```

Re-imports are idempotent for the same category/language.

## Backups

Create a manual backup:

```bash
./scripts/backup-db.sh
```

Default output:

```text
/opt/lexilink/backups/postgres/lexilink-YYYYMMDDTHHMMSSZ.dump
```

The script writes a PostgreSQL custom-format dump, creates a `.sha256` file,
verifies that `pg_restore --list` can read it, and prunes local backups older
than 14 days.

Install nightly backup:

```bash
sudo install -d -m 700 /opt/lexilink/backups/postgres
sudo crontab -e
```

Add:

```cron
17 2 * * * cd /opt/lexilink/app && BACKUP_DIR=/opt/lexilink/backups/postgres RETENTION_DAYS=14 ./scripts/backup-db.sh >> /var/log/lexilink-backup.log 2>&1
```

Copy backups off the VPS after creation. A local-only backup does not protect
against VPS loss.

Example offsite copy from an operator machine:

```bash
rsync -avz user@api.wordlope.com:/opt/lexilink/backups/postgres/ ./lexilink-postgres-backups/
```

## Restore Drill

Use the newest dump, or pass a specific dump:

```bash
./scripts/restore-db.sh /opt/lexilink/backups/postgres/lexilink-YYYYMMDDTHHMMSSZ.dump
curl -fsS https://api.wordlope.com/health/ready
```

The restore script is destructive. It stops the API, terminates database
sessions, restores the dump with `pg_restore --clean --if-exists`, starts the
stack again, and waits for readiness.

For a non-production drill, copy the dump to a disposable host and run the same
compose stack there.

## Rollback

For an application-only rollback:

```bash
cd /opt/lexilink/app
git log --oneline -5
git checkout <known-good-commit>
./scripts/deploy.sh
curl -fsS https://api.wordlope.com/health/ready
```

If the bad deploy included an irreversible database migration, restore from the
latest known-good backup instead of only checking out older code.

## Firewall

Allow only SSH, HTTP, and HTTPS:

```bash
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status verbose
```

Keep Postgres private inside the Docker network. Do not publish port `5432`.

## SSH Hardening

Before changing SSH settings, confirm key-based login works in a second
terminal.

Edit `/etc/ssh/sshd_config`:

```text
PermitRootLogin no
PasswordAuthentication no
KbdInteractiveAuthentication no
PubkeyAuthentication yes
```

Validate and reload:

```bash
sudo sshd -t
sudo systemctl reload ssh
```

Keep an existing SSH session open until a new key-only session succeeds.

## Logs And Resource Limits

Compose sets json-file log rotation for `postgres`, `api`, and `caddy`.

Inspect current usage:

```bash
docker stats --no-stream
docker system df
```

Prune unused images after confirming the current deploy is healthy:

```bash
docker image prune
```

## Secrets Rotation

Secrets live in `/opt/lexilink/app/.env` and must not be committed.

Rotate JWT signing key:

```bash
openssl rand -base64 48
sudoedit /opt/lexilink/app/.env
./scripts/deploy.sh
```

Existing JWTs become invalid when the signing key changes; clients re-auth via
the guest token exchange.

Rotate Postgres password:

1. Take a backup.
2. Change the Postgres user's password inside the running DB.
3. Update `POSTGRES_PASSWORD` and `ConnectionStrings__LexiLinkDb` in `.env`.
4. Run `./scripts/deploy.sh`.
5. Verify `/health/ready`.

## Incident Commands

```bash
docker compose ps
docker compose logs --tail=200 api
docker compose logs --tail=200 postgres
docker compose restart api
curl -v https://api.wordlope.com/health/ready
```

If readiness fails after a deploy, keep the logs, roll back to the last known
good commit, and restore the database only when the failure is migration/data
related.
