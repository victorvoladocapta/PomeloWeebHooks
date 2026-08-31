# PomeloWeebHooks

Servicio independiente .NET 10 para recibir webhooks de Pomelo.

## Endpoint

`POST /api/webhooks/pomelo/cards/v1/cards/events`

`POST /api/webhooks/pomelo/credit-lines/status-changed`

`POST /api/webhooks/pomelo/statements/created-summaries`

`POST /api/webhooks/pomelo/lending/transactions`

`POST /api/webhooks/pomelo/reverted-operations`

`POST /api/webhooks/pomelo/credit-lines/delinquency`

Valida `X-Api-Key`, `X-Signature`, `X-Timestamp` y `X-Endpoint`. El servicio responde `200 OK` después de persistir el evento en PostgreSQL. Los reintentos se deduplican por `idempotency_key`.

## Configuración local

1. Copia `src/PomeloWeebHooks.API/appsettings.Development.example.json` como `appsettings.Development.json`.
2. Configura PostgreSQL, `Pomelo:ApiKey` y `Pomelo:ApiSecret`.
3. Ejecuta `dotnet run --project src/PomeloWeebHooks.API`.

Para producción, suministra secretos mediante variables de entorno:

- `ConnectionStrings__PostgreSql`
- `Pomelo__ApiKey`
- `Pomelo__ApiSecret`
- `Database__ApplyMigrationsOnStartup=true`

Documentación: https://developers.pomelo.la/api-reference/cards/issuing/card-events
