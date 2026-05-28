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
  "src/Modules/Diamond/Tests/LexiLink.Modules.Diamond.Tests.csproj"
  "src/Modules/Hint/Tests/LexiLink.Modules.Hint.Tests.csproj"
  "src/Modules/Undo/Tests/LexiLink.Modules.Undo.Tests.csproj"
  "src/Modules/Reset/Tests/LexiLink.Modules.Reset.Tests.csproj"
  "src/Modules/Administration/Tests/LexiLink.Modules.Administration.Tests.csproj"
  "src/Modules/Market/Tests/LexiLink.Modules.Market.Tests.csproj"
  "src/Modules/Payments/Tests/LexiLink.Modules.Payments.Tests.csproj"
  "src/Tests/ArchitectureTests/LexiLink.ArchitectureTests.csproj"
)

integration_projects=(
  "src/Modules/Games/IntegrationTests/LexiLink.Modules.Games.IntegrationTests.csproj"
  "src/Modules/Players/IntegrationTests/LexiLink.Modules.Players.IntegrationTests.csproj"
  "src/Modules/Stats/IntegrationTests/LexiLink.Modules.Stats.IntegrationTests.csproj"
  "src/Modules/Energy/IntegrationTests/LexiLink.Modules.Energy.IntegrationTests.csproj"
  "src/Modules/Quests/IntegrationTests/LexiLink.Modules.Quests.IntegrationTests.csproj"
  "src/Modules/Diamond/IntegrationTests/LexiLink.Modules.Diamond.IntegrationTests.csproj"
  "src/Modules/Hint/IntegrationTests/LexiLink.Modules.Hint.IntegrationTests.csproj"
  "src/Modules/Undo/IntegrationTests/LexiLink.Modules.Undo.IntegrationTests.csproj"
  "src/Modules/Reset/IntegrationTests/LexiLink.Modules.Reset.IntegrationTests.csproj"
  "src/Modules/Administration/IntegrationTests/LexiLink.Modules.Administration.IntegrationTests.csproj"
  "src/Modules/Market/IntegrationTests/LexiLink.Modules.Market.IntegrationTests.csproj"
  "src/Modules/Payments/IntegrationTests/LexiLink.Modules.Payments.IntegrationTests.csproj"
)

echo "Running DB-free test projects..."
for project in "${db_free_projects[@]}"; do
  dotnet test "$project" "${DOTNET_TEST_ARGS[@]}"
done

echo "Running integration test projects serially..."
for project in "${integration_projects[@]}"; do
  dotnet test "$project" "${DOTNET_TEST_ARGS[@]}"
done
