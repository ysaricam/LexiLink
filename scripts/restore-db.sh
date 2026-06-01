#!/usr/bin/env bash
# Restore a PostgreSQL custom-format backup into the running compose stack.
# Run ON THE SERVER, from anywhere inside the repo checkout.
#
# This is destructive: it stops the API, terminates DB sessions, restores with
# --clean/--if-exists, then starts the stack again.
#
# Usage:
#   scripts/restore-db.sh /opt/lexilink/backups/postgres/lexilink-YYYY...dump
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

DUMP="${1:?usage: scripts/restore-db.sh <backup.dump>}"
[[ -f .env ]] || { echo "ERROR: .env not found in $ROOT_DIR"; exit 1; }
[[ -f "$DUMP" ]] || { echo "ERROR: backup dump not found: $DUMP"; exit 1; }

echo "About to RESTORE database from:"
echo "  $DUMP"
echo
echo "This stops the API and overwrites the current database objects."
read -r -p "Type RESTORE to continue: " CONFIRM
if [[ "$CONFIRM" != "RESTORE" ]]; then
  echo "Aborted."
  exit 1
fi

echo "==> Stopping API while restoring..."
docker compose stop api

echo "==> Terminating database sessions..."
docker compose exec -T postgres sh -c "psql -U \"\$POSTGRES_USER\" -d postgres -v ON_ERROR_STOP=1 -v db=\"\$POSTGRES_DB\" -c \"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = :'db' AND pid <> pg_backend_pid();\""

echo "==> Restoring dump..."
docker compose exec -T postgres sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists --no-owner --no-privileges' < "$DUMP"

echo "==> Starting stack..."
docker compose up -d

echo "==> Waiting for API readiness..."
ok=""
for _ in $(seq 1 40); do
  if docker compose exec -T api curl -fsS http://localhost:8080/health/ready >/dev/null 2>&1; then
    ok=1; break
  fi
  sleep 3
done

if [[ -z "$ok" ]]; then
  echo "ERROR: API did not become ready after restore. Recent logs:" >&2
  docker compose logs --tail=120 api postgres
  exit 1
fi

echo "==> Restore complete and API is ready."
