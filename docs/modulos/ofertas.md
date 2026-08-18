# Módulo de ofertas

## Propósito y responsabilidades

Registra, lista, filtra, edita y elimina propuestas económicas de proveedores para licitaciones publicadas.

## Dependencias, entradas y salidas

- Entrada: controllers MVC/API; commands de registrar, editar y eliminar; query de listado.
- Depende de `IOfertaValidacionRepository`, `IOfertaWriteRepository`, `IOfertaReadRepository` e `IClock`.
- Salida: `RegistrarOfertaResult`, `OfertaListadoDto`, `EditarOfertaDto` y opciones para filtros web.
- Persistencia: tabla `ofertas`, vinculada con `licitaciones` y `proveedores`.

## Reglas y errores

- Monto positivo y no superior al presupuesto.
- La licitación debe existir, estar publicada, no vencida ni eliminada; el proveedor debe existir y estar activo.
- Un proveedor sólo presenta una oferta por licitación.
- Las ofertas de una licitación cerrada o vencida no se editan ni eliminan.
- Errores: oferta duplicada (409), recurso no encontrado (404) y licitación no disponible/estado inválido (409); datos incompatibles con reglas se rechazan antes de persistir.

## Pruebas

`Application/Ofertas`, integración de listado/edición/eliminación/duplicados/vencimiento y `OfertaFlowTests` cubren las reglas y los flujos principales.
