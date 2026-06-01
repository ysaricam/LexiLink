#!/usr/bin/env bash
# Seed game content into the running production DB — run ON THE SERVER, from the
# repo root, after deploy.sh. Imports a category JSON via a one-off .NET SDK
# container attached to the compose network (no .NET install needed on the host).
#
# Usage:
#   scripts/seed-content.sh docs/category-spor.json
#   scripts/seed-content.sh docs/category-animals-en.json
#
# The prod DB starts EMPTY of content (schema only); import at least one
# category or players see an empty game. Re-running is idempotent (upsert).
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

JSON="${1:?usage: scripts/seed-content.sh <path/to/category.json>}"
[[ -f .env ]]  || { echo "ERROR: .env not found in $ROOT_DIR"; exit 1; }
[[ -f "$JSON" ]] || { echo "ERROR: category JSON not found: $JSON"; exit 1; }

# --env-file lets Docker parse the connection string (with ; and =) cleanly;
# the importer reads ConnectionStrings__LexiLinkDb from the container env.
docker run --rm --env-file .env \
  -v "$ROOT_DIR":/src -w /src \
  --network lexilink_default \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  sh -c "dotnet run --project src/Tools/LexiLink.Tools.CategoryImporter -- \"\$ConnectionStrings__LexiLinkDb\" \"$JSON\""
