#!/usr/bin/env bash
# LexiLink production deploy — run ON THE SERVER, from the repo root.
#
# Prereqs:
#   - Docker Engine + Compose plugin installed
#   - a filled .env next to docker-compose.yml (cp .env.example .env, then set
#     LEXILINK_DOMAIN / LEXILINK_ACME_EMAIL / secrets / GuestDevice)
#
# Idempotent: re-run any time to roll the stack. Default mode builds locally.
# CI/CD can pass LEXILINK_IMAGE=ghcr.io/<owner>/lexilink:<tag> to pull a
# prebuilt image instead. The migrator runs first (one-shot), then the API,
# then Caddy fronts it with automatic TLS.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

if [[ ! -f .env ]]; then
  echo "ERROR: .env not found in $ROOT_DIR"
  echo "  cp .env.example .env   # then edit: domain, email, secrets"
  exit 1
fi

image="${LEXILINK_IMAGE:-}"
if [[ -z "$image" ]]; then
  image="$(grep -E '^LEXILINK_IMAGE=' .env | cut -d= -f2- | tr -d '\r' || true)"
fi
image="${image:-lexilink:local}"
export LEXILINK_IMAGE="$image"

admin_web_root="$(grep -E '^ADMIN_WEB_ROOT=' .env | cut -d= -f2- | tr -d '\r' || true)"
admin_web_root="${admin_web_root:-./frontend/build/web}"
if [[ "$admin_web_root" != /* ]]; then
  admin_web_root="$ROOT_DIR/${admin_web_root#./}"
fi

public_web_root="$(grep -E '^PUBLIC_WEB_ROOT=' .env | cut -d= -f2- | tr -d '\r' || true)"
public_web_root="${public_web_root:-./public-site}"
if [[ "$public_web_root" != /* ]]; then
  public_web_root="$ROOT_DIR/${public_web_root#./}"
fi

for required_public_file in index.html support/index.html privacy/index.html; do
  if [[ ! -f "$public_web_root/$required_public_file" ]]; then
    echo "ERROR: public site file not found at $public_web_root/$required_public_file" >&2
    echo "  PUBLIC_WEB_ROOT must point to the directory served at https://wordlope.com." >&2
    exit 1
  fi
done

if [[ ! -f "$admin_web_root/index.html" ]]; then
  echo "ERROR: admin web build not found at $admin_web_root/index.html" >&2
  echo "  Build and copy the Flutter web admin output before deploy:" >&2
  echo "    cd frontend && flutter build web --release --dart-define=LEXILINK_API_BASE_URL=https://api.wordlope.com --dart-define=LEXILINK_ENABLE_ADMIN=true" >&2
  echo "  Then set ADMIN_WEB_ROOT in .env to the directory that contains index.html." >&2
  exit 1
fi

if [[ "$image" == "lexilink:local" ]]; then
  echo "==> Building local image and starting the stack (migrator runs first)..."
  docker compose up -d --build
else
  echo "==> Pulling image ${image} and starting the stack (migrator runs first)..."
  docker compose pull api migrate
  docker compose up -d --no-build
fi

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
  echo "    Public site: https://${domain}/support and https://${domain}/privacy"
  echo "    Admin console: https://admin.${domain}/admin/login"
  echo "    Next: seed content -> scripts/seed-content.sh docs/category-spor.json"
else
  echo "!! API did not become healthy in time. Recent logs:" >&2
  docker compose logs --tail=80 migrate api
  exit 1
fi
