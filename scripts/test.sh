#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

DOTNET_TEST_ARGS=("$@" "--disable-build-servers" "-m:1")

db_free_projects=(
  "src/API/LexiLink.API.Tests/LexiLink.API.Tests.csproj"
  "src/Common/Tests/LexiLink.Common.Tests.csproj"
  "src/Modules/Games/Tests/LexiLink.Modules.Games.Tests.csproj"
  "src/Modules/Players/Tests/LexiLink.Modules.Players.Tests.csproj"
  "src/Modules/Energy/Tests/LexiLink.Modules.Energy.Tests.csproj"
  "src/Modules/Quests/Tests/LexiLink.Modules.Quests.Tests.csproj"
  "src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj"
)

integration_projects=(
  "src/Modules/Games/IntegrationTests/LexiLink.Modules.Games.IntegrationTests.csproj"
  "src/Modules/Players/IntegrationTests/LexiLink.Modules.Players.IntegrationTests.csproj"
  "src/Modules/Stats/IntegrationTests/LexiLink.Modules.Stats.IntegrationTests.csproj"
  "src/Modules/Energy/IntegrationTests/LexiLink.Modules.Energy.IntegrationTests.csproj"
  "src/Modules/Quests/IntegrationTests/LexiLink.Modules.Quests.IntegrationTests.csproj"
)

echo "Running DB-free test projects..."
for project in "${db_free_projects[@]}"; do
  dotnet test "$project" "${DOTNET_TEST_ARGS[@]}"
done

echo "Running integration test projects serially..."
for project in "${integration_projects[@]}"; do
  dotnet test "$project" "${DOTNET_TEST_ARGS[@]}"
done
