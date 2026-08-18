# Módulo de niveles de aprobación

## Propósito y responsabilidades

Parametriza quién aprueba una compra según el monto de la mejor oferta y reemplaza decisiones `if/else` fijas.

## Dependencias, entradas y salidas

- Entrada: controladores MVC/API y `NivelAprobacionService`.
- Persistencia: `INivelAprobacionRepository` sobre `niveles_aprobacion`.
- Consumidor: `ResolverAprobadorService` del detalle de licitaciones.
- Salida: `NivelAprobacionDto` con identificador, límites, aprobador y versión.

## Reglas y errores

- Mínimo no negativo; máximo nulo (rango abierto) o mayor al mínimo; aprobador obligatorio.
- Los rangos cerrados incluyen sus extremos, no se traslapan y sólo existe un rango abierto.
- La base refuerza la regla mediante `numrange`, exclusión GiST e índice parcial.
- Errores inexistentes retornan 404; rangos inválidos o traslapados se traducen a 422.

## Pruebas

`NivelAprobacionTests`, `NivelAprobacionServiceTests` y `ResolverAprobadorServiceTests` verifican límites, traslapes, CRUD y resolución del aprobador.
