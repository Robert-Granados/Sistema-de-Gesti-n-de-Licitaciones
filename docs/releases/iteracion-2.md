# Pequeña liberación — Iteración 2

## Objetivo de la demo

Validar con el cliente que la aplicación permite ejecutar el flujo funcional
mínimo del negocio: crear, publicar y cerrar licitaciones; registrar ofertas
(válidas y rechazadas); administrarlas; y ver automáticamente la mejor oferta
con su clasificación de ahorro.

## Preparación

Desde la raíz del repositorio:

```powershell
docker compose up --build -d
docker compose ps
```

Los servicios `app` y `db` deben aparecer como `healthy`. La aplicación queda
disponible en <http://localhost:8080/Licitaciones>.

## Guion de demostración

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Crear `LIC-2026-001` con presupuesto `1.000.000` CRC y fecha de cierre futura. | La licitación se crea en estado Borrador. |
| 2 | Intentar crear `LIC-2026-001` de nuevo. | Se rechaza como duplicada (HTTP 409). |
| 3 | Publicar `LIC-2026-001`. | El estado cambia a Publicada y queda registrada la fecha de publicación. |
| 4 | En el detalle, registrar una oferta de `Proveedor A` por `850.000` CRC. | La oferta se registra correctamente. |
| 5 | Registrar la misma oferta para el mismo proveedor. | Se rechaza como duplicada. |
| 6 | Registrar una oferta de `1.100.000` CRC. | Se rechaza por superar el presupuesto. |
| 7 | Registrar una oferta de `900.000` CRC para `Proveedor B`. | La oferta se registra correctamente. |
| 8 | Abrir el detalle de `LIC-2026-001`. | Se muestra la mejor oferta (`850.000`, clasificación "Oferta conveniente" por ahorro ≥10%) y el aprobador. |
| 9 | Crear y publicar `LIC-2026-002`. | La segunda licitación queda Publicada. |
| 10 | Registrar una oferta de `950.000` CRC en `LIC-2026-002`. | La oferta se registra correctamente. |
| 11 | Cerrar `LIC-2026-001` indicando un motivo. | El estado cambia a Cerrada. |
| 12 | Intentar registrar una oferta en `LIC-2026-001`. | Se rechaza por licitación cerrada. |
| 13 | Listar ofertas en `/Ofertas` y filtrar por licitación o proveedor. | El filtro devuelve solo las ofertas esperadas. |
| 14 | Editar el monto de la oferta de `LIC-2026-002` a `970.000`. | Se actualiza y la mejor oferta se recalcula (clasificación "Oferta aceptable" por ahorro entre 0% y 10%). |
| 15 | Intentar editar una oferta de `LIC-2026-001` (cerrada). | Se rechaza sin modificar datos. |
| 16 | Pulsar **Eliminar** en una oferta de `LIC-2026-002`. | Se solicita confirmación; confirmar elimina la oferta. |
| 17 | Editar el título de `LIC-2026-002`. | El cambio se guarda y el historial se conserva. |
| 18 | Eliminar `LIC-2026-001` (que tiene ofertas). | Se elimina lógicamente y su historial de ofertas se conserva. |

## Comprobación automatizada

```powershell
dotnet restore SistemaLicitaciones.sln
dotnet build SistemaLicitaciones.sln --configuration Release --no-restore
dotnet test SistemaLicitaciones.sln --configuration Release --no-build
```

Si la máquina no dispone del SDK .NET 9, puede ejecutarse con Docker:

```powershell
docker run --rm -v "${PWD}:/src" -w /src `
  mcr.microsoft.com/dotnet/sdk:9.0 `
  dotnet test SistemaLicitaciones.sln --configuration Release
```

## Lista de aceptación del cliente

- [✔] Pude crear una licitación y la aplicación rechazó códigos duplicados.
- [✔] Pude publicar, editar, cerrar y eliminar (lógicamente) licitaciones.
- [✔] Pude consultar el detalle con la mejor oferta, su clasificación de ahorro y el aprobador.
- [✔] Pude registrar ofertas válidas.
- [✔] La aplicación rechazó ofertas duplicadas, que exceden el presupuesto y de licitaciones vencidas o cerradas.
- [✔] Pude listar y filtrar ofertas por licitación y proveedor.
- [✔] Pude editar y eliminar ofertas con confirmación.
- [✔] La mejor oferta se recalcula automáticamente y se resalta en el detalle.
- [✔] Los mensajes fueron claros.
- [✔] Acepto la pequeña liberación de la Iteración 2.

Las observaciones y la decisión deben copiarse en
[`../bitacora-xp.md`](../bitacora-xp.md) antes de crear el tag de liberación.
