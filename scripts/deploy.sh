#!/usr/bin/env bash
# LexiLink production deploy — run ON THE SERVER, from the repo root.
#
# Prereqs:
#   - Docker Engine + Compose plugin installed
#   - a filled .env next to docker-compose.yml (cp .env.example .env, then set
#     LEXILINK_DOMAIN / LEXILINK_ACME_EMAIL / secrets / GuestDevice)
#
# Idempotent: re-run any time to rebuild + roll the stack. The migrator runs
# first (one-shot), then the API, then Caddy fronts it with automatic TLS.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

if [[ ! -f .env ]]; then
  echo "ERROR: .env not found in $ROOT_DIR"
  echo "  cp .env.example .env   # then edit: domain, email, secrets"
  exit 1
fi

echo "==> Building image and starting the stack (migrator runs first)..."
docker compose up -d --build

echo "==> Stack status:"
docker compose ps

echo "==> Waiting for the API /health/ready (up to ~2 min)..."
ok=""
for _ in $(seq 1 40); do
  if docker compose exec -T api curl -fsS http://localhost:8080/health/ready >/dev/null 2>&1; then
    ok=1; break
  fi
  sleep 3
done

if [[ -n "$ok" ]]; then
  domain="$(grep -E '^LEXILINK_DOMAIN=' .env | cut -d= -f2- | tr -d '\r')"
  echo "==> API is healthy."
  echo "    Public check: curl https://api.${domain}/health/ready"
  echo "    Next: seed content -> scripts/seed-content.sh docs/category-spor.json"
else
  echo "!! API did not become healthy in time. Recent logs:" >&2
  docker compose logs --tail=80 migrate api
  exit 1
fi
