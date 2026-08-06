# Pequeña liberación — Iteración 3

## Objetivo de la demo

Validar con el cliente que la aplicación es usable de principio a fin: landing
page y navegación completa, modo claro/oscuro, validaciones junto al campo,
tablas paginadas, mensajes y confirmaciones; que el aprobador se resuelve según
el monto de la mejor oferta usando los niveles paramétricos; que los montos se
pueden alternar entre CRC/USD sin modificar datos; y que la API REST versionada
está documentada en Swagger con errores estandarizados.

## Preparación

Desde la raíz del repositorio:

```powershell
docker compose up --build -d
docker compose ps
```

Los servicios `app` y `db` deben aparecer como `healthy`. La aplicación queda
disponible en <http://localhost:8080> (página de inicio) y la documentación de
la API en <http://localhost:8080/swagger>.

## Guion de demostración

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Abrir <http://localhost:8080>. | Se muestra la landing page explicando el flujo de licitación, ofertas, mejor oferta, aprobación y conversión CRC/USD, con accesos directos a los módulos. |
| 2 | Recorrer el menú principal. | Inicio, Licitaciones, Proveedores, Ofertas, Niveles de aprobación, Tipo de cambio y Swagger están visibles y funcionan. |
| 3 | Alternar el modo claro/oscuro y recargar la página. | El tema cambia al instante y la preferencia se conserva entre sesiones. |
| 4 | Ir a Niveles de aprobación. | Se listan los tres rangos semilla (`Encargado de área`, `Gerencia`, `Junta Directiva`). |
| 5 | Intentar crear un rango que se traslapa con uno existente. | Se rechaza con un mensaje claro (HTTP 422 en la API). |
| 6 | Intentar crear un segundo rango abierto (sin monto máximo). | Se rechaza porque solo puede existir un rango abierto. |
| 7 | Crear un rango válido en un espacio libre y luego eliminarlo. | El nivel se crea y elimina con confirmación previa. |
| 8 | Abrir el detalle de una licitación con ofertas. | Se muestra la mejor oferta con su clasificación de ahorro y el aprobador resuelto según el monto. |
| 9 | Alternar la vista a USD en el detalle. | Presupuesto y ofertas se convierten con la tasa activa, mostrando la tasa y su fecha de vigencia; los montos almacenados no cambian. |
| 10 | Ir a Tipo de cambio y crear un valor `CRCporUSD` no positivo. | Se rechaza por regla de negocio. |
| 11 | Crear un tipo de cambio válido y activarlo. | El tipo queda activo, el anterior se desactiva automáticamente y solo existe un activo. |
| 12 | En un formulario, enviar un campo inválido. | El error se muestra justo al lado del campo, del lado cliente y servidor. |
| 13 | Usar las tablas de listados. | Paginan, filtran y ordenan de forma consistente entre módulos. |
| 14 | Pulsar **Eliminar** en cualquier registro. | Se presenta el modal de confirmación; cancelar no modifica datos. |
| 15 | Realizar una operación con éxito y otra que falle. | Se muestran notificaciones categorizadas (éxito, advertencia o error) con texto comprensible. |
| 16 | Abrir <http://localhost:8080/swagger>. | Swagger documenta los cinco recursos bajo `/api/v1` con CRUD y las acciones `publicar`, `cerrar` y `activar`. |
| 17 | Ejecutar `docs/api-requests.http` desde VS Code. | El CRUD y las acciones se ejecutan encadenando IDs automáticamente, incluidos casos de 400, 404, 409 y 422. |
| 18 | Provocar un error de negocio desde la API. | Se responde `ProblemDetails` con `errorCode`, `correlationId` y `X-Correlation-ID`, sin detalles técnicos internos. |

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

- [✔] Pude navegar por la landing page y el menú principal.
- [✔] Pude alternar modo claro/oscuro y se recordó mi preferencia.
- [✔] Pude administrar niveles de aprobación y la aplicación rechazó traslapes y un segundo rango abierto.
- [✔] El detalle de licitación mostró el aprobador según el monto de la mejor oferta.
- [✔] Pude alternar la vista CRC/USD sin modificar los montos almacenados.
- [✔] Pude administrar tipos de cambio con un único activo y la aplicación rechazó valores no positivos.
- [✔] Los formularios mostraron errores junto al campo.
- [✔] Las tablas permitieron paginar, filtrar y ordenar.
- [✔] Las eliminaciones pidieron confirmación y las operaciones mostraron mensajes claros.
- [✔] Pude consumir la API v1 documentada en Swagger.
- [✔] Los errores de la API siguieron un formato consistente sin exponer detalles técnicos.
- [✔] Los mensajes fueron claros.
- [✔] Acepto la pequeña liberación de la Iteración 3.

Las observaciones y la decisión deben copiarse en
[`../bitacora-xp.md`](../bitacora-xp.md) antes de crear el tag de liberación.
