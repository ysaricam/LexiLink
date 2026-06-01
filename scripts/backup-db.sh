#!/usr/bin/env bash
# Create a PostgreSQL custom-format backup from the running compose stack.
# Run ON THE SERVER, from anywhere inside the repo checkout.
#
# Defaults:
#   backup dir: /opt/lexilink/backups/postgres
#   retention: 14 days
#
# Usage:
#   scripts/backup-db.sh
#   BACKUP_DIR=/mnt/backups RETENTION_DAYS=30 scripts/backup-db.sh
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

[[ -f .env ]] || { echo "ERROR: .env not found in $ROOT_DIR"; exit 1; }

BACKUP_DIR="${BACKUP_DIR:-/opt/lexilink/backups/postgres}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"
STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
OUT="${BACKUP_DIR}/lexilink-${STAMP}.dump"

mkdir -p "$BACKUP_DIR"
chmod 700 "$BACKUP_DIR"

echo "==> Writing PostgreSQL backup: $OUT"
docker compose exec -T postgres sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc' > "$OUT"
chmod 600 "$OUT"

if command -v sha256sum >/dev/null 2>&1; then
  sha256sum "$OUT" > "${OUT}.sha256"
else
  shasum -a 256 "$OUT" > "${OUT}.sha256"
fi
chmod 600 "${OUT}.sha256"

echo "==> Backup size:"
du -h "$OUT"

echo "==> Verifying backup format..."
docker compose exec -T postgres sh -c 'pg_restore --list >/dev/null' < "$OUT"

echo "==> Pruning backups older than ${RETENTION_DAYS} days in $BACKUP_DIR"
find "$BACKUP_DIR" -type f \( -name 'lexilink-*.dump' -o -name 'lexilink-*.dump.sha256' \) -mtime +"$RETENTION_DAYS" -delete

echo "==> Backup complete."
echo "    Restore drill: scripts/restore-db.sh $OUT"
