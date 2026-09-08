# PomeloWeebHooks.API -- receptor de webhooks de Pomelo (:5090)
#
# El puerto y la ruta de salud coinciden a proposito con los de CaptaCard.API:
# el target group del balanceador comprueba /health y el grupo de seguridad
# abre app_port, asi que los dos servicios encajan en la misma maquinaria sin
# variantes por servicio.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PomeloWeebHooks.Core/PomeloWeebHooks.Core.csproj src/PomeloWeebHooks.Core/
COPY src/PomeloWeebHooks.Application/PomeloWeebHooks.Application.csproj src/PomeloWeebHooks.Application/
COPY src/PomeloWeebHooks.Infrastructure/PomeloWeebHooks.Infrastructure.csproj src/PomeloWeebHooks.Infrastructure/
COPY src/PomeloWeebHooks.API/PomeloWeebHooks.API.csproj src/PomeloWeebHooks.API/
RUN dotnet restore src/PomeloWeebHooks.API/PomeloWeebHooks.API.csproj

COPY src/PomeloWeebHooks.Core/ src/PomeloWeebHooks.Core/
COPY src/PomeloWeebHooks.Application/ src/PomeloWeebHooks.Application/
COPY src/PomeloWeebHooks.Infrastructure/ src/PomeloWeebHooks.Infrastructure/
COPY src/PomeloWeebHooks.API/ src/PomeloWeebHooks.API/

RUN dotnet publish src/PomeloWeebHooks.API/PomeloWeebHooks.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:5090
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 5090

HEALTHCHECK --interval=30s --timeout=5s --start-period=90s --retries=3 \
  CMD curl -fsS http://localhost:5090/health || exit 1

ENTRYPOINT ["dotnet", "PomeloWeebHooks.API.dll"]
