FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Copiar csproj manteniendo la estructura de carpetas del repo
COPY ["Apps/Api/Api.csproj", "Apps/Api/"]
COPY ["WallapopNotification/WallapopNotification.csproj", "WallapopNotification/"]
RUN dotnet restore "Apps/Api/Api.csproj"
# Copiar todo el código fuente
COPY . .
# Cambiar al directorio correcto donde está Api.csproj
WORKDIR "/src/Apps/Api"
RUN dotnet build "Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wallanoti.Apps.Api.dll"]
