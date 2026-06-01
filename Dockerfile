# syntax=docker/dockerfile:1
#
# LexiLink API + DbUp migrator — single runtime image.
#
# The image holds two .NET artifacts:
#   /app           -> the ASP.NET Core API host (default ENTRYPOINT)
#   /app/migrator  -> the DbUp console migrator (run as a one-shot in compose)
#
# The API csproj copies Database/Structure/**/*.sql into its publish output,
# so the SQL scripts live at /app/Database/Structure and are used by BOTH:
#   - the migrate one-shot:  dotnet /app/migrator/LexiLink.DatabaseMigrator.dll "<conn>" /app/Database/Structure
#   - the API /health/ready journal check (AppContext.BaseDirectory/Database/Structure)

# ---- Build stage ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Central build config first for restore-layer caching (CPM + shared props).
COPY LexiLink.sln Directory.Build.props Directory.Packages.props ./
COPY src/ ./src/

# Publish the two runtime artifacts. UseAppHost=false: we always invoke via
# `dotnet X.dll`, so no native apphost is needed.
RUN dotnet restore src/API/LexiLink.API/LexiLink.API.csproj \
 && dotnet publish src/API/LexiLink.API/LexiLink.API.csproj \
      -c Release -o /publish/api --no-restore -p:UseAppHost=false

RUN dotnet restore src/Database/LexiLink.DatabaseMigrator/LexiLink.DatabaseMigrator.csproj \
 && dotnet publish src/Database/LexiLink.DatabaseMigrator/LexiLink.DatabaseMigrator.csproj \
      -c Release -o /publish/migrator --no-restore -p:UseAppHost=false

# ---- Runtime stage --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is used by the compose `api` healthcheck (GET /health/live) and is handy
# for on-box debugging. Not present in the base image by default.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# API publish output (includes Database/Structure/**/*.sql via the csproj Content link).
COPY --from=build /publish/api ./
# DbUp migrator, invoked as a one-shot before the API on each deploy.
COPY --from=build /publish/migrator ./migrator/

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Default command runs the API host. The compose `migrate` service overrides
# the entrypoint to run the migrator instead.
ENTRYPOINT ["dotnet", "LexiLink.API.dll"]
