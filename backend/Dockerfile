# API ShopMaster (C# / .NET 10) — image multi-étapes
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Restauration des paquets d'abord : couche mise en cache tant que le .csproj ne change pas.
COPY src/ShopMaster.Api/ShopMaster.Api.csproj src/ShopMaster.Api/
RUN dotnet restore src/ShopMaster.Api/ShopMaster.Api.csproj

COPY src/ src/
RUN dotnet publish src/ShopMaster.Api/ShopMaster.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
# Npgsql charge libgssapi_krb5 à la connexion : sans elle, une fausse erreur apparaît dans les logs.
RUN apk add --no-cache krb5-libs
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=5 \
  CMD wget -qO- "http://127.0.0.1:${PORT:-8080}/health" > /dev/null || exit 1
ENTRYPOINT ["dotnet", "ShopMaster.Api.dll"]
