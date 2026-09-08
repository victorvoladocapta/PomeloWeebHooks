# PomeloWeebHooks

Servicio independiente .NET 10 para recibir los webhooks con los que Pomelo
avisa de comportamientos de credito: eventos de tarjeta, cambios de estado de
linea, resumenes de estado de cuenta, transacciones, reversos y mora.

## Endpoints

```
POST /api/webhooks/pomelo/cards/v1/cards/events
POST /api/webhooks/pomelo/credit-lines/status-changed
POST /api/webhooks/pomelo/statements/created-summaries
POST /api/webhooks/pomelo/lending/transactions
POST /api/webhooks/pomelo/reverted-operations
POST /api/webhooks/pomelo/credit-lines/delinquency
POST /transactions/v1/notifications
POST /presentments/v1/notifications
POST /statements-opened
POST /interest
POST /identity/v1/users/status-changed
```

Cada uno valida `X-Api-Key`, `X-Signature`, `X-Timestamp` y `X-Endpoint`. El
servicio responde `200 OK` despues de persistir el evento en PostgreSQL, y los
reintentos se deduplican por `idempotency_key`.

`GET /health` es lo que comprueba el target group del balanceador.

## Configuracion local

1. Copia `src/PomeloWeebHooks.API/appsettings.Development.example.json` como
   `appsettings.Development.json`.
2. Configura PostgreSQL, `Pomelo:ApiKey` y `Pomelo:ApiSecret`.
3. Ejecuta `dotnet run --project src/PomeloWeebHooks.API`.

## En AWS

El servicio no recibe la contrasena de la base ni las credenciales de Pomelo:
recibe dos ARN de Secrets Manager y el rol de la instancia le da permiso de
lectura. Los valores existen solo dentro del proceso que los necesita, no en la
plantilla de lanzamiento, ni en la imagen, ni en el estado de Terraform, y
rotarlos no obliga a reconstruir nada.

Variables que inyecta la plantilla de lanzamiento:

```
DB_SECRET_ARN             credencial de PostgreSQL
DB_HOST                   endpoint del RDS Proxy
DB_NAME                   captacard
INTEGRATIONS_SECRET_ARN   Pomelo__ApiKey y Pomelo__ApiSecret
```

El esquema propio es `webhooks`, asi que el servicio comparte la base con
CaptaCard.API sin chocar con sus tablas.

`Database__ApplyMigrationsOnStartup=true` deja que cada instancia migre al
arrancar. Con una sola instancia no hay problema; antes de subir a dos hay que
revisarlo, porque dos procesos migrando la misma base compiten por el bloqueo
de EF y el segundo puede agotar el tiempo del lifecycle hook del ASG.

Los eventos adicionales conservan el JSON completo en
`webhooks.pomelo_inbound_event`, además de columnas indexadas para el tipo,
recurso, usuario y estado. La idempotencia es única por tipo de webhook.

Documentación de Pomelo:

- https://developers.pomelo.la/api-reference/cards/issuing/card-events
- https://developers.pomelo.la/api-reference/processing/transactions/transaction-notifications
- https://developers.pomelo.la/api-reference/processing/transactions/presentments-notifications
- https://developers.pomelo.la/api-reference/core-credit/credit-lines/notifications-of-opened-statements
- https://developers.pomelo.la/api-reference/core-credit/credit-lines/notifications-of-accrued-interest-and-charges
- https://developers.pomelo.la/api-reference/identity/users-webhooks/user-status-change-notifications
