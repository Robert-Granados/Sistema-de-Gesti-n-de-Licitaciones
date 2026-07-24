# Pequeña liberación — Iteración 1

## Objetivo de la demo

Validar con el cliente que la aplicación permite administrar proveedores sin
perder el historial y que los errores de negocio se presentan de forma
comprensible.

## Preparación

Desde la raíz del repositorio:

```powershell
docker compose up --build -d
docker compose ps
```

Los servicios `app` y `db` deben aparecer como `healthy`. La aplicación queda
disponible en <http://localhost:8080/Proveedores>.

## Guion de demostración

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Registrar `Compañía Demo, S.A.` | El proveedor se crea correctamente. |
| 2 | Intentar registrar `  compañia   demo, s.a.  ` | Se rechaza como duplicado. |
| 3 | Buscar `compañia` en el listado | El proveedor aparece en la página. |
| 4 | Abrir el detalle | Se muestran sus datos y el historial de ofertas. |
| 5 | Editar el nombre | El cambio se guarda y `UpdatedAt` se actualiza. |
| 6 | Simular una edición concurrente | Se responde HTTP 409 con un mensaje controlado. |
| 7 | Pulsar **Eliminar** | Se presenta un modal; cancelar no modifica datos. |
| 8 | Confirmar la eliminación | El proveedor deja de aparecer en el listado. |
| 9 | Revisar la base de datos | La fila tiene `deleted_at`; sus ofertas se conservan. |
| 10 | Intentar asociarle una oferta nueva | La operación se rechaza. |

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

- [✔] Pude registrar un proveedor válido.
- [✔] La aplicación rechazó nombres inválidos y duplicados.
- [✔] Pude buscar, ordenar y paginar proveedores.
- [✔] Pude consultar el detalle y las ofertas.
- [✔] Pude editar un proveedor.
- [✔] La aplicación solicitó confirmación antes de eliminar.
- [✔] El proveedor eliminado desapareció del listado.
- [✔] El historial permaneció disponible en la base de datos.
- [✔] Los mensajes fueron claros.
- [✔] Acepto la pequeña liberación de la Iteración 1.

Las observaciones y la decisión deben copiarse en
[`../bitacora-xp.md`](../bitacora-xp.md) antes de crear el tag de liberación.
