# Módulo de API REST

## Propósito y responsabilidades

Expone contratos JSON versionados para integrar clientes externos con licitaciones, proveedores, ofertas, niveles de aprobación y tipos de cambio.

## Dependencias, entradas y salidas

- Entrada: rutas `/api/v1`, JSON y query strings definidos en `Controllers` y `Contracts/ApiContracts.cs`.
- Depende de handlers/services de Application e Infrastructure mediante inyección.
- Salida: DTO JSON, códigos HTTP, `ProblemDetails`, Swagger y `/health`.
- No expone entidades de Domain ni accede directamente a `AppDbContext`.

## Reglas y errores

- `[ApiController]` valida contratos; acciones específicas son publicar, cerrar y activar tipo de cambio.
- `ApiExceptionMiddleware` clasifica errores en 400, 404, 409, 422 y 500, añade `X-Correlation-ID` y elimina detalles sensibles.
- Ediciones concurrentes de proveedor/licitación requieren `rowVersion`.
- El contrato completo y ejemplos están en [API](../api.md) y [solicitudes reproducibles](../api-requests.http).

## Pruebas

`ApiContractTests` verifica versión, recursos, acciones, Swagger y aislamiento del dominio. `ApiExceptionMiddlewareTests` verifica formato, correlación, concurrencia y seguridad de errores.
