#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

CONNECTION_STRING="${ConnectionStrings__LexiLinkDb:-Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852}"
API_PROJECT="src/API/LexiLink.API/LexiLink.API.csproj"
MIGRATOR_PROJECT="src/Database/LexiLink.DatabaseMigrator/LexiLink.DatabaseMigrator.csproj"
SCRIPTS_DIRECTORY="src/Database/LexiLink.Database/Structure"
PORT="${LEXILINK_SMOKE_PORT:-5089}"
BASE_URL="http://127.0.0.1:${PORT}"
LOG_FILE="${TMPDIR:-/tmp}/lexilink-smoke-${PORT}.log"

cleanup() {
  if [[ -n "${API_PID:-}" ]] && kill -0 "$API_PID" 2>/dev/null; then
    kill "$API_PID" 2>/dev/null || true
    wait "$API_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT

echo "Building API..."
dotnet build "$API_PROJECT" --no-restore --disable-build-servers -v minimal

echo "Applying DbUp migrations..."
dotnet run \
  --project "$MIGRATOR_PROJECT" \
  --no-restore \
  -- \
  "$CONNECTION_STRING" \
  "$SCRIPTS_DIRECTORY"

echo "Starting API on ${BASE_URL}..."
ASPNETCORE_ENVIRONMENT=Production \
ASPNETCORE_URLS="$BASE_URL" \
ConnectionStrings__LexiLinkDb="$CONNECTION_STRING" \
Authentication__Mode=ProductionJwt \
Authentication__Jwt__Issuer=LexiLink.Smoke \
Authentication__Jwt__Audience=LexiLink.Api.Smoke \
Authentication__Jwt__SigningKey=lexilink-smoke-test-signing-key-32 \
Authentication__TokenExchange__Mode=Disabled \
dotnet run \
  --project "$API_PROJECT" \
  --no-build \
  --no-restore \
  >"$LOG_FILE" 2>&1 &
API_PID=$!

wait_for_health() {
  local path="$1"
  local url="${BASE_URL}${path}"

  for _ in {1..30}; do
    if curl --fail --silent --show-error "$url" >/dev/null; then
      echo "${path} healthy"
      return 0
    fi

    if ! kill -0 "$API_PID" 2>/dev/null; then
      echo "API exited before ${path} became healthy. Logs:"
      sed -n '1,220p' "$LOG_FILE"
      return 1
    fi

    sleep 1
  done

  echo "${path} did not become healthy. Logs:"
  sed -n '1,220p' "$LOG_FILE"
  return 1
}

wait_for_health "/health/live"
wait_for_health "/health/ready"

echo "Smoke check passed."
