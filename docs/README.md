# Documentación — Sistema de Gestión de Licitaciones

Índice principal de la documentación técnica y de producto.

## Arquitectura y contratos

- [Arquitectura general](arquitectura-general.md)
- [Modelo de datos](modelo-datos.md)
- [Integración de módulos](integracion-modulos.md)
- [API REST v1](api.md)
- [Colección reproducible de solicitudes](api-requests.http)

## Módulos y servicios

- [Proveedores](modulos/proveedores.md)
- [Licitaciones](modulos/licitaciones.md)
- [Ofertas](modulos/ofertas.md)
- [Niveles de aprobación](modulos/niveles-aprobacion.md)
- [Tipo de cambio](modulos/tipo-cambio.md)
- [Persistencia](modulos/persistencia.md)
- [Interfaz web](modulos/interfaz-web.md)
- [API REST](modulos/api-rest.md)

## Calidad, operación y entrega

- [Pruebas](pruebas.md)
- [Ejecución con Docker](docker.md)
- [Despliegue en Kubernetes](kubernetes.md)
- [Pipeline de CI/CD](ci-cd.md)
- [Uso responsable de IA](uso-ia.md)

## Producto y prácticas XP

- [Adaptación de XP para una persona](xp-individual.md)
- [Visión y alcance](vision-alcance.md)
- [Historias de usuario](historias-usuario.md)
- [Plan de liberación XP](plan-xp.md)
- [Bitácora XP](bitacora-xp.md)
- Pequeñas liberaciones: [Iteración 1](releases/iteracion-1.md), [Iteración 2](releases/iteracion-2.md), [Iteración 3](releases/iteracion-3.md) e [Iteración 4](releases/iteracion-4.md)

## Fuentes de implementación

- [`database_schema.sql`](../database_schema.sql): esquema relacional de referencia.
- [`docker-compose.yml`](../docker-compose.yml) y [`Dockerfile`](../Dockerfile): ejecución contenerizada.
- [`k8s/README.md`](../k8s/README.md): manifiestos y orden de despliegue.
