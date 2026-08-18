# Módulo de licitaciones

## Propósito y responsabilidades

Gestiona el ciclo de vida del proceso: crear borrador, listar/filtrar, consultar detalle, editar, publicar, cerrar y eliminar lógicamente. Calcula mejor oferta, ahorro, aprobador y visualización monetaria.

## Dependencias, entradas y salidas

- Entrada: controladores MVC/API y commands/queries bajo `Application/Licitaciones`.
- Depende de sus puertos de repositorio, `IClock`, niveles de aprobación, tipo de cambio y datos de ofertas/proveedores para el detalle.
- Salida: DTO de listado, edición y detalle; resultados de crear/publicar/cerrar/eliminar.
- Persistencia: `licitaciones`, con lecturas relacionadas de `ofertas` y `proveedores`.

## Reglas y errores

- Código único normalizado, título obligatorio, fecha de cierre futura y presupuesto CRC positivo.
- Transiciones válidas: Borrador → Publicada → Cerrada. Sólo el borrador es editable/eliminable según las reglas del caso de uso.
- El presupuesto no se reduce por debajo de ofertas existentes; cierre requiere motivo y vuelve inmutables las ofertas.
- La mejor oferta es el menor monto, con desempate por fecha; el aprobador proviene del rango configurado.
- Errores: duplicada (409), no encontrada (404), estado/cierre inválido (409), presupuesto insuficiente (422) y concurrencia (409).

## Pruebas

Las pruebas de dominio y `Application/Licitaciones` cubren transiciones, CRUD, publicación/cierre, cálculos, moneda y concurrencia. `LicitacionFlowTests` y `CrudCompletoTests` cubren el flujo web.
