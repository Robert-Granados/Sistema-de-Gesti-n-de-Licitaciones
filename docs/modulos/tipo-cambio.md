# Módulo de tipo de cambio

## Propósito y responsabilidades

Administra tasas CRC por USD y permite alternar la presentación de montos sin modificar la fuente de verdad en CRC.

## Dependencias, entradas y salidas

- Entrada: controladores MVC/API y `TipoCambioService`.
- Persistencia: `ITipoCambioRepository` sobre `tipos_cambio`.
- Consumidor: `ConversionMonedaService` en el detalle de licitación.
- Salida: `TipoCambioDto` y valores USD calculados/redondeados para visualización.

## Reglas y errores

- `crcPorUsd` debe ser positivo y la fecha incluye zona horaria.
- Sólo un registro puede estar activo; activar uno desactiva el anterior, respaldado por trigger e índice parcial.
- CRC nunca se sobrescribe al mostrar USD.
- Datos inválidos producen 400/422 según su origen; identificadores inexistentes producen 404.

## Pruebas

`TipoCambioTests`, `TipoCambioServiceTests`, `ConversionMonedaTests` unitarias y de navegador cubren activación única, conversión y presentación.
