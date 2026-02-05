# ============================================================
# Arooba Marketplace - Multi-Stage Dockerfile
# ============================================================
# Build:   docker build -t arooba-api -f src/Arooba.API/Dockerfile .
# Run:     docker run -p 5000:8080 -p 5001:8081 arooba-api
# ============================================================

# ──────────────────────────────────────────────
# Stage 1: Restore dependencies
# ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

# Copy solution and project files first for layer caching
COPY Arooba.sln .
COPY Directory.Build.props .
COPY global.json .

# Copy all project files
COPY src/Arooba.Domain/Arooba.Domain.csproj src/Arooba.Domain/
COPY src/Arooba.Application/Arooba.Application.csproj src/Arooba.Application/
COPY src/Arooba.Infrastructure/Arooba.Infrastructure.csproj src/Arooba.Infrastructure/
COPY src/Arooba.API/Arooba.API.csproj src/Arooba.API/

# Copy test project files
COPY tests/Arooba.Domain.Tests/Arooba.Domain.Tests.csproj tests/Arooba.Domain.Tests/
COPY tests/Arooba.Application.Tests/Arooba.Application.Tests.csproj tests/Arooba.Application.Tests/
COPY tests/Arooba.API.Tests/Arooba.API.Tests.csproj tests/Arooba.API.Tests/

# Restore NuGet packages (cached if .csproj files unchanged)
RUN dotnet restore Arooba.sln

# ──────────────────────────────────────────────
# Stage 2: Build and publish
# ──────────────────────────────────────────────
FROM restore AS build
WORKDIR /src

# Copy all source code
COPY . .

# Build in Release mode
RUN dotnet build Arooba.sln -c Release --no-restore

# Publish the API project
RUN dotnet publish src/Arooba.API/Arooba.API.csproj \
    -c Release \
    --no-build \
    -o /app/publish \
    /p:UseAppHost=false

# ──────────────────────────────────────────────
# Stage 3: Test (optional, skippable via --target)
# ──────────────────────────────────────────────
FROM build AS test
WORKDIR /src
RUN dotnet test Arooba.sln \
    -c Release \
    --no-build \
    --verbosity normal \
    --results-directory /test-results \
    --logger "trx;LogFileName=test-results.trx"

# ──────────────────────────────────────────────
# Stage 4: Runtime
# ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN adduser --disabled-password --gecos "" --home /app --no-create-home appuser
USER appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose ports
# 8080 = HTTP, 8081 = HTTPS
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS="http://+:8080;https://+:8081"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=40s \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Arooba.API.dll"]
