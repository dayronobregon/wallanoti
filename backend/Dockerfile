FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto para restore (mejor cache)
COPY ["api/api.csproj", "api/"]
COPY ["src/src.csproj", "src/"]
RUN dotnet restore "api/api.csproj"

# Copiar el resto del código fuente
COPY . .

# Construir el proyecto
WORKDIR "/src/api"
RUN dotnet build "api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM base AS final
ARG APP_UID=1000

# Metadata
LABEL maintainer="wallanoti@gmail.com"
LABEL version="1.0"
LABEL description="Wallanoti API - Telegram bot for Wallapop notifications"

WORKDIR /app

# Copiar artifacts publicados
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .

# Asegurar que corremos como usuario no-root
USER $APP_UID

# Variables de entorno de producción
ENV DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Wallanoti.Api.dll"]
