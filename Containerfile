# ==============================================================================
# Containerfile for PoolNet (.NET 10 ASP.NET Core MVC)
# ==============================================================================

# --- Stage 1: Build & Publish ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for NuGet package layer caching
COPY PoolNet.slnx ./
COPY src/PoolNet.Domain/PoolNet.Domain.csproj src/PoolNet.Domain/
COPY src/PoolNet.Application/PoolNet.Application.csproj src/PoolNet.Application/
COPY src/PoolNet.Infrastructure/PoolNet.Infrastructure.csproj src/PoolNet.Infrastructure/
COPY src/PoolNet.Web/PoolNet.Web.csproj src/PoolNet.Web/
COPY tests/PoolNet.UnitTests/PoolNet.UnitTests.csproj tests/PoolNet.UnitTests/

# Restore dependencies
RUN dotnet restore PoolNet.slnx

# Copy application source code
COPY src/ src/

# Build and publish in Release mode
WORKDIR /src/src/PoolNet.Web
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# --- Stage 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# ASP.NET Core settings:
# - Default port is 8080
# - Store SQLite database in /app/data for easy volume persistence
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/poolnet.db"

# Create /app/data directory and ensure ownership for non-root app user
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app

# Copy compiled files from build stage
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish ./

# Switch to non-root user (UID 1654 in .NET 8+)
USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "PoolNet.Web.dll"]
